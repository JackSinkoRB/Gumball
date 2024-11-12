using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class SliderWithPercent : MonoBehaviour
    {

        [SerializeField] private Slider slider;
        [SerializeField] private TextMeshProUGUI percentLabel;
        [SerializeField, Range(0, 1)] private float minValue = 0;
        [SerializeField, Range(0, 1)] private float maxValue = 1;
        
        public Slider Slider => slider;

        protected virtual void OnEnable()
        {
            UpdatePercentageLabel();
            
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        public virtual void UpdateSlider(float valueNormalized)
        {
            slider.value = Mathf.Clamp(valueNormalized, minValue, maxValue);
            UpdatePercentageLabel();
        }
        
        private void OnValueChanged(float value)
        {
            UpdateSlider(value);
        }

        private void UpdatePercentageLabel()
        {
            percentLabel.text = GetLabelString();
        }

        //override to use different value other than percent
        protected virtual string GetLabelString()
        {
            int percent = Mathf.RoundToInt(slider.value * 100f);
            return percent.ToString();
        }

    }
}
