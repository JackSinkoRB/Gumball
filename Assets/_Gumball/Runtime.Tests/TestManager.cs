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
        [SerializeField] private SceneAsset chunkMapScene;
        [SerializeField] private SceneAsset workshopScene;

        [Header("Chunks")]
        [SerializeField] private GameSession chunkTestingSession;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabA;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabB;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabC;
        [SerializeField] private AssetReferenceGameObject testChunkPrefabCustomLoad;
        
        [Header("Core parts")]
        [SerializeField] private CorePart corePartA;
        [SerializeField] private CorePart corePartB;

        [Header("Sub parts")]
        [SerializeField] private SubPart subPartA;
        [SerializeField] private SubPart subPartB;
        
        [Header("Warehouse manager")]
        [SerializeField] private GameObject carTemplatePrefab;
        
        public string BootScenePath => AssetDatabase.GetAssetPath(bootScene);
        public string DecalEditorScenePath => AssetDatabase.GetAssetPath(decalEditorScene);
        public string AvatarEditorScenePath => AssetDatabase.GetAssetPath(avatarEditorScene);
        public string ChunkMapScenePath => AssetDatabase.GetAssetPath(chunkMapScene);
        public string WorkshopScenePath => AssetDatabase.GetAssetPath(workshopScene);

        public GameSession ChunkTestingSession => chunkTestingSession;
        public AssetReferenceGameObject TestChunkPrefabA => testChunkPrefabA;
        public AssetReferenceGameObject TestChunkPrefabB => testChunkPrefabB;
        public AssetReferenceGameObject TestChunkPrefabC => testChunkPrefabC;
        public AssetReferenceGameObject TestChunkPrefabCustomLoad => testChunkPrefabCustomLoad;

        public CorePart CorePartA => corePartA;
        public CorePart CorePartB => corePartB;

        public SubPart SubPartA => subPartA;
        public SubPart SubPartB => subPartB;

        public GameObject CarTemplatePrefab => carTemplatePrefab;

    }
}
