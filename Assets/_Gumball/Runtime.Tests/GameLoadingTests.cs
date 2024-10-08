using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class GameLoadingTests : BaseRuntimeTests
    {
        
        protected override string sceneToLoadPath => TestManager.Instance.BootScenePath;

        [SetUp]
        public void SetUp()
        {
            DataManager.RemoveAllData();
        }

        [UnityTest]
        [Order(1)]
        public IEnumerator GameLoadsSuccessfully()
        {
            Debug.Log("Starting GameLoadsSuccessfully");
            if (Application.isBatchMode)
            {
                //ignore these as there's no graphics when running batch mode in CI/CD
                LogAssert.Expect(LogType.Exception, new Regex(@".*colors\.Length is 2 and exceeds the maximum number of supported render targets \(1\).*"));
                LogAssert.Expect(LogType.Error, new Regex(@".*Trying to set 2 renderTargets, which is more than the maximum supported:1.*"));
            }
            
            yield return new WaitUntil(() => sceneHasLoaded);
            
            const float maxLoadTimeAllowed = 180; //in seconds
            
            float totalTimeWaiting = 0;
            while (true)
            {
                if (GameLoaderSceneManager.HasLoaded)
                    break;
                
                yield return new WaitForSeconds(1);
                totalTimeWaiting += 1;

                if (totalTimeWaiting > maxLoadTimeAllowed)
                    break;
            }
            
            Assert.Less(totalTimeWaiting, maxLoadTimeAllowed);
        }
        
        [UnityTest]
        [Order(2)]
        public IEnumerator FoundCoreParts()
        {
            yield return new WaitUntil(() => sceneHasLoaded);
            Assert.IsTrue(GameLoaderSceneManager.HasLoaded);
            
            Assert.IsTrue(CorePartManager.AllParts.Count > 0);
        }
        
        [UnityTest]
        [Order(3)]
        public IEnumerator FoundSubParts()
        {
            yield return new WaitUntil(() => sceneHasLoaded);
            Assert.IsTrue(GameLoaderSceneManager.HasLoaded);
            
            Assert.IsTrue(SubPartManager.AllParts.Count > 0);
        }

    }
}
