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

        private const float targetFPS = 30;
        private const float timeBetweenChecks = 0.25f;
        private const int numberOfFrameTimingsToCollect = 1;
        private const float scaleStep = 0.05f; //how much to decrease/increase the resolution each check - eg. 0.05 is 5% each check
        
        public static float CurrentScale { get; private set; } = 1;
        
        private static readonly FrameTiming[] frameTimings = new FrameTiming[numberOfFrameTimingsToCollect];
        private static float timeSinceLastCheck;
        
        [RuntimeInitializeOnLoadMethod]
        private static void Initialise()
        {
            CoroutineHelper.onUnityUpdate -= Update;
            CoroutineHelper.onUnityUpdate += Update;

            //reset
            CurrentScale = 1;
            timeSinceLastCheck = 0;
        }
        
        private static void Update()
        {
            CalculateResolution();
        }

        public static float GetCurrentFPS()
        {
            //calculate FPS:
            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings(numberOfFrameTimingsToCollect, frameTimings);
            
            double gpuFrameTime = frameTimings[0].gpuFrameTime; //in ms
            double cpuFrameTime = frameTimings[0].cpuFrameTime; //in ms
            double frameTime = Mathf.Max((float)gpuFrameTime, (float)cpuFrameTime); //use the one that's taking longer
            float currentFPS = (float)(TimeUtils.MillisecondsInSecond / frameTime);

            return currentFPS;
        }

        private static void CalculateResolution()
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck < timeBetweenChecks)
                return;
            
            timeSinceLastCheck = 0;
            
            //calculate FPS:
            float currentFPS = GetCurrentFPS();

            CurrentScale = currentFPS < targetFPS ? Mathf.Max(minResolutionScale, CurrentScale - scaleStep) : Mathf.Min(maxResolutionScale, CurrentScale + scaleStep);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UniversalRenderPipelineAsset urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            urp.renderScale = CurrentScale;
#else
            ScalableBufferManager.ResizeBuffers(CurrentScale, CurrentScale);
#endif
        }
        
    }
}
