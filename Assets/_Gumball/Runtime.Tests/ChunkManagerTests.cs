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
            
            AsyncOperation loadMainScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.MapDrivingScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMainScene.completed += OnSceneLoadComplete;
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
        
        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            CoroutineHelper.Instance.StartCoroutine(Initialise());
        }
        
        private IEnumerator Initialise()
        {
            yield return PlayerCarManager.Instance.SpawnCar(Vector3.zero, Quaternion.Euler(Vector3.zero));
            yield return MapDrivingSceneManager.SetupMapDrivingScene(map);
            
            isInitialised = true;
        }
        
        [UnityTest]
        [Order(0)]
        public IEnumerator TestsAreReady()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.IsTrue(ChunkManager.ExistsRuntime);
            
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabA.editorAsset.GetComponent<Chunk>().SplineLength);
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabB.editorAsset.GetComponent<Chunk>().SplineLength);
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabC.editorAsset.GetComponent<Chunk>().SplineLength);
            Assert.AreEqual(chunkSplineLengths, TestManager.Instance.TestChunkPrefabCustomLoad.editorAsset.GetComponent<Chunk>().SplineLength);

            Assert.AreEqual(500, map.ChunkLoadDistance);
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator OnlyLoadChunksInLoadDistance()
        {
            yield return new WaitUntil(() => isInitialised);
            
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
            //make sure the custom loaded chunk is waiting to be accessible
            Assert.AreEqual(1, ChunkManager.Instance.ChunksWaitingToBeAccessible.Count);
        }
        
        [UnityTest]
        [Order(4)]
        public IEnumerator ChunkIndices()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Assert.AreEqual(0, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Max);
            
            Assert.AreEqual(0, ChunkManager.Instance.AccessibleChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.AccessibleChunksIndices.Max);
        }

        private IEnumerator MoveAndLoadAroundPosition(Vector3 position)
        {
            PlayerCarManager.Instance.CurrentCar.Rigidbody.isKinematic = true;
            PlayerCarManager.Instance.CurrentCar.Teleport(position, Quaternion.Euler(Vector3.zero));

            ChunkManager.Instance.HasLoaded = false; //don't slow down loading
            yield return ChunkManager.Instance.LoadChunksAroundPosition(position);
            
            PlayerCarManager.Instance.CurrentCar.Rigidbody.isKinematic = false;
            yield return new WaitForFixedUpdate();
        }
        
        [UnityTest]
        [Order(5)]
        public IEnumerator ChunksLoadAfterMovingCar()
        {
            yield return new WaitUntil(() => isInitialised);

            Assert.AreEqual(1, ChunkManager.Instance.ChunksWaitingToBeAccessible.Count);
            
            yield return MoveAndLoadAroundPosition(new Vector3(0, 5, 710));
            
            Assert.AreEqual(8, ChunkManager.Instance.CurrentChunks.Count);
            Assert.AreEqual(1, ChunkManager.Instance.CurrentCustomLoadedChunks.Count);
            
            Assert.AreEqual(2, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Min);
            Assert.AreEqual(9, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Max);
            
            Assert.AreEqual(2, ChunkManager.Instance.AccessibleChunksIndices.Min);
            Assert.AreEqual(10, ChunkManager.Instance.AccessibleChunksIndices.Max);

            //chunk player is on:
            Assert.AreEqual(7, ChunkManager.Instance.GetMapIndexOfLoadedChunk(ChunkManager.Instance.GetChunkPlayerIsOn()));
            
            //check that the custom loaded chunk is the correct chunk
            Assert.AreEqual(TestManager.Instance.TestChunkPrefabCustomLoad.editorAsset.GetComponent<Chunk>().UniqueID, ChunkManager.Instance.CurrentCustomLoadedChunks[0].Chunk.UniqueID);
            
            //make sure the custom loaded chunk is no longer waiting to be accessible
            Assert.AreEqual(0, ChunkManager.Instance.ChunksWaitingToBeAccessible.Count);
            
            yield return MoveAndLoadAroundPosition(map.VehicleStartingPosition);
            
            Assert.AreEqual(6, ChunkManager.Instance.CurrentChunks.Count);
            Assert.AreEqual(1, ChunkManager.Instance.CurrentCustomLoadedChunks.Count);
            
            Assert.AreEqual(0, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Max);
            
            Assert.AreEqual(0, ChunkManager.Instance.AccessibleChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.AccessibleChunksIndices.Max);

            //chunk player is on:
            Assert.AreEqual(0, ChunkManager.Instance.GetMapIndexOfLoadedChunk(ChunkManager.Instance.GetChunkPlayerIsOn()));
        }

        [UnityTest]
        [Order(6)]
        public IEnumerator CustomLoadedChunkAccessibility()
        {
            yield return new WaitUntil(() => isInitialised);

            //ensure chunks are in correct position
            Assert.AreEqual(6, ChunkManager.Instance.CurrentChunks.Count);

            Assert.AreEqual(0, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Max);
            
            Assert.AreEqual(0, ChunkManager.Instance.AccessibleChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.AccessibleChunksIndices.Max);
            
            Assert.AreEqual(1, ChunkManager.Instance.CurrentCustomLoadedChunks.Count);
            //make sure the custom loaded chunk is waiting to be accessible
            Assert.AreEqual(1, ChunkManager.Instance.ChunksWaitingToBeAccessible.Count);
            
            yield return MoveAndLoadAroundPosition(new Vector3(0, 5, 410));
            
            //not waiting anymore
            Assert.AreEqual(0, ChunkManager.Instance.ChunksWaitingToBeAccessible.Count);
            
            Assert.AreEqual(0, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Min);
            Assert.AreEqual(9, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Max);
            
            Assert.AreEqual(0, ChunkManager.Instance.AccessibleChunksIndices.Min);
            Assert.AreEqual(10, ChunkManager.Instance.AccessibleChunksIndices.Max);
            
            yield return MoveAndLoadAroundPosition(map.VehicleStartingPosition);

            //waiting again
            Assert.AreEqual(1, ChunkManager.Instance.ChunksWaitingToBeAccessible.Count);
            
            Assert.AreEqual(0, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.LoadingOrLoadedChunksIndices.Max);
            
            Assert.AreEqual(0, ChunkManager.Instance.AccessibleChunksIndices.Min);
            Assert.AreEqual(5, ChunkManager.Instance.AccessibleChunksIndices.Max);
        }

    }
}
