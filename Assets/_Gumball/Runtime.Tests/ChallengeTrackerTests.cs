using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class ChallengeTrackerTests : BaseRuntimeTests
    {

        private bool isInitialised;
        
        private GameSession GameSession => TestManager.Instance.ChunkTestingSession;

        [OneTimeSetUp]
        public override void OneTimeSetUp()
        {
            base.OneTimeSetUp();
            
            AsyncOperation loadMapScene = EditorSceneManager.LoadSceneAsyncInPlayMode(TestManager.Instance.ChunkMapScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMapScene.completed += OnSceneLoadComplete;
        }

        [OneTimeTearDown]
        public override void OneTimeTearDown()
        {
            base.OneTimeTearDown();
            
            GameSession.EndSession(GameSession.ProgressStatus.NOT_ATTEMPTED);
        }

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            isInitialised = true;
        }

        [Test]
        [Order(1)]
        public void StartAndStopTracking()
        {
            ChallengeTracker tracker = ChallengeTrackerManager.Instance.Trackers[0];
            const string id = "Test";
            
            tracker.StartListening(id, 100);
            Assert.IsNotNull(tracker.GetListener(id));
            
            tracker.StopListening(id);
            Assert.IsNull(tracker.GetListener(id));
        }
        
        [Test]
        [Order(2)]
        public void Track()
        {
            ChallengeTracker tracker = ChallengeTrackerManager.Instance.Trackers[0];
            const string id = "Test";
            
            tracker.StartListening(id, 100);

            tracker.Track(50);
            Assert.AreEqual(0.5f, tracker.GetListener(id).Progress);
            
            tracker.Track(20);
            Assert.AreEqual(0.7f, tracker.GetListener(id).Progress);
        }
        
        [Test]
        [Order(3)]
        public void SetTracker()
        {
            ChallengeTracker tracker = ChallengeTrackerManager.Instance.Trackers[0];
            const string id = "Test";
            
            tracker.StartListening(id, 100);

            tracker.Track(50);
            Assert.AreEqual(0.5f, tracker.GetListener(id).Progress);
            
            tracker.SetListenerValues(20);
            Assert.AreEqual(0.2f, tracker.GetListener(id).Progress);
        }
        
    }
}
