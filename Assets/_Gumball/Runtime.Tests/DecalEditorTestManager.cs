using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball.Runtime.Tests
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Decal Editor Test Manager")]
    public class DecalEditorTestManager : ScriptableObject
    {

        private static DecalEditorTestManager instance;
        public static DecalEditorTestManager Instance
        {
            get
            {
                if (instance == null)
                {
                    AsyncOperationHandle<DecalEditorTestManager> handle = Addressables.LoadAssetAsync<DecalEditorTestManager>(nameof(DecalEditorTestManager));
                    instance = handle.WaitForCompletion();
                }
                return instance;
            }
        }

        [SerializeField] private SceneAsset testScene;
        [SerializeField] private AssetReferenceGameObject carManager;

        public string TestScenePath => AssetDatabase.GetAssetPath(testScene);
        public AssetReferenceGameObject CarManager => carManager;

    }
}
