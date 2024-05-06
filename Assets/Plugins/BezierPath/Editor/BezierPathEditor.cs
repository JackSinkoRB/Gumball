using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(BezierPath))]
public class BezierPathEditor : Editor
{
    int selectedPoint = -1;
    Vector3 lastPointPos;

    LayerMask raycastLayerMask = -1;

    private Color debugColor;
    bool CheckValidPath(BezierPath path)
    {
        if (path.pointsPerSegment < 1)
            path.pointsPerSegment = 1;

        if (path.bezierPoints == null)
            path.bezierPoints = new List<Vector3>();

        if (selectedPoint < -1 || selectedPoint >= path.bezierPoints.Count)
            selectedPoint = -1;

        return path.cachedPoints != null && path.cachedPoints.Count > 1;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BezierPath path = (BezierPath)target;
        bool isvalidpath = CheckValidPath(path);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("C - Adds point to end");
        sb.AppendLine("X - Start line again at new point");
        sb.AppendLine("I - Insert point at");
        sb.AppendLine("M - Move selected point");

        sb.AppendLine("\nSTATS");
        if (isvalidpath)
        {
            sb.AppendLine("Total Distance: " + path.totalDistance.ToString());
            sb.AppendLine("Cached Points: " + path.cachedPoints.Count.ToString());
            sb.AppendLine("Bezier Points: " + path.bezierPoints.Count.ToString());
            sb.AppendLine("View Color Val: " + debugColor.ToString());
        }
        sb.Append("Selected: " + selectedPoint.ToString());

        EditorGUILayout.HelpBox(sb.ToString(), MessageType.None);

        //raycastLayerMask = UnityUtils.LayerMaskField("Raycast Layers", raycastLayerMask);

        if (GUILayout.Button("Reverse"))
        {
            Undo.RegisterCompleteObjectUndo(path, "Reverse Bezier Points");
            path.bezierPoints.Reverse();
            EditorUtility.SetDirty(path);
            selectedPoint = -1;
            path.Rebuild();
        }

        if (GUI.changed)
        {
            path.Rebuild();
            EditorUtility.SetDirty(target);
        }
    }

    bool Raycast(Ray ray, out RaycastHit hit)
    {
        List<RaycastHit> hits = new List<RaycastHit>(Physics.RaycastAll(ray, 5000, raycastLayerMask));
        hits.RemoveAll(h => h.collider.isTrigger);
        hits.Sort((a, b) => a.distance < b.distance ? -1 : 1);
        if (hits.Count == 0)
        {
            hit = new RaycastHit();
            return false;
        }

        hit = hits[0];
        return true;
    }


    void OnSceneGUI()
    {
        BezierPath path = (BezierPath)target;
        bool isvalidpath = CheckValidPath(path);

        if (Event.current.type == EventType.MouseDown && selectedPoint != -1)
        {
            // record position
            lastPointPos = path.bezierPoints[selectedPoint];
        }
        if (Event.current.type == EventType.MouseUp && selectedPoint != -1 && path.bezierPoints[selectedPoint] != lastPointPos)
        {
            // put them back, register undo
            Vector3 newPos = path.bezierPoints[selectedPoint];
            path.bezierPoints[selectedPoint] = lastPointPos;
            Undo.RegisterCompleteObjectUndo(path, "Move Bezier Point");
            path.bezierPoints[selectedPoint] = newPos;
            EditorUtility.SetDirty(path);
        }

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.C)
            {
                // add point to end
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Raycast(ray, out hit))
                {
                    Undo.RegisterCompleteObjectUndo(path, "Append Bezier Point");
                    path.bezierPoints.Add(hit.point);
                    EditorUtility.SetDirty(path);
                    selectedPoint = path.bezierPoints.Count - 1;
                    path.Rebuild();
                }
            }
            else if (Event.current.keyCode == KeyCode.I && isvalidpath)
            {
                // insert point
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Raycast(ray, out hit))
                {
                    int segment = Mathf.FloorToInt(path.GetPointFromT(path.GetNearestTAtPosition(hit.point)).bezierRatio) + 1;

                    Undo.RegisterCompleteObjectUndo(path, "Insert Bezier Point");
                    path.bezierPoints.Insert(segment, hit.point);
                    EditorUtility.SetDirty(path);
                    selectedPoint = segment;
                    path.Rebuild();
                }
            }
            else if (Event.current.keyCode == KeyCode.X)
            {
                // clear and add point
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Raycast(ray, out hit))
                {
                    Undo.RegisterCompleteObjectUndo(path, "Insert Bezier Point");
                    path.bezierPoints.Clear();
                    path.bezierPoints.Add(hit.point);
                    EditorUtility.SetDirty(path);
                    selectedPoint = 0;
                    path.Rebuild();
                }
            }
            else if (selectedPoint != -1 && Event.current.keyCode == KeyCode.Backspace)
            {
                // remove selected point
                Undo.RegisterCompleteObjectUndo(path, "Remove Bezier Point");
                path.bezierPoints.RemoveAt(selectedPoint);
                EditorUtility.SetDirty(path);
                selectedPoint = -1;
                path.Rebuild();
            }
            if (selectedPoint != -1 && Event.current.keyCode == KeyCode.M)
            {
                // move point
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                if (Raycast(ray, out hit))
                {
                    path.bezierPoints[selectedPoint] = hit.point;
                    EditorUtility.SetDirty(path);
                    path.Rebuild();
                }
            }
        }

        // disable transform tool if selected

        if (Selection.activeGameObject == path.gameObject)
        {
            Tools.current = Tool.None;
            
        }
        if (selectedPoint == -1 && path.bezierPoints.Count > 0)
            selectedPoint = 0;
        // draw the handles for the points
        Handles.color = path.displayHandleColor;
        debugColor = Handles.color;
        for (int i = 0; i < path.bezierPoints.Count; ++i)
        {
            if (i == selectedPoint)
            {
                Handles.Button(path.bezierPoints[i], Quaternion.identity, path.displayPointSize, path.displayPointSize, Handles.CubeHandleCap);
                Handles.CubeHandleCap(0, path.bezierPoints[i], Quaternion.identity, path.displayPointSize, EventType.MouseDown);
                path.bezierPoints[i] = Handles.PositionHandle(path.bezierPoints[i], Quaternion.identity);
            }
            else
            {
                // draw button
                if (Handles.Button(path.bezierPoints[i], Quaternion.identity, path.displayPointSize, path.displayPointSize, Handles.CubeHandleCap))
                    selectedPoint = i;
            }
        }

        // draw the lines
        if (isvalidpath)
        {
            Handles.color = path.displayLineColor;
            for (int i = 0; i < path.cachedPoints.Count - 1; i++)
                Handles.DrawLine(path.cachedPoints[i].position, path.cachedPoints[i + 1].position);
        }

        if (GUI.changed)
        {
            path.Rebuild();
            EditorUtility.SetDirty(target);
        }
    }

    [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.Pickable)]
    static void DrawPathGizmos(BezierPath path, GizmoType gizmoType)
    {
        // don't draw if selected
        if (Selection.activeGameObject == path.gameObject || path.cachedPoints == null)
             return;
        for (int i = 0; i < path.bezierPoints.Count; ++i)
        {
            // draw point
            Gizmos.color = path.displayLineColor;
            Gizmos.DrawCube(path.bezierPoints[i], Vector3.one * path.displayPointSize);
        }

        Gizmos.color = path.displayLineColor;
        for (int i = 0; i < path.cachedPoints.Count - 1; i++)
            Gizmos.DrawLine(path.cachedPoints[i].position, path.cachedPoints[i + 1].position);
    }
}
