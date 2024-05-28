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

        private bool usingMiles
        {
            get => DataManager.Settings.Get("UnitOfSpeedIsMiles", false);
            set => DataManager.Settings.Set("UnitOfSpeedIsMiles", value);
        }

        private void OnEnable()
        {
            SetToggle(usingMiles);
        }

        public void OnClickToggle()
        {
            SetToggle(!usingMiles);
        }

        public void SetToggle(bool rightSideEnabled)
        {
            usingMiles = rightSideEnabled;
            
            selectorIcon.rectTransform.anchoredPosition = selectorIcon.rectTransform.anchoredPosition.SetX(rightSideEnabled ? selectorIconOffset : -selectorIconOffset);
            leftSideLabel.color = rightSideEnabled ? deselectedLabelColor : selectedLabelColor;
            rightSideLabel.color = rightSideEnabled ? selectedLabelColor : deselectedLabelColor;
        }
        
    }
}
