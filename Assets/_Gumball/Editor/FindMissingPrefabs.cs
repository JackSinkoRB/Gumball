using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class FindMissingPrefabs : EditorWindow
{
    [MenuItem("Tools/Find Missing Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<FindMissingPrefabs>("Find Missing Prefabs");
    }

    private GameObject parentPrefab;
    private List<GameObject> childPrefabs;

    void OnGUI()
    {
        parentPrefab = (GameObject)EditorGUILayout.ObjectField("Parent Prefab", parentPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Select Child Prefabs"))
        {
            childPrefabs = new List<GameObject>();
            foreach (var obj in Selection.objects)
            {
                if (obj is GameObject)
                {
                    childPrefabs.Add((GameObject)obj);
                }
            }
        }

        if (GUILayout.Button("Find Missing/Extra Prefabs"))
        {
            FindMissingAndExtra();
        }

        if (GUILayout.Button("Select Missing Prefabs"))
        {
            SelectMissingPrefabs();
        }
    }

    private void FindMissingAndExtra()
    {
        if (parentPrefab == null || childPrefabs == null || childPrefabs.Count == 0)
        {
            Debug.LogError("<color=red>Please select a parent prefab and child prefabs first.</color>");
            return;
        }

        var childPrefabNames = new HashSet<string>();
        foreach (var prefab in childPrefabs)
        {
            childPrefabNames.Add(prefab.name);
        }

        var parentPrefabNames = new HashSet<string>();
        AddChildPrefabNamesRecursively(parentPrefab.transform, parentPrefabNames);

        var missingPrefabs = new List<string>();
        foreach (var name in childPrefabNames)
        {
            if (!parentPrefabNames.Contains(name))
            {
                missingPrefabs.Add(name);
            }
        }

        var extraPrefabs = new List<string>();
        foreach (var name in parentPrefabNames)
        {
            if (!childPrefabNames.Contains(name))
            {
                extraPrefabs.Add(name);
            }
        }

        if (missingPrefabs.Count > 0)
        {
            Debug.Log("<color=yellow>Missing Prefabs (In Project but not in Parent):</color>");
            foreach (var missingPrefab in missingPrefabs)
            {
                Debug.Log("<color=yellow>-- " + missingPrefab + "</color>");
            }
        }

        if (extraPrefabs.Count > 0)
        {
            Debug.Log("<color=cyan>Extra Prefabs (In Parent but not in Project):</color>");
            foreach (var extraPrefab in extraPrefabs)
            {
                Debug.Log("<color=cyan>-- " + extraPrefab + "</color>");
            }
        }

        if (missingPrefabs.Count == 0 && extraPrefabs.Count == 0)
        {
            Debug.Log("<color=green>No missing or extra prefabs.</color>");
        }
    }

    private void SelectMissingPrefabs()
    {
        if (parentPrefab == null || childPrefabs == null || childPrefabs.Count == 0)
        {
            Debug.LogError("<color=red>Please select a parent prefab and child prefabs first.</color>");
            return;
        }

        var childPrefabNames = new HashSet<string>();
        foreach (var prefab in childPrefabs)
        {
            childPrefabNames.Add(prefab.name);
        }

        var parentPrefabNames = new HashSet<string>();
        AddChildPrefabNamesRecursively(parentPrefab.transform, parentPrefabNames);

        var missingPrefabs = new List<string>();
        foreach (var name in childPrefabNames)
        {
            if (!parentPrefabNames.Contains(name))
            {
                missingPrefabs.Add(name);
            }
        }

        List<Object> prefabsToSelect = new List<Object>();
        foreach (var missingPrefab in missingPrefabs)
        {
            string[] guids = AssetDatabase.FindAssets(missingPrefab);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object prefab = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (prefab != null)
                {
                    prefabsToSelect.Add(prefab);
                }
            }
        }

        Selection.objects = prefabsToSelect.ToArray();
        Debug.Log("<color=green>Selected missing prefabs in Project window.</color>");
    }

    private void AddChildPrefabNamesRecursively(Transform parentTransform, HashSet<string> prefabNames)
    {
        foreach (Transform child in parentTransform)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject))
            {
                prefabNames.Add(child.name);
            }
            else
            {
                AddChildPrefabNamesRecursively(child, prefabNames);
            }
        }
    }
}
#endif
