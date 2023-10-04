using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using PaintIn3D;
using UnityEngine;

namespace Gumball
{
    public class DecalManager : MonoBehaviour
    {
        
        [SerializeField] private LiveDecal liveDecalPrefab;
        [SerializeField] private Transform car;
        [SerializeField, ReadOnly] private List<PaintableMesh> paintableMeshes = new();

        private void OnEnable()
        {
            StartSession(); //temp
        }

        private void OnDisable()
        {
            EndSession(); //temp
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
            P3dPaintable paintable = meshFilter.gameObject.AddComponent<P3dPaintable>();
            P3dMaterialCloner materialCloner = meshFilter.gameObject.AddComponent<P3dMaterialCloner>();
            P3dPaintableTexture paintableTexture = meshFilter.gameObject.AddComponent<P3dPaintableTexture>();
            
            //todo: need to disable the car collider too
            MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();

            if (!meshFilter.GetComponent<MeshRenderer>().material.HasProperty("_MainTex"))
                paintableTexture.Slot = new P3dSlot(0, "_Albedo");
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

        private void Start()
        {
            CreateLiveDecal();
        }
        
        private void CreateLiveDecal()
        {
            Instantiate(liveDecalPrefab.gameObject, transform);
        }
    }
}
