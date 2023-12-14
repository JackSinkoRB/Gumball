using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace MagneticScrollUtils.Tests.Runtime
{
    public class BootSceneClear : IPrebuildSetup, IPostBuildCleanup
    {
        
        private static SceneAsset bootSceneSetting;
        
        public void Setup()
        {
            bootSceneSetting = EditorSceneManager.playModeStartScene;
            EditorSceneManager.playModeStartScene = null;
        }

        public void Cleanup()
        {
            EditorSceneManager.playModeStartScene = bootSceneSetting;
        }

        [UnityTest]
        public IEnumerator BootSceneClearIsSetup()
        {
            Assert.IsTrue(true);
            yield break;
        }

    }
}
