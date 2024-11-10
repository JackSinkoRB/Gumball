using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Gumball.Runtime.Tests
{
    public class BaseRuntimeTests : IPrebuildSetup, IPostBuildCleanup
    {

        protected bool sceneHasLoaded;
        
        protected virtual string sceneToLoadPath => null;

        public void Setup()
        {
            sceneHasLoaded = false;
            
            BootSceneClear.TrySetup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = true;
        }

        public void Cleanup()
        {
            sceneHasLoaded = false;
            
            BootSceneClear.TryCleanup();

            SingletonScriptableHelper.LazyLoadingEnabled = false;
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            DecalEditor.IsRunningTests = true;
            IAPManager.IsRunningTests = true;
            ChunkManager.IsRunningTests = true;
            PersistentCooldown.IsRunningTests = true;
            
            PlayFabManager.DisableServerTime = true;

            UniversalRenderPipelineUtils.SetRendererFeatureActive("Decal", false);
            
            DataManager.EnableTestProviders(true);

            if (sceneToLoadPath != null)
            {
                AsyncOperation loadScene = EditorSceneManager.LoadSceneAsyncInPlayMode(sceneToLoadPath, new LoadSceneParameters(LoadSceneMode.Single));
                loadScene.completed += OnSceneLoadComplete;
            }
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
            
            UniversalRenderPipelineUtils.SetRendererFeatureActive("Decal", true); //reenable

            //destroy the car instance
            if (WarehouseManager.HasLoaded && WarehouseManager.Instance.CurrentCar != null)
                Object.DestroyImmediate(WarehouseManager.Instance.CurrentCar.gameObject);
        }

        protected virtual void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            sceneHasLoaded = true;
        }

    }
}
