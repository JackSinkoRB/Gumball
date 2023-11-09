using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MyBox;
using PaintIn3D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Gumball
{
    public class DecalManager : Singleton<DecalManager>
    {

        public const int MaxDecalsAllowed = 50;
        private const int liveDecalLayer = 6;

        public event Action<LiveDecal> onSelectLiveDecal;
        public event Action<LiveDecal> onDeselectLiveDecal;
        public event Action<LiveDecal> onCreateLiveDecal;
        public event Action<LiveDecal> onDestroyLiveDecal;
        
        [SerializeField] private LiveDecal liveDecalPrefab;
        [Tooltip("The shader that the car body uses. The decal will only be applied to the materials using this shader.")]
        [SerializeField] private Shader carBodyShader;
        [SerializeField] private Transform car;
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

            car = PlayerCarManager.Instance.CurrentCar.transform;
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

            paintableMeshes.Clear();
            foreach (MeshFilter meshFilter in car.GetComponentsInAllChildren<MeshFilter>())
            {
                SetMeshPaintable(meshFilter);
            }
            
            //disable the car's collider temporarily
            PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(false);
        }

        private void EndSession()
        {
            for (int i = paintableMeshes.Count - 1; i >= 0; i--)
            {
                PaintableMesh paintableMesh = paintableMeshes[i];
                RemoveMeshPaintable(paintableMesh);
                paintableMeshes.Remove(paintableMesh);
            }
            
            if (PlayerCarManager.ExistsRuntime)
                PlayerCarManager.Instance.CurrentCar.Colliders.SetActive(true);
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

        public LiveDecal CreateLiveDecal(DecalUICategory category, Sprite sprite)
        {
            LiveDecal liveDecal = Instantiate(liveDecalPrefab.gameObject, transform).GetComponent<LiveDecal>();
            liveDecal.PaintDecal.Texture = sprite.texture;
            liveDecal.SetSprite(sprite);

            if (category.CategoryName.Equals("Shapes"))
                liveDecal.SetColor(Color.gray);

            liveDecal.SetPriority(priorityCount);
            priorityCount++;

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

            int raycastHits = Physics.RaycastNonAlloc(ray, decalsUnderPointer, Mathf.Infinity, 1 << liveDecalLayer);

            LiveDecal highestPriorityDecal = null;
            for (int index = 0; index < raycastHits; index++)
            {
                RaycastHit hit = decalsUnderPointer[index];
                if (hit.collider == null)
                    break;
                
                LiveDecal decal = hit.transform.parent.GetComponent<LiveDecal>();

                if (highestPriorityDecal == null || decal.Priority > highestPriorityDecal.Priority)
                    highestPriorityDecal = decal;
            }

            if (highestPriorityDecal != null)
                SelectLiveDecal(highestPriorityDecal);
            else DeselectLiveDecal();
        }
        
        private void SetMeshPaintable(MeshFilter meshFilter)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            if (!meshRenderer.material.shader.Equals(carBodyShader))
                return;
            
            P3dPaintable paintable = meshFilter.gameObject.AddComponent<P3dPaintable>();
            P3dMaterialCloner materialCloner = meshFilter.gameObject.AddComponent<P3dMaterialCloner>();
            P3dPaintableTexture paintableTexture = meshFilter.gameObject.AddComponent<P3dPaintableTexture>();

            paintable.UseMesh = P3dModel.UseMeshType.AutoSeamFix;
            paintableTexture.Slot = new P3dSlot(0, "_Albedo"); //car body shader uses albedo
                        
            meshFilter.gameObject.AddComponent<MeshCollider>();
            
            PaintableMesh paintableMesh = new PaintableMesh(paintable, materialCloner, paintableTexture);
            paintableMeshes.Add(paintableMesh);
        }

        private void RemoveMeshPaintable(PaintableMesh paintableMesh)
        {
            Destroy(paintableMesh.MaterialCloner);
            Destroy(paintableMesh.PaintableTexture);
            Destroy(paintableMesh.Paintable);
            paintableMeshes.Remove(paintableMesh);
        }
        
    }
}
