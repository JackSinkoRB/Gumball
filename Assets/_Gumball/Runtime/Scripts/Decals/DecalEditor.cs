using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyBox;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalEditor : Singleton<DecalEditor>
    {
        
        /// <summary>
        /// Is true if tests have started running, and the runtime hasn't yet started
        /// </summary>
        public static bool IsRunningTests;

        public const int MaxDecalsAllowed = 50;

        public static event Action onSessionStart;
        public static event Action onSessionEnd;
        
        public static event Action<LiveDecal> onSelectLiveDecal;
        public static event Action<LiveDecal> onDeselectLiveDecal;
        public static event Action<LiveDecal> onCreateLiveDecal;
        public static event Action<LiveDecal> onDestroyLiveDecal;
        
        //these must be static so they are still alive when the application is quitting
        private static CarManager currentCar;
        private static List<LiveDecal> liveDecals = new();

        [SerializeField] private SelectedDecalUI selectedLiveDecalUI;
        [SerializeField] private DecalCameraController cameraController;

        [Header("Colours")]
        [SerializeField] private Color[] colorPalette;
        [SerializeField] private int numberOfGrayscaleColors = 10;
        [SerializeField] private int numberOfRainbowColours = 50;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private LiveDecal currentSelected;
        [SerializeField, ReadOnly] private List<PaintableMesh> paintableMeshes = new();
        
        public List<LiveDecal> LiveDecals => liveDecals;
        public LiveDecal CurrentSelected => currentSelected;
        public CarManager CurrentCar => currentCar;
        public SelectedDecalUI SelectedDecalUI => selectedLiveDecalUI;
        public Color[] ColorPalette => colorPalette;

        private readonly RaycastHit[] decalsUnderPointer = new RaycastHit[MaxDecalsAllowed];

        private Coroutine disablePaintableMeshesCoroutine;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialise()
        {
            IsRunningTests = false;
            
            onSessionStart = null;
            onSessionEnd = null;
            onSelectLiveDecal = null;
            onDeselectLiveDecal = null;
            onCreateLiveDecal = null;
            onDestroyLiveDecal = null;

            currentCar = null;
            liveDecals.Clear();
        }
        
        public static void LoadEditor()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadEditorIE());
        }
        
        private static IEnumerator LoadEditorIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = Stopwatch.StartNew();
            yield return Addressables.LoadSceneAsync(SceneManager.DecalEditorSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.DecalEditorSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");

            CarManager car = PlayerCarManager.Instance.CurrentCar;
            
            //move the vehicle to the right position
            car.Rigidbody.Move(Vector3.zero, Quaternion.Euler(Vector3.zero));
            
            Instance.cameraController.gameObject.SetActive(true);
            Instance.StartSession(car);
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }

        private void Update()
        {
            bool canFade = PrimaryContactInput.IsPressed && currentSelected != null && currentSelected.IsValidPosition;
            selectedLiveDecalUI.Fade(canFade);
        }

        private void LateUpdate()
        {
            selectedLiveDecalUI.UpdatePosition();
        }
        
        private void OnPrimaryContactReleased()
        {
            if (!PanelManager.PanelExists<DecalEditorPanel>())
                return; //editor panel isn't open
            
            bool pointerWasPressed = PrimaryContactInput.OffsetSincePressedNormalised.Approximately(Vector2.zero, PrimaryContactInput.PressedThreshold);
            if (!pointerWasPressed)
                return;

            if (PrimaryContactInput.IsGraphicUnderPointer(PanelManager.GetPanel<DecalEditorPanel>().TrashButton.image)
                || PrimaryContactInput.IsGraphicUnderPointer(PanelManager.GetPanel<DecalEditorPanel>().ColourButton.image)
                || PrimaryContactInput.IsGraphicUnderPointer(PanelManager.GetPanel<DecalEditorPanel>().SendForwardButton.image)
                || PrimaryContactInput.IsGraphicUnderPointer(PanelManager.GetPanel<DecalEditorPanel>().SendBackwardButton.image)
                || PrimaryContactInput.IsGraphicUnderPointer(PanelManager.GetPanel<DecalColourSelectorPanel>().MagneticScroll.GetComponent<Image>()))
                return;

            UpdateDecalUnderPointer();
        }

        public void StartSession(CarManager car)
        {
            PrimaryContactInput.onRelease += OnPrimaryContactReleased;
            DataProvider.onBeforeSaveAllDataOnAppExit += OnBeforeSaveAllDataOnAppExit;
            
            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.General);

            AvatarManager.Instance.HideAvatars(true);

            currentCar = car;
            
            if (disablePaintableMeshesCoroutine != null)
                CoroutineHelper.Instance.StopCoroutine(disablePaintableMeshesCoroutine);
            
            //set the vehicle kinematic
            currentCar.Rigidbody.isKinematic = true;
            
            liveDecals = DecalManager.CreateLiveDecalsFromData(car);
            this.PerformAtEndOfFrame(DeselectLiveDecal); //perform at end of frame as magnetic scroll will select it in LateUpdate()
            
            GlobalLoggers.DecalsLogger.Log($"Starting session for {car.gameObject.name} with {liveDecals.Count} saved decals.");

            paintableMeshes.Clear();
            foreach (PaintableMesh paintableMesh in car.transform.GetComponentsInAllChildren<PaintableMesh>())
            {
                paintableMeshes.Add(paintableMesh);
                paintableMesh.EnablePainting();
            }
            
            //disable the car's collider temporarily
            PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(false);
            
            onSessionStart?.Invoke();
        }

        public void EndSession()
        {
            GlobalLoggers.DecalsLogger.Log($"Ending session.");

            PrimaryContactInput.onRelease -= OnPrimaryContactReleased;
            DataProvider.onBeforeSaveAllDataOnAppExit -= OnBeforeSaveAllDataOnAppExit;

            DeselectLiveDecal();
            
            AvatarManager.Instance.HideAvatars(false);

            DecalManager.SaveLiveDecalData(currentCar, liveDecals);

            foreach (LiveDecal liveDecal in liveDecals)
            {
                liveDecal.Apply();
                Destroy(liveDecal.gameObject);
            }
            
            liveDecals.Clear();

            if (PlayerCarManager.ExistsRuntime)
                PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(true);
            
            DecalStateManager.ClearHistory();
            
            onSessionEnd?.Invoke();

            SessionCleanup();
        }

        /// <summary>
        /// Searches through the decals to find the priority of the decal with the highest priority.
        /// </summary>
        public int GetHighestPriority()
        {
            int highestPriority = 0;
            foreach (LiveDecal liveDecal in liveDecals)
            {
                if (liveDecal.Priority > highestPriority)
                    highestPriority = liveDecal.Priority;
            }

            return highestPriority;
        }
        
        public LiveDecal CreateLiveDecal(DecalUICategory category, DecalTexture decalTexture)
        {
            LiveDecal liveDecal = DecalManager.CreateLiveDecal(category, decalTexture, GetHighestPriority() + 1);
            
            liveDecals.Add(liveDecal);
            OnLiveDecalsListChanged();
            
            onCreateLiveDecal?.Invoke(liveDecal);

            return liveDecal;
        }

        public LiveDecal CreateLiveDecalFromData(LiveDecal.LiveDecalData data)
        {
            LiveDecal liveDecal = DecalManager.CreateLiveDecalFromData(data);

            liveDecals.Insert(data.Priority - 1, liveDecal);
            OnLiveDecalsListChanged();

            onCreateLiveDecal?.Invoke(liveDecal);

            return liveDecal;
        }
        
        public void SelectLiveDecal(LiveDecal liveDecal)
        {
            if (currentSelected != null && currentSelected == liveDecal)
                return; //already selected
            
            currentSelected = liveDecal;
            liveDecal.OnSelect();
            onSelectLiveDecal?.Invoke(liveDecal);
        }

        public void DeselectLiveDecal()
        {
            if (currentSelected == null)
                return; //nothing selected
            
            currentSelected.OnDeselect();
            onDeselectLiveDecal?.Invoke(currentSelected);
            currentSelected = null;
        }

        public void DisableLiveDecal(LiveDecal liveDecal)
        {
            if (currentSelected != null && currentSelected == liveDecal)
                DeselectLiveDecal();

            liveDecals.Remove(liveDecal);
            OnLiveDecalsListChanged();

            onDestroyLiveDecal?.Invoke(liveDecal);

            liveDecal.gameObject.Pool();
        }
        
        public void OrderDecalsListByPriority()
        {
            liveDecals = liveDecals.OrderBy(liveDecal => liveDecal.Priority).ToList();
        }
        
        private void UpdateDecalUnderPointer()
        {
            //raycast from the pointer position into the world
            Ray ray = Camera.main.ScreenPointToRay(PrimaryContactInput.Position);

            //max raycast distance from the camera to the middle of the car, so it doesn't detect decals on the other side
            float maxRaycastDistance = Vector3.Distance(Camera.main.transform.position, currentCar.transform.position);
            int raycastHits = Physics.RaycastNonAlloc(ray, decalsUnderPointer, maxRaycastDistance, LayersAndTags.GetLayerMaskFromLayer(LayersAndTags.Layer.LiveDecal));

            LiveDecal closestDecal = null;
            float distanceToClosestDecalSqr = Mathf.Infinity;
            
            for (int index = 0; index < raycastHits; index++)
            {
                RaycastHit hit = decalsUnderPointer[index];
                LiveDecal decal = hit.transform.parent.GetComponent<LiveDecal>();

                if (decal == currentSelected)
                {
                    //if clicking the already-selected decal, don't select any higher priority ones, so it can be moved etc.
                    closestDecal = decal;
                    break;
                }

                Vector2 clickScreenPos = PrimaryContactInput.Position;
                Vector2 centreOfDecalScreenPos = Camera.main.WorldToScreenPoint(decal.transform.position);
                
                float distanceToDecalSqr = Vector2.SqrMagnitude(clickScreenPos - centreOfDecalScreenPos);
                if (closestDecal == null || distanceToDecalSqr < distanceToClosestDecalSqr)
                {
                    closestDecal = decal;
                    distanceToClosestDecalSqr = distanceToDecalSqr;
                }

                GlobalLoggers.DecalsLogger.Log($"Distance to {decal.Priority} = {distanceToDecalSqr}");
            }

            if (closestDecal != null)
            {
                bool currentSelectedHasChanged = currentSelected == null || currentSelected != closestDecal;
                
                if (currentSelectedHasChanged
                    && (!PrimaryContactInput.IsGraphicUnderPointer(selectedLiveDecalUI.Ring) || !PrimaryContactInput.HasDraggedSincePressing))
                {
                    DeselectLiveDecal();
                    SelectLiveDecal(closestDecal);
                }
            }
            else
            {
                if (!PrimaryContactInput.IsGraphicUnderPointer(selectedLiveDecalUI.Ring))
                    DeselectLiveDecal();
            }
        }
        
        private void SessionCleanup()
        {
            //need to wait for the texture to fully apply before removing paintable components
            disablePaintableMeshesCoroutine = CoroutineHelper.PerformAtEndOfFrame(() =>
            {
                for (int i = paintableMeshes.Count - 1; i >= 0; i--)
                {
                    PaintableMesh paintableMesh = paintableMeshes[i];
                    paintableMesh.DisablePainting();
                    paintableMeshes.Remove(paintableMesh);
                }
                
                currentCar.Rigidbody.isKinematic = false;
                currentCar = null;
            });
        }
        
        private void OnBeforeSaveAllDataOnAppExit()
        {
#if UNITY_EDITOR
            if (!IsRunningTests) //don't run save if running tests
#endif
            {
                if (currentCar == null) //there will only be a car if a session was started
                    return;
                
                //save the decal data before app is closed if closing during a session
                DecalManager.SaveLiveDecalData(currentCar, liveDecals);
            }
        }

        private void OnLiveDecalsListChanged()
        {
            //reassign the priorities based on order in the list
            for (int index = 0; index < liveDecals.Count; index++)
            {
                LiveDecal liveDecal = liveDecals[index];
                liveDecal.SetPriority(index + 1); 
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Use this method to generate an array of colours that can be used as a colour pallete.
        /// </summary>
        [ButtonMethod]
        public void GenerateColours()
        {
            List<Color> colors = new List<Color>();

            for (int count = 0; count <= numberOfGrayscaleColors; count++)
            {
                float lerpFactor = (float)count / numberOfGrayscaleColors;
                Color shade = Color.Lerp(Color.black, Color.white, lerpFactor);
                colors.Add(shade);
            }
            
            for (int count = 0; count < numberOfRainbowColours; count++)
            {
                float hue = (float)count / numberOfRainbowColours;
                Color color = Color.HSVToRGB(hue, 1f, 1f);
                colors.Add(color);
            }

            colorPalette = colors.ToArray();
            EditorUtility.SetDirty(this);
        }
#endif
        
    }
}
