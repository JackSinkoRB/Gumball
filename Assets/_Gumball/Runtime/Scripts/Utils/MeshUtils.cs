using System.Collections;
using System.Collections.Generic;
using System.IO;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    public static class MeshUtils
    {

#if UNITY_EDITOR
        public static void SetReadable(this Mesh mesh)
        {
            if (mesh == null)
                return;

            if (mesh.isReadable)
                return; //is already readable
            
            string assetPath = AssetDatabase.GetAssetPath(mesh);
            if (assetPath.IsNullOrEmpty())
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
                if (lines[i].Contains($"{propertyKey}: 0"))
                {
                    //modify the value here (change 0 to 1)
                    lines[i] = lines[i].Replace($"{propertyKey}: 0", $"{propertyKey}: 1");
                    modified = true;
                }
            }
            
            if (modified)
            {
                File.WriteAllLines(assetMetaPath, lines);
            
                EditorApplication.delayCall += AssetDatabase.Refresh;
                
                Debug.Log($"Set mesh {mesh.name} readable.");
            }
        }
#endif

    }
}
