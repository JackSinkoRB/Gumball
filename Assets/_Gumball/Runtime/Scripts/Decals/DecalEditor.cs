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

        public event Action<LiveDecal> onSelectLiveDecal;
        public event Action<LiveDecal> onDeselectLiveDecal;
        public event Action<LiveDecal> onCreateLiveDecal;
        public event Action<LiveDecal> onDestroyLiveDecal;

        [SerializeField] private Logger logger;
        
        [SerializeField] private LiveDecal liveDecalPrefab;
        [Tooltip("The shader that the car body uses. The decal will only be applied to the materials using this shader.")]
        [SerializeField] private Shader carBodyShader;
        [SerializeField] private CarManager car;
        [SerializeField] private SelectedDecalUI selectedLiveDecalUI;
        [SerializeField] private DecalUICategory[] decalUICategories;

        [Header("Debugging")]
        [SerializeField, ReadOnly] private LiveDecal currentSelected;
        [SerializeField, ReadOnly] private int priorityCount;
        [SerializeField, ReadOnly] private List<PaintableMesh> paintableMeshes = new();
        [Tooltip("Index is the priority")]
        [SerializeField, ReadOnly] private List<LiveDecal> liveDecals = new();
        
        public DecalUICategory[] DecalUICategories => decalUICategories;
        public List<LiveDecal> LiveDecals => liveDecals;
        public LiveDecal CurrentSelected => currentSelected;
        
        private readonly RaycastHit[] decalsUnderPointer = new RaycastHit[MaxDecalsAllowed];

        public static void LoadDecalEditor()
        {
            CoroutineHelper.Instance.StartCoroutine(LoadDecalEditorIE());
        }
        
        private static IEnumerator LoadDecalEditorIE()
        {
            PanelManager.GetPanel<LoadingPanel>().Show();
            
            //set the vehicle kinematic
            Rigidbody currentCarRigidbody = PlayerCarManager.Instance.CurrentCar.Rigidbody;
            currentCarRigidbody.isKinematic = true;
            
            Stopwatch sceneLoadingStopwatch = new Stopwatch();
            sceneLoadingStopwatch.Start();
            yield return Addressables.LoadSceneAsync(SceneManager.DecalEditorSceneName, LoadSceneMode.Single, true);
            sceneLoadingStopwatch.Stop();
            GlobalLoggers.LoadingLogger.Log($"{SceneManager.DecalEditorSceneName} loading complete in {sceneLoadingStopwatch.Elapsed.ToPrettyString(true)}");
            
            //move the vehicle to the right position
            currentCarRigidbody.Move(Vector3.zero, Quaternion.Euler(Vector3.zero));
            
            PanelManager.GetPanel<LoadingPanel>().Hide();
        }
        
        private void OnEnable()
        {
            PrimaryContactInput.onPress += OnPrimaryContactPressed;

            car = PlayerCarManager.Instance.CurrentCar;
            StartSession();
        }

        private void OnDisable()
        {
            PrimaryContactInput.onPress -= OnPrimaryContactPressed;

            EndSession();
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
            Image layerSelectorImage = PanelManager.GetPanel<DecalEditorPanel>().LayerSelector.MagneticScroll.GetComponent<Image>();
            Image scaleRotationHandleImage = selectedLiveDecalUI.ScaleRotationHandle.Button.image;
            
            if (!PrimaryContactInput.IsClickableUnderPointer(scaleRotationHandleImage)
                && !PrimaryContactInput.IsClickableUnderPointer(layerSelectorImage))
            {
                UpdateDecalUnderPointer();
            }
        }

        private void StartSession()
        {
            InputManager.Instance.EnableActionMap(InputManager.ActionMapType.General);

            liveDecals = DecalManager.CreateLiveDecalsFromData(car);
            this.PerformAtEndOfFrame(DeselectLiveDecal);
            
            paintableMeshes.Clear();
            foreach (MeshFilter meshFilter in car.transform.GetComponentsInAllChildren<MeshFilter>())
            {
                SetMeshPaintable(meshFilter);
            }
            
            //disable the car's collider temporarily
            PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(false);
        }

        private void EndSession()
        {
            DeselectLiveDecal();
            
            DecalManager.SaveLiveDecalData(car, liveDecals);

            foreach (LiveDecal liveDecal in liveDecals)
            {
                liveDecal.Apply();
                Destroy(liveDecal.gameObject);
            }
            
            liveDecals.Clear();
            priorityCount = 0;
            
            for (int i = paintableMeshes.Count - 1; i >= 0; i--)
            {
                PaintableMesh paintableMesh = paintableMeshes[i];
                RemoveMeshPaintable(paintableMesh);
                paintableMeshes.Remove(paintableMesh);
            }
            
            if (PlayerCarManager.ExistsRuntime)
                PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(true);
        }


        public Sprite GetTextureFromCategoryAndIndex(int categoryIndex, int index)
        {
            return decalUICategories[categoryIndex].Sprites[index];
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

        public LiveDecal CreateLiveDecalFromData(LiveDecal.LiveDecalData data)
        {
            DecalUICategory category = decalUICategories[data.CategoryIndex];
            Sprite sprite = category.Sprites[data.TextureIndex];
            LiveDecal liveDecal = CreateLiveDecal(category, sprite, data.Priority);
            liveDecal.UpdatePosition(data.LastKnownPosition.ToVector3(), data.LastKnownHitNormal.ToVector3(), Quaternion.Euler(data.LastKnownRotationEuler.ToVector3()));
            liveDecal.SetScale(data.Scale.ToVector3());
            liveDecal.SetAngle(data.Angle);
            liveDecal.SetValid();
            return liveDecal;
        }

        public LiveDecal CreateLiveDecal(DecalUICategory category, Sprite sprite, int priority = -1)
        {
            LiveDecal liveDecal = Instantiate(liveDecalPrefab.gameObject, transform).GetComponent<LiveDecal>();
            liveDecal.Initialise(Array.IndexOf(decalUICategories, category), Array.IndexOf(category.Sprites, sprite));
            liveDecal.SetSprite(sprite);

            if (category.CategoryName.Equals("Shapes"))
                liveDecal.SetColor(Color.gray);

            //set priority
            if (priority == -1)
            {
                liveDecal.SetPriority(priorityCount);
                priorityCount++;
            }
            else
            {
                liveDecal.SetPriority(priority);
                if (priority > priorityCount)
                    priorityCount = priority;
            }

            liveDecals.Add(liveDecal);
            
            onCreateLiveDecal?.Invoke(liveDecal);
            
            return liveDecal;
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
            float maxRaycastDistance = Vector3.Distance(Camera.main.transform.position, car.transform.position);
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

                logger.Log($"Distance to {decal.Priority} = {distanceToDecalSqr}");
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
            
            P3dPaintable paintable = meshFilter.gameObject.GetComponent<P3dPaintable>(true);
            P3dMaterialCloner materialCloner = meshFilter.gameObject.GetComponent<P3dMaterialCloner>(true);
            P3dPaintableTexture paintableTexture = meshFilter.gameObject.GetComponent<P3dPaintableTexture>(true);
            MeshCollider meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>(true);
            materialCloner.enabled = true;
            paintableTexture.enabled = true;
            paintable.enabled = true;
            meshCollider.enabled = true;
            
            paintable.UseMesh = P3dModel.UseMeshType.AutoSeamFix;
            paintableTexture.Slot = new P3dSlot(0, "_Albedo"); //car body shader uses albedo
            
            PaintableMesh paintableMesh = new PaintableMesh(paintable, materialCloner, paintableTexture, meshCollider);
            paintableMeshes.Add(paintableMesh);
        }

        private void RemoveMeshPaintable(PaintableMesh paintableMesh)
        {
            paintableMesh.MaterialCloner.enabled = false;
            paintableMesh.PaintableTexture.enabled = false;
            paintableMesh.Paintable.enabled = false;
            paintableMesh.MeshCollider.enabled = false;
            paintableMeshes.Remove(paintableMesh);
        }
        
    }
}
