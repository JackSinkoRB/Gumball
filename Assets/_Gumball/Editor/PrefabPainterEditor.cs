using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PrefabPainterEditor : EditorWindow
{
    public List<GameObject> prefabs = new List<GameObject>();
    public Transform parent;
    public float scaleRandomness = 0;
    public bool randomRotation = true;
    public bool alignToNormal = true;
    public float verticalOffset = 0;
    public Vector3 extraRotation = Vector3.zero;
    public LayerMask layerMask = -1;
    public float raycastDistance = 1000;
    
    static List<string> layers;
    static string[] layerNames;

    bool paintModeEnabled = false;

    public static LayerMask LayerMaskField(string label, LayerMask selected)
    {
        if (layers == null)
        {
            layers = new List<string>();
            layerNames = new string[4];
        }
        else
            layers.Clear();

        int emptyLayers = 0;
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);

            if (layerName != "")
            {
                for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                layers.Add(layerName);
            }
            else
                emptyLayers++;
        }

        if (layerNames.Length != layers.Count)
            layerNames = new string[layers.Count];
        for (int i = 0; i < layerNames.Length; i++)
            layerNames[i] = layers[i];

        selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);
        return selected;
    }

    void OnGUI()
    {
        paintModeEnabled = EditorGUILayout.Toggle("Painting Enabled", paintModeEnabled);

        EditorGUILayout.Separator();

        int size = EditorGUILayout.IntField("Prefab Count", prefabs.Count);
        if (size < prefabs.Count)
            prefabs.RemoveRange(size, prefabs.Count - size);
        else if (size > prefabs.Count)
        {
            while (prefabs.Count < size)
                prefabs.Add(null);
        }

        for (int i = 0; i < prefabs.Count; ++i)
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField(i.ToString(), prefabs[i], typeof(GameObject), false);

        parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);
        scaleRandomness = EditorGUILayout.Slider("Scale Randomness", scaleRandomness, 0.0f, 1.0f);
        randomRotation = EditorGUILayout.Toggle("Random Rotation", randomRotation);
        alignToNormal = EditorGUILayout.Toggle("Align To Normal", alignToNormal);
        verticalOffset = EditorGUILayout.FloatField("Vertical Offset", verticalOffset);
        extraRotation = EditorGUILayout.Vector3Field("Extra Rotation", extraRotation);
        layerMask = LayerMaskField("Layer Mask", layerMask);
        raycastDistance = EditorGUILayout.FloatField("Raycast Distance", raycastDistance);
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }

    void SceneGUI(SceneView sceneView)
    {
        if (prefabs.Count == 0)
            return;

        if (paintModeEnabled && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            RaycastHit hit;
            if (!Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit, raycastDistance, layerMask))
                return;
            
            Vector3 position = hit.point;
            Vector3 up = alignToNormal ? hit.normal : Vector3.up;
            position += up * verticalOffset;
            Vector3 extrarotation = extraRotation;
            if (randomRotation)
                extrarotation.y += Random.Range(0, 360);

            Transform obj = ((GameObject)PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Count)])).transform;
            obj.parent = parent;
            obj.position = position;
            obj.up = up;
            obj.localRotation *= Quaternion.Euler(extrarotation);
            obj.localScale *= Random.Range(1 - scaleRandomness, 1 + scaleRandomness);

            Undo.RegisterCreatedObjectUndo(obj.gameObject, "Prefab Painted");
            EditorUtility.SetDirty(obj.gameObject);
        }
    }

    [MenuItem("Custom/Prefab Painter", false, 'P')]
    static void Init()
    {
        PrefabPainterEditor window = (PrefabPainterEditor)GetWindow(typeof(PrefabPainterEditor));
        window.Show();
    }

    [MenuItem("Custom/Turn Into LOD", false, 'T')]
    static void TurnIntoLOD()
    {
        if (Selection.activeGameObject == null)
            return;

        MeshRenderer renderer = Selection.activeGameObject.GetComponent<MeshRenderer>();
        if (renderer == null)
            return;

        Transform t = new GameObject("LOD0").transform;
        t.parent = Selection.activeTransform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;

        t.gameObject.AddComponent<MeshFilter>().sharedMesh = Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh;
        t.gameObject.AddComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;

        DestroyImmediate(renderer);
        DestroyImmediate(Selection.activeGameObject.GetComponent<MeshFilter>());

        LODGroup lodgroup = Selection.activeGameObject.AddComponent<LODGroup>();

        LOD[] lods = new LOD[]
        {
            new LOD(0.5f, new Renderer[] { t.GetComponent<Renderer>() })
        };
        lodgroup.SetLODs(lods);

    }
}