using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public static class BootSceneClear
    {

        public static bool IsSetUp;
        public static SceneAsset BootSceneSetting;

        public static void TrySetup()
        {
            if (IsSetUp)
                return;
            
            IsSetUp = true;
            BootSceneSetting = EditorSceneManager.playModeStartScene;
            EditorSceneManager.playModeStartScene = null;
        }
        
        public static void TryCleanup()
        {
            if (!IsSetUp)
                return;
            
            IsSetUp = false;
            EditorSceneManager.playModeStartScene = BootSceneSetting;
        }
    }
}
