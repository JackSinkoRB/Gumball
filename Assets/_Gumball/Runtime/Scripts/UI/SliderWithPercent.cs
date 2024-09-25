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

        private void OnEnable()
        {
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnValueChanged(float value)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            percentLabel.text = $"{percent}";
        }

    }
}
