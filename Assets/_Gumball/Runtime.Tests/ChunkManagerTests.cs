using System.Collections;
using System.Collections.Generic;
using MyBox;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ChunkManagerTests : IPrebuildSetup, IPostBuildCleanup
    {

        private const float chunkSplineLengths = 100;

        private bool isInitialised;
        private MapData map => TestManager.Instance.ChunkTestingMap;
        
        public void Setup()
        {
            BootSceneClear.TrySetup();
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DataManager.EnableTestProviders(true);
            
            AsyncOperation loadMainScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.BootScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMainScene.completed += OnBootSceneLoadComplete;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }
        
        private void OnBootSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(LoadMap());
        }
        
        private IEnumerator LoadMap()
        {
            yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Equals(SceneManager.MainSceneName));
            
            MapDrivingSceneManager.LoadMapDrivingScene(map);
            
            yield return new WaitUntil(() => ChunkManager.Instance.HasLoaded);
            
            isInitialised = true;
        }
        
        [UnityTest]
        [Order(0)]
        public IEnumerator TestsAreReady()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsTrue(ChunkManager.ExistsRuntime);

            //yield return new WaitUntil(CarDoesntHaveMeshCollider);
            Assert.IsTrue(CarDoesntHaveMeshCollider());
            
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().SplineLength);
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabB.editorAsset.GetComponent<Chunk>().SplineLength);
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabC.editorAsset.GetComponent<Chunk>().SplineLength);
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabCustomLoad.editorAsset.GetComponent<Chunk>().SplineLength);

            Assert.AreEqual(500, map.ChunkLoadDistance);
        }

        private bool CarDoesntHaveMeshCollider()
        {
            List<MeshCollider> meshColliders = PlayerCarManager.Instance.CurrentCar.transform.GetComponentsInAllChildren<MeshCollider>();
            return meshColliders.Count == 0;
        }
        
        [UnityTest]
        [Order(1)]
        public IEnumerator OnlyLoadChunksInLoadDistance()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsTrue(CarDoesntHaveMeshCollider());

            float carDistance = map.VehicleStartingPosition.z;
            Assert.AreEqual(Mathf.CeilToInt((map.ChunkLoadDistance + carDistance) / chunkSplineLengths), ChunkManager.Instance.CurrentChunks.Count);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator ChunksInMapOrder()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[0].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabB.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[1].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabC.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[2].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabC.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[3].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabB.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[4].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[5].Chunk.UniqueID);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator CustomLoadedChunk()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(1, ChunkManager.Instance.CurrentCustomLoadedChunks.Count);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabCustomLoad.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentCustomLoadedChunks[0].Chunk.UniqueID);
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator LoadedChunksIndices()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(new MinMaxInt(0, 5), ChunkManager.Instance.LoadedChunksIndices);
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator ChunksLoadAfterMovingCar()
        {
            yield return new WaitUntil(() => isInitialised);

            Vector3 startOfChunk7 = map.GetChunkData(7).Position;
            PlayerCarManager.Instance.CurrentCar.Teleport(startOfChunk7, Quaternion.Euler(Vector3.zero));
            yield return new WaitForFixedUpdate();
            
            ChunkManager.Instance.DoLoadingCheck(true);
            
            yield return ChunkManager.Instance.DistanceLoadingCoroutine.Coroutine;
            
            Chunk chunkPlayerIsOn = ChunkManager.Instance.GetChunkPlayerIsOn();
            Assert.AreEqual(7, ChunkManager.Instance.GetMapIndexOfLoadedChunk(chunkPlayerIsOn));
            
            Assert.AreEqual(new MinMaxInt(1, 10), ChunkManager.Instance.LoadedChunksIndices);

            Assert.AreEqual(9, ChunkManager.Instance.CurrentChunks.Count);
            Assert.AreEqual(1, ChunkManager.Instance.CurrentCustomLoadedChunks.Count);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabB.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[0].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabC.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[1].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabC.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[2].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabB.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[3].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[4].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[5].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[6].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabC.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[7].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentChunks[8].Chunk.UniqueID);
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabCustomLoad.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentCustomLoadedChunks[0].Chunk.UniqueID);
        }
        
    }
}
