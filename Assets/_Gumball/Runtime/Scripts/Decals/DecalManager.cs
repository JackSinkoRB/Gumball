using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using PaintIn3D;
using UnityEngine;

namespace Gumball
{
    public class DecalManager : Singleton<DecalManager>
    {

        [SerializeField] private LiveDecal liveDecalPrefab;
        [SerializeField] private Transform car;
        [SerializeField] private RectTransform selectedLiveDecalUI;

        [SerializeField] private Sprite[] textureOptions;

        [SerializeField, ReadOnly] private LiveDecal currentSelectedDecal;
        
        [SerializeField, ReadOnly] private List<PaintableMesh> paintableMeshes = new();

        public Sprite[] TextureOptions => textureOptions;
        public LiveDecal CurrentSelectedDecal => currentSelectedDecal;
        
        private void OnEnable()
        {
            StartSession(); //temp
        }

        private void OnDisable()
        {
            EndSession(); //temp
        }

        private void LateUpdate()
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentSelectedDecal.transform.position);
            selectedLiveDecalUI.position = screenPos;
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
            InputManager.Instance.SetActionMap(InputManager.ActionMapType.General);
            
            paintableMeshes.Clear();
            //TODO: only set the mesh paintable if it has the 'paintable' tag
            foreach (MeshFilter meshFilter in car.GetComponentsInAllChildren<MeshFilter>())
            {
                SetMeshPaintable(meshFilter);
            }
        }

        private void EndSession()
        {
            for (int i = paintableMeshes.Count - 1; i >= 0; i--)
            {
                PaintableMesh paintableMesh = paintableMeshes[i];
                RemoveMeshPaintable(paintableMesh);
                paintableMeshes.Remove(paintableMesh);
            }
        }
        
        private void SetMeshPaintable(MeshFilter meshFilter)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            //TODO: compare the shaders instead for the reflection shader
            if (!meshRenderer.material.HasProperty("_Albedo"))
                return; //the car reflection shader does not use a _MainTex, so only apply to the reflection shader
            
            P3dPaintable paintable = meshFilter.gameObject.AddComponent<P3dPaintable>();
            P3dMaterialCloner materialCloner = meshFilter.gameObject.AddComponent<P3dMaterialCloner>();
            P3dPaintableTexture paintableTexture = meshFilter.gameObject.AddComponent<P3dPaintableTexture>();

            paintable.UseMesh = P3dModel.UseMeshType.AutoSeamFix;
            paintableTexture.Slot = new P3dSlot(0, "_Albedo");
                        
            //todo: need to disable the car collider too
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
            currentSelectedDecal = liveDecal;
        }
        
        public LiveDecal CreateLiveDecal(Texture texture)
        {
            LiveDecal liveDecal = Instantiate(liveDecalPrefab.gameObject, transform).GetComponent<LiveDecal>();
            liveDecal.PaintDecal.Texture = texture;
            return liveDecal;
        }
    }
}
