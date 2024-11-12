using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class QualityUtils
    {

        public static event Action onUpdateFPS;
        
        public const float TargetFPS = 25;

        private const int numberOfFramesToCollect = 10;
        private const int numberOfFrameTimingsToCollect = 1;
        
        private static float[] frames = new float[numberOfFramesToCollect];
        private static readonly FrameTiming[] frameTimings = new FrameTiming[numberOfFrameTimingsToCollect];
        private static int currentFrameIndex;
        private static bool filled;

        public static float CurrentFPS { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            onUpdateFPS = null;
            
            CoroutineHelper.onUnityUpdate -= Update;
            CoroutineHelper.onUnityUpdate += Update;

            //reset
            CurrentFPS = 0;
            currentFrameIndex = 0;
            frames = new float[numberOfFramesToCollect];
            filled = false;
        }

        private static void Update()
        {
            CalculateFPS();
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
            {
                onUpdateFPS?.Invoke();
                return; //is valid
            }

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
            onUpdateFPS?.Invoke();
        }
        
    }
}
