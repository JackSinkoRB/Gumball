using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace Gumball.Runtime.Tests
{
    public class BaseRuntimeTests : IPrebuildSetup, IPostBuildCleanup
    {

        private RenderPipelineAsset previousRenderPipelineAsset;
        
        public void Setup()
        {
            BootSceneClear.TrySetup();

            //switch the render pipeline asset to the one that is supported in batch mode (decals removed etc.)
            RenderPipelineAsset lightweightAsset = Resources.Load<RenderPipelineAsset>("URP_Renderer UNIT TESTS"); 
            GraphicsSettings.renderPipelineAsset = lightweightAsset;
        }

        public void Cleanup()
        {
            BootSceneClear.TryCleanup();
            
            //change back to the previous render pipeline asset
            GraphicsSettings.renderPipelineAsset = previousRenderPipelineAsset;
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            DecalEditor.IsRunningTests = true;
            IAPManager.IsRunningTests = true;
            DataManager.EnableTestProviders(true);
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            DataManager.EnableTestProviders(false);
        }
        
    }
}
