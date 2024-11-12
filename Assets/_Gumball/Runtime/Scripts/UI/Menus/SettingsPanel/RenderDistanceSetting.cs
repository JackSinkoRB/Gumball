using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class RenderDistanceSetting : SliderWithPercent
    {

        public static float RenderDistancePercent
        {
            get => DataManager.Settings.Get("RenderDistance", 1f);
            private set => DataManager.Settings.Set("RenderDistance", value);
        }

        public static float RenderDistance => DynamicChunkCullDistance.MinMaxDistance.Lerp(RenderDistancePercent);
        
        protected override void OnEnable()
        {
            UpdateSlider(RenderDistancePercent);

            base.OnEnable();
        }

        public override void UpdateSlider(float valueNormalized)
        {
            RenderDistancePercent = valueNormalized;

            base.UpdateSlider(valueNormalized);
        }

        protected override string GetLabelString()
        {
            float metres = RenderDistance;
            float feet = SpeedUtils.FromMetresToFeet(metres);

            string metresString = $"{Mathf.RoundToInt(metres)}m";
            string feetString = $"{Mathf.RoundToInt(feet)}ft";
            
            return UnitOfSpeedSetting.UseMiles ? feetString : metresString;
        }
        
    }
}
