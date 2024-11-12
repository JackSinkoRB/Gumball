using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gumball
{
    public static class DynamicResolution
    {
        
        private const float maxResolutionScale = 0.9f;
        private const float minResolutionScale = 0.5f;

        private const float timeBetweenChecks = 0.25f;
        private const float scaleStep = 0.025f; //how much to decrease/increase the resolution each check - eg. 0.05 is 5% each check
        
        private static float timeSinceLastCheck;
        
        public static float CurrentScale { get; private set; } = 1;
        
        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            QualityUtils.onUpdateFPS -= Update;
            QualityUtils.onUpdateFPS += Update;

            //reset
            CurrentScale = 1;
            timeSinceLastCheck = 0;
        }
        
        private static void Update()
        {
            if (DynamicQualitySetting.UseDynamicQuality)
                CalculateResolution();
            else
            {
                //TODO: set the resolution based on the setting
            }
        }

        private static void CalculateResolution()
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck < timeBetweenChecks)
                return;
            
            timeSinceLastCheck = 0;
            
            CurrentScale = QualityUtils.CurrentFPS < QualityUtils.TargetFPS ? Mathf.Max(minResolutionScale, CurrentScale - scaleStep) : Mathf.Min(maxResolutionScale, CurrentScale + scaleStep);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UniversalRenderPipelineAsset urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            urp.renderScale = CurrentScale;
#else
            ScalableBufferManager.ResizeBuffers(CurrentScale, CurrentScale);
#endif
        }
        
    }
}
