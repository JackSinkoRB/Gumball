using UnityEngine;
using UnityEditor;

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
