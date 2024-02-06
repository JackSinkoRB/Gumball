using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Gumball.Runtime.Tests
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Test Manager")]
    public class TestManager : ScriptableObject
    {

        private static TestManager instance;
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

        [SerializeField] private SceneAsset decalEditorScene;
        [SerializeField] private SceneAsset avatarEditorScene;
        [SerializeField] private SceneAsset chunkTestingScene;

        public string DecalEditorScenePath => AssetDatabase.GetAssetPath(decalEditorScene);
        public string AvatarEditorScenePath => AssetDatabase.GetAssetPath(avatarEditorScene);
        public string ChunkTestingScenePath => AssetDatabase.GetAssetPath(chunkTestingScene);

    }
}
