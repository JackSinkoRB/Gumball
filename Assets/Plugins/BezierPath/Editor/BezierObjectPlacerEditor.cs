using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BezierObjectPlacer))]
public class BezierObjectPlacerEditor : Editor
{
    bool _autoUpdate = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BezierObjectPlacer bop = (BezierObjectPlacer)target;
        EditorGUILayout.Separator();
        _autoUpdate = EditorGUILayout.Toggle("Auto Update", _autoUpdate);
        if ((bop.isActiveAndEnabled && _autoUpdate) || GUILayout.Button("Force Update"))
            bop.Apply();
    }
}
