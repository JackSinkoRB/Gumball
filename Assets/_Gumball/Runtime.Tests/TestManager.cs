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

        [SerializeField] private SceneAsset bootScene;
        [SerializeField] private SceneAsset decalEditorScene;
        [SerializeField] private SceneAsset avatarEditorScene;
        [SerializeField] private SceneAsset mapDrivingScene;

        [Header("Chunks")]
        [SerializeField] private MapData chunkTestingMap;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabA;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabB;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabC;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabCustomLoad;

        public string DecalEditorScenePath => AssetDatabase.GetAssetPath(decalEditorScene);
        public string AvatarEditorScenePath => AssetDatabase.GetAssetPath(avatarEditorScene);
        public string BootScenePath => AssetDatabase.GetAssetPath(bootScene);
        public string MapDrivingScenePath => AssetDatabase.GetAssetPath(mapDrivingScene);

        public MapData ChunkTestingMap => chunkTestingMap;
        public AssetReferenceGameObject TestChunkPrefabA => testChunkPrefabA;
        public AssetReferenceGameObject TestChunkPrefabB => testChunkPrefabB;
        public AssetReferenceGameObject TestChunkPrefabC => testChunkPrefabC;
        public AssetReferenceGameObject TestChunkPrefabCustomLoad => testChunkPrefabCustomLoad;

    }
}
