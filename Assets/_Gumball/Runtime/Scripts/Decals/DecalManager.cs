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

        public DecalUICategory[] DecalUICategories => decalUICategories;
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
            if (!PrimaryContactInput.IsClickableUnderPointer(selectedLiveDecalUI.ScaleRotationHandle.Button.image))
                GetDecalsUnderPointer();
        }

        [Serializable]
        private struct PaintableMesh
        {
            public P3dPaintable Paintable;
            public P3dMaterialCloner MaterialCloner;
            public P3dPaintableTexture PaintableTexture;

            public PaintableMesh(P3dPaintable paintable, P3dMaterialCloner materialCloner, P3dPaintableTexture paintableTexture)
            {
                Paintable = paintable;
                MaterialCloner = materialCloner;
                PaintableTexture = paintableTexture;
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

        public void SelectLiveDecal(LiveDecal liveDecal)
        {
            currentSelected = liveDecal;
        }

        public void DeselectLiveDecal()
        {
            currentSelected = null;
        }
        
        public event Action onCreateLiveDecal;
        public event Action onDestroyLiveDecal;

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
            
            onCreateLiveDecal?.Invoke();
            
            return liveDecal;
        }

        public void DestroyLiveDecal(LiveDecal liveDecal)
        {
            Destroy(gameObject);
            
            onDestroyLiveDecal?.Invoke();
        }

        [Serializable]
        public class LiveDecalData
        {
            [SerializeField, ReadOnly] private Vector3 position;
            [SerializeField, ReadOnly] private Sprite sprite;
        }

        //index is the priority
        [SerializeField, ReadOnly] private List<LiveDecalData> liveDecalsData = new();
        [SerializeField, ReadOnly] private List<LiveDecal> liveDecals = new();

        public List<LiveDecal> LiveDecals => liveDecals;
        
        public LiveDecal GetLiveDecalByPriority(int priority)
        {
            return liveDecals[priority];
        }
        
        public void GetDecalsUnderPointer()
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
        
    }
}
