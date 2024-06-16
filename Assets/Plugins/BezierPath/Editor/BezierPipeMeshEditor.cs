#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace BezierPath
{
    [CustomEditor(typeof(BezierPipeMesh))]
    [CanEditMultipleObjects]
    public class BezierPipeMeshEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            foreach (Object t in targets)
            {
                BezierPipeMesh bpm = (BezierPipeMesh)t;
                if (bpm.isActiveAndEnabled)
                    bpm.Generate();
            }
        }
    }   
}
#endif