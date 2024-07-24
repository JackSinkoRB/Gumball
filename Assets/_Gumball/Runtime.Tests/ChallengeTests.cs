using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ChallengeTests : IPrebuildSetup, IPostBuildCleanup
    {

        private bool isInitialised;
        
        private GameSession GameSession => TestManager.Instance.ChunkTestingSession;
        
        public void Setup()
        {
            BootSceneClear.TrySetup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = true;
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = false;
        }
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            DecalEditor.IsRunningTests = true;
            DataManager.EnableTestProviders(true);
            
            AsyncOperation loadMapScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.ChunkMapScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMapScene.completed += OnSceneLoadComplete;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
            
            GameSession.EndSession(GameSession.ProgressStatus.NOT_ATTEMPTED);
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
            yield return WarehouseManager.Instance.SpawnCar(0, new Vector3(0,0,2), Quaternion.Euler(Vector3.zero), (car) => WarehouseManager.Instance.SetCurrentCar(car));
            // yield return GameSession.LoadChunkMap();
            // yield return GameSession.SetupSession();
            // GameSessionManager.Instance.SetCurrentSession(GameSession);
            //
            // GameSession.StartTrackingObjectives();
            
            ChallengeTrackerManager.LoadInstanceAsync();
            yield return new WaitUntil(() => ChallengeTrackerManager.HasLoaded);

            GameSession.StartSession();
            yield return new WaitUntil(() => GameSession.HasStarted);
            
            isInitialised = true;
        }
        
        [Test]
        [Order(1)]
        public void StartAndStopTracking()
        {
            ChallengeTracker tracker = ChallengeTrackerManager.Instance.Trackers[0];
            const string id = "Test";
            
            tracker.StartTracking(id, 100);
            Assert.IsNotNull(tracker.GetTracker(id));
            
            tracker.StopTracking(id);
            Assert.IsNull(tracker.GetTracker(id));
        }
        
        [Test]
        [Order(2)]
        public void Track()
        {
            ChallengeTracker tracker = ChallengeTrackerManager.Instance.Trackers[0];
            const string id = "Test";
            
            tracker.StartTracking(id, 100);

            tracker.Track(50);
            Assert.AreEqual(0.5f, tracker.GetTracker(id).Progress);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator DrivingDistance()
        {
            yield return new WaitUntil(() => isInitialised);
            
            Challenge subObjective = GameSession.SubObjectives[0];
            ChallengeTracker tracker = subObjective.Tracker;
            string trackerId = GameSession.GetChallengeTrackerID(subObjective);
            
            Assert.IsNotNull(tracker.GetTracker(trackerId));

            //ensure teleport doesn't add to it
            float trackerBeforeTeleport = tracker.GetTracker(trackerId).Progress;
            WarehouseManager.Instance.CurrentCar.Teleport(new Vector3(1,0,2), Quaternion.Euler(Vector3.zero));
            yield return null;
            Assert.AreEqual(trackerBeforeTeleport, tracker.GetTracker(trackerId).Progress);

            WarehouseManager.Instance.CurrentCar.SetAutoDrive(true);
            WarehouseManager.Instance.CurrentCar.SetSpeed(100);
            
            yield return null;

            Assert.Greater(tracker.GetTracker(trackerId).Progress, 0);
        }

    }
}
