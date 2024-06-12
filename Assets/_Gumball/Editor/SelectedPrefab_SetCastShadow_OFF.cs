using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

public class ShadowCasterTool : Editor
{
    [MenuItem("Tools/Disable Shadow Casting for Selected Prefabs")]
    static void DisableShadowCasting()
    {
        // Ensure the user has selected something in the project window.
        if (Selection.objects.Length == 0)
        {
            Debug.LogWarning("No prefabs selected. Please select prefabs in the Project window.");
            return;
        }

        // Iterate through all selected objects.
        foreach (Object selectedObject in Selection.objects)
        {
            // Load the prefab.
            string path = AssetDatabase.GetAssetPath(selectedObject);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
            {
                // Get all MeshRenderers in the prefab.
                MeshRenderer[] meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);

                // Keep track if any changes were made.
                bool hasChanged = false;

                foreach (MeshRenderer renderer in meshRenderers)
                {
                    // Check if the shadow casting option is on, and turn it off.
                    if (renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)
                    {
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        hasChanged = true;
                    }
                }

                // Save the changes to the prefab.
                if (hasChanged)
                {
                    EditorUtility.SetDirty(prefab);
                    PrefabUtility.SavePrefabAsset(prefab);
                    Debug.Log($"Updated prefab: {prefab.name}");
                }
            }
        }

        Debug.Log("Shadow casting disabled for all selected prefabs.");
    }
}
#endif
