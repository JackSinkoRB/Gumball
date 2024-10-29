#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using MyBox.Internal;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [CustomEditor(typeof(PaintableMesh))]
    public class PaintableMeshEditor : UnityObjectEditor
    {

        private PaintableMesh paintableMesh => (PaintableMesh)target;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (paintableMesh.MeshFilter != null && paintableMesh.MeshFilter.sharedMesh != null)
                paintableMesh.MeshFilter.sharedMesh.SetReadable();
        }
        
    }
}
#endif