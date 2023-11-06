using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Gumball.Editor
{
    public class SaveEditorAssetsEvents : AssetModificationProcessor
    {
    
        public delegate void SceneSaveDelegate(string sceneName);
        public static event SceneSaveDelegate onSaveScene;
    
        private static string[] OnWillSaveAssets(string[] paths)
        {
            // Get the name of the scene to save.
            string scenePath = string.Empty;
            string sceneName = string.Empty;

            foreach (string path in paths)
            {
                if (path.Contains(".unity"))
                {
                    scenePath = Path.GetDirectoryName(path);
                    sceneName = Path.GetFileNameWithoutExtension(path);
                }
            }

            if (sceneName.Length == 0)
            {
                return paths;
            }
        
            onSaveScene?.Invoke(sceneName);

            return paths;
        }
    
    }
}
