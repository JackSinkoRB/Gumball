using System.Collections.Generic;
using UnityEngine;
using System;

namespace CC
{
    public class BlendshapeManager : MonoBehaviour
    {

        public const float BlendShapeValueModifier = 100;
        
        /// <summary>
        /// Blendshape property, index in shared mesh
        /// </summary>
        private readonly Dictionary<string, int> blendshapeNames = new();

        private bool isInitialised;
        private SkinnedMeshRenderer mesh;
        
        private void TryInitialise()
        {
            if (!isInitialised)
            {
                isInitialised = true;
                ParseBlendshapes();
            }
        }
        
        public void SetBlendshape(string propertyName, float value)
        {
            TryInitialise();

            if (!blendshapeNames.ContainsKey(propertyName))
                return;
            
            int index = blendshapeNames[propertyName];
            mesh.SetBlendShapeWeight(index, value * BlendShapeValueModifier);
        }

        public float GetBlendshape(string propertyName)
        {
            TryInitialise();

            if (!blendshapeNames.ContainsKey(propertyName))
                throw new NullReferenceException($"Property {propertyName} doesn't exist in the blend shapes.");
            
            int index = blendshapeNames[propertyName];
            return mesh.GetBlendShapeWeight(index);
        }
        
        private void ParseBlendshapes()
        {
            mesh = gameObject.GetComponent<SkinnedMeshRenderer>();

            if (mesh.sharedMesh == null)
                return;
            
            for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                blendshapeNames[mesh.sharedMesh.GetBlendShapeName(i)] = i;
            }
        }
    }
}