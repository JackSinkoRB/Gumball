using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dreamteck.Splines
{
    public static class MeshUtils
    {

#if UNITY_EDITOR
        public static void SetReadable(this Mesh mesh, bool readable = true, Action onComplete = null)
        {
            if (mesh == null)
                return;

            if (mesh.isReadable == readable)
                return; //is already set
            
            string assetPath = AssetDatabase.GetAssetPath(mesh);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"Cannot set mesh filter {mesh.name} readable, because the mesh isn't an asset.");
                return;
            }
            
            string assetMetaPath = $"{assetPath}.meta";

            string[] lines = File.ReadAllLines(assetMetaPath);

            const string propertyKey = "isReadable";
            bool modified = false;
            for (int i = 0; i < lines.Length; i++)
            {
                int desiredValue = readable ? 1 : 0;
                int otherValue = readable ? 0 : 1;
                if (lines[i].Contains($"{propertyKey}: {otherValue}"))
                {
                    //modify the value here (change 0 to 1)
                    lines[i] = lines[i].Replace($"{propertyKey}: {otherValue}", $"{propertyKey}: {desiredValue}");
                    modified = true;
                }
            }
            
            if (modified)
            {
                File.WriteAllLines(assetMetaPath, lines);
            
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh();
                    onComplete?.Invoke();
                };
                
                Debug.Log($"Set mesh {mesh.name} readable ({readable}).");
            }
        }
#endif

    }
}