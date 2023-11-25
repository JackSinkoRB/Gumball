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
    public class MagneticScrollTestManager : ScriptableObject
    {

        private static  MagneticScrollTestManager instance;
        public static MagneticScrollTestManager Instance
        {
            get
            {
                if (instance == null)
                {
                    AsyncOperationHandle<MagneticScrollTestManager> handle = Addressables.LoadAssetAsync<MagneticScrollTestManager>(nameof(MagneticScrollTestManager));
                    instance = handle.WaitForCompletion();
                }
                return instance;
            }
        }

        [SerializeField] private SceneAsset testScene;

        public string TestScenePath => AssetDatabase.GetAssetPath(testScene);

    }
}
