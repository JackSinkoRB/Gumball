using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using MyBox;
using PaintIn3D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Gumball
{
    public class DecalEditor : Singleton<DecalEditor>
    {

        public const int MaxDecalsAllowed = 50;
        
        private const int decalLayer = 6;
        private static readonly LayerMask decalLayerMask = 1 << decalLayer;
        private static readonly int AlbedoShaderID = Shader.PropertyToID("_Albedo");

        public static event Action onSessionStart;
        public static event Action onSessionEnd;
        
        public event Action<LiveDecal> onSelectLiveDecal;
        public event Action<LiveDecal> onDeselectLiveDecal;
        public event Action<LiveDecal> onCreateLiveDecal;
        public event Action<LiveDecal> onDestroyLiveDecal;
        
        [Tooltip("The shader that the car body uses. The decal will only be applied to the materials using this shader.")]
        [SerializeField] private Shader carBodyShader;
        [SerializeField] private CarManager currentCar;
        [SerializeField] private SelectedDecalUI selectedLiveDecalUI;
        [SerializeField] private DecalCameraController cameraController;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private LiveDecal currentSelected;
        [SerializeField, ReadOnly] private int priorityCount;
        [SerializeField, ReadOnly] private List<PaintableMesh> paintableMeshes = new();
        [Tooltip("Index is the priority")]
        [SerializeField, ReadOnly] private List<LiveDecal> liveDecals = new();
        
        public List<LiveDecal> LiveDecals => liveDecals;
        public LiveDecal CurrentSelected => currentSelected;
        public CarManager CurrentCar => currentCar;
        
        private readonly RaycastHit[] decalsUnderPointer = new RaycastHit[MaxDecalsAllowed];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialise()
        {
            onSessionStart = null;
            onSessionEnd = null;
        }
        
        public static void LoadDecalEditor()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadDecalEditorIE());
        }
        
        private static IEnumerator LoadDecalEditorIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();

            Stopwatch sceneLoadingStopwatch = new Stopwatch();
            sceneLoadingStopwatch.Start();
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

        private void OnPrimaryContactPressed()
        {
            if (!PanelManager.PanelExists<DecalEditorPanel>())
                return; //editor panel isn't open
            
            Image layerSelectorImage = PanelManager.GetPanel<DecalEditorPanel>().LayerSelector.MagneticScroll.GetComponent<Image>();
            Image scaleRotationHandleImage = selectedLiveDecalUI.ScaleRotationHandle.Button.image;
            
            if (!PrimaryContactInput.IsClickableUnderPointer(scaleRotationHandleImage)
                && !PrimaryContactInput.IsClickableUnderPointer(layerSelectorImage))
            {
                UpdateDecalUnderPointer();
            }
        }

        public void StartSession(CarManager car)
        {
            PrimaryContactInput.onPress += OnPrimaryContactPressed;

            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.General);

            currentCar = car;
            
            //set the vehicle kinematic
            currentCar.Rigidbody.isKinematic = true;
            
            liveDecals = DecalManager.CreateLiveDecalsFromData(car);
            this.PerformAtEndOfFrame(DeselectLiveDecal);
            
            GlobalLoggers.DecalsLogger.Log($"Starting session for {car.gameObject.name} with {liveDecals.Count} saved decals.");

            paintableMeshes.Clear();
            foreach (MeshFilter meshFilter in car.transform.GetComponentsInAllChildren<MeshFilter>())
            {
                SetMeshPaintable(meshFilter);
            }
            
            //disable the car's collider temporarily
            PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(false);

            onSessionStart?.Invoke();
        }

        public void EndSession()
        {
            GlobalLoggers.DecalsLogger.Log($"Ending session.");

            PrimaryContactInput.onPress -= OnPrimaryContactPressed;

            DeselectLiveDecal();
            
            DecalManager.SaveLiveDecalData(currentCar, liveDecals);

            foreach (LiveDecal liveDecal in liveDecals)
            {
                liveDecal.Apply();
                Destroy(liveDecal.gameObject);
            }
            
            liveDecals.Clear();
            priorityCount = 0;
            
            //need to wait for the texture to fully apply before removing paintable components
            this.PerformAtEndOfFrame(() =>
            {
                for (int i = paintableMeshes.Count - 1; i >= 0; i--)
                {
                    PaintableMesh paintableMesh = paintableMeshes[i];
                    RemoveMeshPaintable(paintableMesh);
                    paintableMeshes.Remove(paintableMesh);
                }
            });

            if (PlayerCarManager.ExistsRuntime)
                PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(true);
            
            currentCar.Rigidbody.isKinematic = false;
            
            onSessionEnd?.Invoke();

            currentCar = null;
        }

        public LiveDecal CreateLiveDecal(DecalUICategory category, Sprite sprite)
        {
            LiveDecal liveDecal = DecalManager.CreateLiveDecal(category, sprite, priorityCount);
            priorityCount++;
            
            liveDecals.Add(liveDecal);
            
            onCreateLiveDecal?.Invoke(liveDecal);

            return liveDecal;
        }

        public LiveDecal GetLiveDecalByPriority(int priority)
        {
            return liveDecals[priority];
        }

        public int GetPriorityOfLiveDecal(LiveDecal liveDecal)
        {
            return liveDecals.IndexOf(liveDecal);
        }

        public void SelectLiveDecal(LiveDecal liveDecal)
        {
            currentSelected = liveDecal;
            onSelectLiveDecal?.Invoke(liveDecal);
        }

        public void DeselectLiveDecal()
        {
            if (currentSelected == null)
                return; //nothing selected
            
            onDeselectLiveDecal?.Invoke(currentSelected);
            currentSelected = null;
        }

        public void DestroyLiveDecal(LiveDecal liveDecal)
        {
            liveDecals.Remove(liveDecal);
            
            onDestroyLiveDecal?.Invoke(liveDecal);

            Destroy(liveDecal.gameObject);
        }

        public void UpdateDecalUnderPointer()
        {
            //raycast from the pointer position into the world
            Ray ray = Camera.main.ScreenPointToRay(PrimaryContactInput.Position);

            //max raycast distance from the camera to the middle of the car, so it doesn't detect decals on the other side
            float maxRaycastDistance = Vector3.Distance(Camera.main.transform.position, currentCar.transform.position);
            int raycastHits = Physics.RaycastNonAlloc(ray, decalsUnderPointer, maxRaycastDistance, decalLayerMask);

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
                SelectLiveDecal(closestDecal);
            else DeselectLiveDecal();
        }
        
        private void SetMeshPaintable(MeshFilter meshFilter)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            if (!meshRenderer.material.shader.Equals(carBodyShader))
                return;

            //before resetting the textures, clear the previous texture it has made
            meshRenderer.sharedMaterial.SetTexture(AlbedoShaderID, null);

            DestroyImmediate(meshFilter.gameObject.GetComponent<P3dPaintableTexture>());
            
            P3dPaintable paintable = meshFilter.gameObject.GetComponent<P3dPaintable>(true);
            P3dPaintableTexture paintableTexture = meshFilter.gameObject.GetComponent<P3dPaintableTexture>(true);
            MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>(true);
            P3dMaterialCloner materialCloner = meshFilter.gameObject.GetComponent<P3dMaterialCloner>(true);
            
            materialCloner.enabled = true;
            paintableTexture.enabled = true;
            meshCollider.enabled = true;
            paintable.enabled = true;

            //paintable.UseMesh = P3dModel.UseMeshType.AutoSeamFix;
            paintableTexture.Slot = new P3dSlot(0, "_Albedo"); //car body shader uses albedo
            
            PaintableMesh paintableMesh = new PaintableMesh(paintable, materialCloner, paintableTexture, meshCollider);
            paintableMeshes.Add(paintableMesh);
        }

        private void RemoveMeshPaintable(PaintableMesh paintableMesh)
        {
            Destroy(paintableMesh.MaterialCloner);
            Destroy(paintableMesh.Paintable);
            paintableMesh.PaintableTexture.enabled = false;
            paintableMesh.MeshCollider.enabled = false;
            paintableMeshes.Remove(paintableMesh);
        }
        
    }
}
