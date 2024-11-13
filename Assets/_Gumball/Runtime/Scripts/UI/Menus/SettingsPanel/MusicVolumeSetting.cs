using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class MusicVolumeSetting : MonoBehaviour
    {

#region STATIC
        public static event Action<float> onVolumeSettingChange;
        
        public static float MusicVolumePercent
        {
            get => DataManager.Settings.Get("MusicVolumePercent", 1f);
            private set => DataManager.Settings.Set("MusicVolumePercent", value);
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
            slider.UpdateSlider(MusicVolumePercent);
        }

        public void OnSliderValueChanged()
        {
            MusicVolumePercent = slider.Slider.value;
            onVolumeSettingChange?.Invoke(MusicVolumePercent);
        }

    }
}
