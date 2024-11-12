using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public static class DynamicChunkCullDistance
    {

        private static readonly MinMaxFloat minMaxDistance = new(300,800);
        
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
            CurrentDistance = minMaxDistance.Max;
            timeSinceLastCheck = 0;
        }
        
        private static void Update()
        {
            CalculateDistance();
        }

        private static void CalculateDistance()
        {
            timeSinceLastCheck += Time.deltaTime;
            if (timeSinceLastCheck < timeBetweenChecks)
                return;
            
            bool isDecreasing = QualityUtils.CurrentFPS < QualityUtils.TargetFPS;
            float timeToChange = isDecreasing ? timeToGoFromMaxToMin : timeToGoFromMinToMax;
            float frameTimePercent = Mathf.Clamp01(timeSinceLastCheck / timeToChange);
            float distanceDelta = frameTimePercent * minMaxDistance.Difference;
            
            CurrentDistance = minMaxDistance.Clamp(isDecreasing ? CurrentDistance - distanceDelta : CurrentDistance + distanceDelta);
            
            timeSinceLastCheck = 0;
        }

    }
}
