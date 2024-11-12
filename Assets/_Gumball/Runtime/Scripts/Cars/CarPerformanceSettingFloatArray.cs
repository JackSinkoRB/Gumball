using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettingFloatArray : CarPerformanceSetting
    {
        
        [Space]
        [SerializeField] private float[] min;
        [SerializeField] private float[] max;

        private float[] resultsCached;

        public CarPerformanceSettingFloatArray(float[] min, float[] max)
        {
            this.min = min;
            this.max = max;

            resultsCached = new float[min.Length];
        }

        public float[] GetValue(CarPerformanceProfile profile)
        {
            if (AreMinAndMaxEqual())
                return min; //values can't be upgraded, no need to calculate

            float finalWeight = GetFinalWeight(profile);

            for (int index = 0; index < min.Length; index++)
            {
                float minValue = min[index];
                float maxValue = index < max.Length ? max[index] : minValue; //if array lengths don't match, just use the min value to prevent errors
                
                float finalValue = Mathf.Lerp(minValue, maxValue, finalWeight);
                resultsCached[index] = finalValue;
            }
            
            return resultsCached;
        }

        private bool AreMinAndMaxEqual()
        {
            for (var index = 0; index < min.Length; index++)
            {
                if (!min[index].Approximately(max[index]))
                    return false;
            }

            return true;
        }
        
    }
}
