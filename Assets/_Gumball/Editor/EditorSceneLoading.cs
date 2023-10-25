using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Gumball.Editor
{
    [InitializeOnLoad]
    public static class EditorSceneLoading
    {

        private const string ToggleBootSceneLoading = "Gumball/Load BootScene On Play";

        public static bool BootSceneLoadingEnabled
        {
            get => EditorPrefs.GetBool(ToggleBootSceneLoading, true);
            set => EditorPrefs.SetBool(ToggleBootSceneLoading, value);
        }
        
        static EditorSceneLoading()
        {
            UpdatePlayModeStartScene();
        }

        private static void UpdatePlayModeStartScene()
        {
            if (BootSceneLoadingEnabled)
            {
                //use the boot scene
                string pathOfFirstScene = EditorBuildSettings.scenes[0].path;
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
                EditorSceneManager.playModeStartScene = sceneAsset;
            }
            else
            {
                EditorSceneManager.playModeStartScene = null;
            }
        }
        
        [MenuItem(ToggleBootSceneLoading)]
        private static void ToggleBootSceneLoadOption()
        {
            BootSceneLoadingEnabled = !BootSceneLoadingEnabled;
            UpdatePlayModeStartScene();
        }

        [MenuItem(ToggleBootSceneLoading, true)]
        private static bool ToggleBootSceneLoadOptionValidation()
        {
            Menu.SetChecked(ToggleBootSceneLoading, BootSceneLoadingEnabled);
            return true;
        }
        
    }
}
