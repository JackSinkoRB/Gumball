using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class ResolutionScaleSetting : MonoBehaviour
    {

        public static float ResolutionScale
        {
            get => DataManager.Settings.Get("ResolutionScale", 0.9f);
            private set => DataManager.Settings.Set("ResolutionScale", value);
        }

        [SerializeField] private SliderWithPercent slider;

        private void OnEnable()
        {
            slider.UpdateSlider(ResolutionScale);
        }

        public void OnSliderValueChanged()
        {
            ResolutionScale = slider.Slider.value;
        }

    }
}
