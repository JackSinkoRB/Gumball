using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MagneticScrollUtils.Tests.Runtime
{
    [CreateAssetMenu(menuName = "MagneticScroll/Test Manager")]
    public class TestManager : ScriptableObject
    {

        private static  TestManager instance;
        public static TestManager Instance
        {
            get
            {
                if (instance == null)
                {
                    AsyncOperationHandle<TestManager> handle = Addressables.LoadAssetAsync<TestManager>(nameof(TestManager));
                    instance = handle.WaitForCompletion();
                }
                return instance;
            }
        }

        [SerializeField] private SceneAsset testScene;

        public string TestScenePath => AssetDatabase.GetAssetPath(testScene);

    }
}
