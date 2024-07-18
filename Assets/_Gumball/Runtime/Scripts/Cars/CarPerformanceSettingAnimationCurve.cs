using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettingAnimationCurve : CarPerformanceSetting
    {
        
        [Space]
        [SerializeField] private AnimationCurve min;
        [SerializeField] private AnimationCurve max;

        private AnimationCurve resultCached;
        private float lastKnownFinalWeight;
        
        public CarPerformanceSettingAnimationCurve(AnimationCurve min, AnimationCurve max)
        {
            this.min = min;
            this.max = max;
        }

        public AnimationCurve GetValue(CarPerformanceProfile profile)
        {
            resultCached ??= new AnimationCurve(min.keys);
            
            float finalWeight = GetFinalWeight(profile);
            if (lastKnownFinalWeight.Approximately(finalWeight))
                return resultCached;

            lastKnownFinalWeight = finalWeight;

            for (int index = 0; index < resultCached.length; index++)
            {
                Keyframe minKey = min.keys[index];
                Keyframe maxKey = max.keys[index];
                float desiredValue = Mathf.Lerp(minKey.value, maxKey.value, finalWeight);
                
                //move the key
                Keyframe newKeyframe = new Keyframe(minKey.time, desiredValue, 
                    (minKey.inTangent + maxKey.inTangent) / 2f,
                    (minKey.outTangent + maxKey.outTangent) / 2f,
                    (minKey.inWeight + maxKey.inWeight) / 2f,
                    (minKey.outWeight + maxKey.outWeight) / 2f);
                resultCached.MoveKey(index, newKeyframe);
            }
            
            return resultCached;
        }
        
    }
}
