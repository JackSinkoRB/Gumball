using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class VisualSettingsSubMenu : SettingsSubMenu
    {

        [SerializeField] private DynamicQualitySetting dynamicQualitySetting;
        [SerializeField] private Transform resolutionScaleSetting;
        [SerializeField] private Transform renderDistanceSetting;

        protected override void OnShow()
        {
            base.OnShow();
            
            dynamicQualitySetting.onToggleChange += OnDynamicQualityToggleChange;
            
            Refresh();
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            dynamicQualitySetting.onToggleChange -= OnDynamicQualityToggleChange;
        }

        private void OnDynamicQualityToggleChange(bool isRightSideToggled)
        {
            Refresh();
        }

        private void Refresh()
        {
            resolutionScaleSetting.gameObject.SetActive(dynamicQualitySetting.RightSideEnabled);
            renderDistanceSetting.gameObject.SetActive(dynamicQualitySetting.RightSideEnabled);
        }
        
    }
}
