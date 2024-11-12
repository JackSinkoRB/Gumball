using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public static class DynamicChunkCullDistance
    {

        public static readonly MinMaxFloat MinMaxDistance = new(300,800);
        
        private const float timeBetweenChecks = 0.25f;
        private const float timeToGoFromMinToMax = 5;
        private const float timeToGoFromMaxToMin = 2;
        
        private static float timeSinceLastCheck;

        public static float CurrentDistance { get; private set; }

        [RuntimeInitializeOnLoadMethod]
        private static void RuntimeInitialise()
        {
            QualityUtils.onUpdateFPS -= Update;
            QualityUtils.onUpdateFPS += Update;
            
            //reset
            CurrentDistance = MinMaxDistance.Max;
            timeSinceLastCheck = 0;
        }
        
        private static void Update()
        {
            if (DynamicQualitySetting.UseDynamicQuality)
                CalculateDistance();
            else
                CurrentDistance = RenderDistanceSetting.RenderDistance; //manually set
        }

        private static void CalculateDistance()
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck < timeBetweenChecks)
                return;
            
            bool isDecreasing = QualityUtils.CurrentFPS < QualityUtils.TargetFPS;
            float timeToChange = isDecreasing ? timeToGoFromMaxToMin : timeToGoFromMinToMax;
            float frameTimePercent = Mathf.Clamp01(timeSinceLastCheck / timeToChange);
            float distanceDelta = frameTimePercent * MinMaxDistance.Difference;
            
            CurrentDistance = MinMaxDistance.Clamp(isDecreasing ? CurrentDistance - distanceDelta : CurrentDistance + distanceDelta);
            
            timeSinceLastCheck = 0;
        }

    }
}
