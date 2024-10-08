using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class BaseRuntimeTests : IPrebuildSetup, IPostBuildCleanup
    {

        public void Setup()
        {
            BootSceneClear.TrySetup();
            
            SingletonScriptableHelper.LazyLoadingEnabled = true;

            //switch the render pipeline asset to the one that is supported in batch mode (decals removed etc.)
            const int indexOfRenderer = 2;
            Camera.main.GetComponent<UniversalAdditionalCameraData>().SetRenderer(indexOfRenderer);
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
            
            //change back to the default renderer
            Camera.main.GetComponent<UniversalAdditionalCameraData>().SetRenderer(0);

            SingletonScriptableHelper.LazyLoadingEnabled = false;
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            DecalEditor.IsRunningTests = true;
            IAPManager.IsRunningTests = true;
            ChunkManager.IsRunningTests = true;
            PersistentCooldown.IsRunningTests = true;

            DataManager.EnableTestProviders(true);
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
        }
        
    }
}
