using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class UnitOfSpeedSetting : MonoBehaviour
    {

        [SerializeField] private Image selectorIcon;
        [SerializeField] private float selectorIconOffset = 25;
        [SerializeField] private TextMeshProUGUI leftSideLabel;
        [SerializeField] private TextMeshProUGUI rightSideLabel;
        [SerializeField] private Color deselectedLabelColor = Color.white;
        [SerializeField] private Color selectedLabelColor = Color.white;

        public static bool UseMiles
        {
            get => DataManager.Settings.Get("UnitOfSpeedIsMiles", false);
            private set => DataManager.Settings.Set("UnitOfSpeedIsMiles", value);
        }

        private void OnEnable()
        {
            SetToggle(UseMiles);
        }

        public void OnClickToggle()
        {
            SetToggle(!UseMiles);
        }

        public void SetToggle(bool rightSideEnabled)
        {
            UseMiles = rightSideEnabled;
            
            selectorIcon.rectTransform.anchoredPosition = selectorIcon.rectTransform.anchoredPosition.SetX(rightSideEnabled ? selectorIconOffset : -selectorIconOffset);
            leftSideLabel.color = rightSideEnabled ? deselectedLabelColor : selectedLabelColor;
            rightSideLabel.color = rightSideEnabled ? selectedLabelColor : deselectedLabelColor;
        }
        
    }
}
