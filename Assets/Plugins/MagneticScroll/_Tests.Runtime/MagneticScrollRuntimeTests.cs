using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace MagneticScrollUtils.Tests.Runtime
{
    public class MagneticScrollRuntimeTests : IPrebuildSetup, IPostBuildCleanup
    {
        
        private static SceneAsset loadBootScene;

        private bool isInitialised;
        private MagneticScrollTestSceneManager sceneManager;

        public void Setup()
        {
            loadBootScene = EditorSceneManager.playModeStartScene;
            EditorSceneManager.playModeStartScene = null;
        }

        public void Cleanup()
        {
            EditorSceneManager.playModeStartScene = loadBootScene;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AsyncOperation loadMainScene = EditorSceneManager.LoadSceneAsyncInPlayMode(MagneticScrollTestManager.Instance.TestScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadMainScene.completed += OnSceneLoadComplete;
        }

        private void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            sceneManager = Object.FindObjectOfType<MagneticScrollTestSceneManager>();
            isInitialised = true;
        }
        
        [UnityTest]
        public IEnumerator MagneticScrollIsSetup()
        {
            yield return new WaitUntil(() => isInitialised);

            MagneticScroll magneticScroll = sceneManager.PopulatedMagneticScroll;
            
            Assert.AreEqual(0, magneticScroll.Items.Count);
            Assert.AreEqual(5, magneticScroll.NumberOfIcons);
        }

        [UnityTest]
        public IEnumerator PopulateItemsNonInfiniteScroll()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(false);

            PopulateWithColors(magneticScroll, 10);
            
            Assert.AreEqual(10, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(magneticScroll.Icons.Count - 1, magneticScroll.LastIconIndex);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(magneticScroll.NumberOfIcons - 1, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }

        [UnityTest]
        public IEnumerator PopulateItemsNonInfiniteScrollNotEnoughItems()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(false);
            
            PopulateWithColors(magneticScroll, 3);
            
            Assert.AreEqual(3, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(magneticScroll.Items.Count - 1, magneticScroll.LastIconIndex);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(magneticScroll.Items.Count - 1, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator PopulateItemsInfiniteScroll()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 6);
            
            Assert.AreEqual(6, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(magneticScroll.Icons.Count - 1, magneticScroll.LastIconIndex);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(4, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator PopulateItemsInfiniteScrollNotEnoughItems()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 3);
            
            Assert.AreEqual(3, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(magneticScroll.Icons.Count - 1, magneticScroll.LastIconIndex);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator PopulateItemsNonInfiniteScrollSetSelected()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(false);

            PopulateWithColors(magneticScroll, 6, 1);
            
            Assert.AreEqual(6, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator PopulateItemsNonInfiniteScrollSetSelectedNotEnoughItems()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(false);

            PopulateWithColors(magneticScroll, 3, 1);
            
            Assert.AreEqual(3, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator PopulateItemsInfiniteScrollSetSelected()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 6, 1);
            
            Assert.AreEqual(6, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(magneticScroll.Icons.Count - 1, magneticScroll.LastIconIndex);

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(5, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator PopulateItemsInfiniteScrollSetSelectedNotEnoughItems()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 3, 1);
            
            Assert.AreEqual(3, magneticScroll.Items.Count);

            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(magneticScroll.Icons.Count - 1, magneticScroll.LastIconIndex);

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.LastItemIndexShowing);

            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator SnapToItemNonInfiniteScroll()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(false);

            PopulateWithColors(magneticScroll, 6);
            
            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();
            
            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(3, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(3, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(4, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(4, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(5, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            //shouldn't change:
            Assert.AreEqual(5, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            //go backwards:
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(4, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(4, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(3, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(3, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            //shouldn't change
            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator SnapToItemNonInfiniteScrollNotEnoughItems()
        {
            yield return new WaitUntil(() => isInitialised);
            
            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(false);

            PopulateWithColors(magneticScroll, 3);
            
            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();
            
            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();
            
            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();
            
            //shouldn't change
            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();
            
            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();
            
            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();
            
            //shouldn't change
            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);

            Object.Destroy(magneticScroll.gameObject);
        }

        [UnityTest]
        public IEnumerator SnapToItemInfiniteScrollNext()
        {
            yield return new WaitUntil(() => isInitialised);

            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 6);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(4, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);

            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(3, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(5, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(4, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.FirstIconIndex);
            Assert.AreEqual(1, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(3, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(4, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(2, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.FirstIconIndex);
            Assert.AreEqual(3, magneticScroll.LastIconIndex);

            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(5, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(3, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(3, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(4, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            Object.Destroy(magneticScroll.gameObject);
        }
        
        [UnityTest]
        public IEnumerator SnapToItemInfiniteScrollPreviousNotEnoughItems()
        {
            yield return new WaitUntil(() => isInitialised);

            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 3);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);

            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.FirstIconIndex);
            Assert.AreEqual(3, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(2, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(4, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.FirstIconIndex);
            Assert.AreEqual(1, magneticScroll.LastIconIndex);
            
            Object.Destroy(magneticScroll.gameObject);
        }

        [UnityTest]
        public IEnumerator SnapToItemInfiniteScrollNextNotEnoughItems()
        {
            yield return new WaitUntil(() => isInitialised);

            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 3);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);

            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(3, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(2, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(4, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.FirstIconIndex);
            Assert.AreEqual(1, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToNextItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            Object.Destroy(magneticScroll.gameObject);
        }

        [UnityTest]
        public IEnumerator SnapToItemInfiniteScrolPrevious()
        {
            yield return new WaitUntil(() => isInitialised);

            MagneticScroll magneticScroll = Object.Instantiate(sceneManager.PopulatedMagneticScroll.gameObject).GetComponent<MagneticScroll>();
            magneticScroll.ToggleUseInfiniteScroll(true);

            PopulateWithColors(magneticScroll, 6);

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(4, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);

            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(5, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(3, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.FirstIconIndex);
            Assert.AreEqual(3, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(4, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(2, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);

            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(3, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(4, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(1, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(5, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.FirstIconIndex);
            Assert.AreEqual(1, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(2, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(3, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(0, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.FirstIconIndex);
            Assert.AreEqual(0, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(1, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(2, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(5, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(0, magneticScroll.FirstIconIndex);
            Assert.AreEqual(4, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(0, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(1, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(4, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(2, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(4, magneticScroll.FirstIconIndex);
            Assert.AreEqual(3, magneticScroll.LastIconIndex);
            
            magneticScroll.SnapToPreviousItem();
            magneticScroll.CompleteAllTweens();

            Assert.AreEqual(5, magneticScroll.LastSelectedItemIndex);
            Assert.AreEqual(0, magneticScroll.LastSelectedIconIndex);
            Assert.AreEqual(3, magneticScroll.FirstItemIndexShowing);
            Assert.AreEqual(1, magneticScroll.LastItemIndexShowing);
            Assert.AreEqual(3, magneticScroll.FirstIconIndex);
            Assert.AreEqual(2, magneticScroll.LastIconIndex);
            
            Object.Destroy(magneticScroll.gameObject);
        }

        private void PopulateWithColors(MagneticScroll magneticScroll, int itemsToUse, int selectedIndex = 0)
        {
            //populate
            List<ScrollItem> scrollItems = new List<ScrollItem>();
            for (int count = 0; count < itemsToUse; count++)
            {
                Color color = sceneManager.PopulateColors[count % sceneManager.PopulateColors.Length];

                ScrollItem scrollItem = new ScrollItem();
                scrollItem.onLoad += () => scrollItem.CurrentIcon.ImageComponent.color = color;

                scrollItems.Add(scrollItem);
            }

            magneticScroll.SetItems(scrollItems, selectedIndex);
        }

    }
}
