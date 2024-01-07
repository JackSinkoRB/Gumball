using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gumball.Editor
{
    public class SaveEditorAssetsEvents : AssetModificationProcessor
    {
    
        public delegate void SceneSaveDelegate(string sceneName, string path);
        public static event SceneSaveDelegate onSaveScene;
        
        public delegate void PrefabSaveDelegate(string sceneName, string path);
        public static event PrefabSaveDelegate onSavePrefab;

        private static bool isSaving;
        
        private static string[] OnWillSaveAssets(string[] paths)
        {
            if (isSaving)
                return paths;
            
            isSaving = true;

            try
            {
                foreach (string path in paths)
                {
                    if (path.Contains(".unity"))
                    {
                        string sceneName = Path.GetFileNameWithoutExtension(path);
                        onSaveScene?.Invoke(sceneName, path);
                    }

                    if (path.Contains(".prefab"))
                    {
                        string sceneName = Path.GetFileNameWithoutExtension(path);
                        onSavePrefab?.Invoke(sceneName, path);
                    }
                }
            }
            finally
            {
                isSaving = false;
            }
            
            return paths;
        }
    
    }
}
