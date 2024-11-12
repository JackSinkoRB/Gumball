using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SFXVolumeSetting : MonoBehaviour
    {

#region STATIC
        public static event Action<float> onVolumeSettingChange;

        public static float SFXVolumePercent
        {
            get => DataManager.Settings.Get("SFXVolumePercent", 1f);
            private set => DataManager.Settings.Set("SFXVolumePercent", value);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            onVolumeSettingChange = null;
        }
#endregion
        
        [SerializeField] private SliderWithPercent slider;

        private void OnEnable()
        {
            slider.UpdateSlider(SFXVolumePercent);
        }

        public void OnSliderValueChanged()
        {
            SFXVolumePercent = slider.Slider.value;
            onVolumeSettingChange?.Invoke(SFXVolumePercent);
        }

    }
}
