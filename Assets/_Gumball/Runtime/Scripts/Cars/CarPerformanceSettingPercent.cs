using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class CarPerformanceSettingPercent : CarPerformanceSetting
    {
        
        [Space]
        [SerializeField, Range(0,1)] private float maxPercent;

        public CarPerformanceSettingPercent(float maxPercent)
        {
            this.maxPercent = Mathf.Clamp01(maxPercent);
        }

        public float GetValue(CarPerformanceProfile profile)
        {
            float finalWeight = GetFinalWeight(profile);

            float finalValue = Mathf.Lerp(0, maxPercent, finalWeight);
            return finalValue;
        }
        
    }
}
