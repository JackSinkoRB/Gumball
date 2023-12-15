using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using PaintIn3D;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class PaintableMesh : MonoBehaviour
    {

        [Serializable]
        public struct ResolutionData
        {
            [Serializable]
            public struct Resolution
            {
                public int width;
                public int height;

                public Resolution(int width, int height)
                {
                    this.width = width;
                    this.height = height;
                }
            }
            
            [SerializeField] private Resolution mobile;
            [SerializeField] private Resolution pc;

            public ResolutionData(Resolution mobile, Resolution pc)
            {
                this.mobile = mobile;
                this.pc = pc;
            }

            public Resolution GetResolution()
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return mobile;               
#else
                return pc;
#endif
            }
        }
        
        private const string textureString = "_BaseMap";
        private static readonly int textureID = Shader.PropertyToID(textureString);
        
        [SerializeField] private ResolutionData textureResolution = new(
            new ResolutionData.Resolution(512, 512), 
            new ResolutionData.Resolution(512, 512));

        [Header("Debugging")]
        [SerializeField, ReadOnly] private P3dPaintable paintable;
        [SerializeField, ReadOnly] private P3dMaterialCloner materialCloner;
        [SerializeField, ReadOnly] private P3dPaintableTexture paintableTexture;
        [SerializeField, ReadOnly] private MeshCollider meshCollider;

        private MeshRenderer meshRenderer => GetComponent<MeshRenderer>();
        private MeshFilter meshFilter => GetComponent<MeshFilter>();
        
        public void EnablePainting()
        {
            //before resetting the textures, clear the previous texture it has made
            meshRenderer.sharedMaterial.SetTexture(textureID, null);

            if (meshFilter.gameObject.GetComponent<P3dPaintableTexture>() != null)
                DestroyImmediate(meshFilter.gameObject.GetComponent<P3dPaintableTexture>());
            
            paintable = meshFilter.gameObject.GetComponent<P3dPaintable>(true);
            paintableTexture = meshFilter.gameObject.GetComponent<P3dPaintableTexture>(true);
            meshCollider = meshFilter.gameObject.GetComponent<MeshCollider>(true);
            materialCloner = meshFilter.gameObject.GetComponent<P3dMaterialCloner>(true);

            paintableTexture.Width = textureResolution.GetResolution().width;
            paintableTexture.Height = textureResolution.GetResolution().height;
            
            materialCloner.enabled = true;
            paintableTexture.enabled = true;
            meshCollider.enabled = true;
            paintable.enabled = true;

            paintableTexture.Slot = new P3dSlot(0, textureString); //car body shader uses albedo
            
            //paintable.UseMesh = P3dModel.UseMeshType.AutoSeamFix;
        }

        public void DisablePainting()
        {
            Destroy(materialCloner);
            Destroy(paintable);
            paintableTexture.enabled = false;
            DestroyImmediate(meshCollider);
        }
        
    }
}
