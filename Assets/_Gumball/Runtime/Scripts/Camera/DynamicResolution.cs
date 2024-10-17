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
        private const int numberOfFramesToCollect = 10;
        private const int numberOfFrameTimingsToCollect = 1;
        private const float scaleStep = 0.05f; //how much to decrease/increase the resolution each check - eg. 0.05 is 5% each check
        
        public static float CurrentScale { get; private set; } = 1;
        public static float CurrentFPS { get; private set; }
        
        private static float[] frames = new float[numberOfFramesToCollect];
        private static readonly FrameTiming[] frameTimings = new FrameTiming[numberOfFrameTimingsToCollect];
        private static int currentFrameIndex = 0;
        private static float timeSinceLastCheck;
        private static bool filled;
        
        [RuntimeInitializeOnLoadMethod]
        private static void Initialise()
        {
            CoroutineHelper.onUnityUpdate -= Update;
            CoroutineHelper.onUnityUpdate += Update;

            //reset
            CurrentScale = 1;
            CurrentFPS = 0;
            timeSinceLastCheck = 0;
            currentFrameIndex = 0;
            frames = new float[numberOfFramesToCollect];
            filled = false;
        }
        
        private static void Update()
        {
            CalculateFPS();
            CalculateResolution();
        }
        
        private static void CalculateFPS()
        {
            //try with FrameTimingManager first as it's more accurate
            FrameTimingManager.CaptureFrameTimings();
            FrameTimingManager.GetLatestTimings(numberOfFrameTimingsToCollect, frameTimings);
            
            double gpuFrameTime = frameTimings[0].gpuFrameTime; //in ms
            double cpuFrameTime = frameTimings[0].cpuFrameTime; //in ms
            double frameTime = Mathf.Max((float)gpuFrameTime, (float)cpuFrameTime); //use the one that's taking longer
            CurrentFPS = (float)(TimeUtils.MillisecondsInSecond / frameTime);

            if (CurrentFPS is > 0 and < 10000)
                return; //is valid
            
            //calculate using average
            frames[currentFrameIndex] = Time.deltaTime;
            currentFrameIndex = (currentFrameIndex + 1) % numberOfFramesToCollect;
            if (currentFrameIndex == 0)
                filled = true;

            int numberOfFramesToUse = filled ? numberOfFramesToCollect : currentFrameIndex;
            float totalFrameTime = 0f;
            for (int index = 0; index < numberOfFramesToUse; index++)
                totalFrameTime += frames[index];

            if (totalFrameTime == 0) //no division by 0
                return;

            CurrentFPS = numberOfFramesToUse / totalFrameTime;
        }

        private static void CalculateResolution()
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck < timeBetweenChecks)
                return;
            
            timeSinceLastCheck = 0;
            
            CurrentScale = CurrentFPS < targetFPS ? Mathf.Max(minResolutionScale, CurrentScale - scaleStep) : Mathf.Min(maxResolutionScale, CurrentScale + scaleStep);

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UniversalRenderPipelineAsset urp = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
            urp.renderScale = CurrentScale;
#else
            ScalableBufferManager.ResizeBuffers(CurrentScale, CurrentScale);
#endif
        }
        
    }
}
