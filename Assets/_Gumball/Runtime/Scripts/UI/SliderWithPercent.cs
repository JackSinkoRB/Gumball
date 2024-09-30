using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SliderWithPercent : MonoBehaviour
    {

        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI percentLabel;

        public Slider Slider => slider;

        private void OnEnable()
        {
            UpdatePercentageLabel();
            
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void UpdateSlider(float valueNormalized)
        {
            slider.value = valueNormalized;
            UpdatePercentageLabel();
        }
        
        private void OnValueChanged(float value)
        {
            UpdatePercentageLabel();
        }

        private void UpdatePercentageLabel()
        {
            int percent = Mathf.RoundToInt(slider.value * 100f);
            percentLabel.text = $"{percent}";
            
            Debug.Log("Updated {name}");
        }

    }
}
