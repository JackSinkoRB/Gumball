using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class DynamicQualitySetting : ToggleUI
    {

        public static bool UseDynamicQuality
        {
            get => DataManager.Settings.Get("UseDynamicQuality", true);
            private set => DataManager.Settings.Set("UseDynamicQuality", value);
        }

        protected override void Initialise()
        {
            SetToggle(UseDynamicQuality);
        }

        protected override void OnSelectLeftSide()
        {
            UseDynamicQuality = false;
        }

        protected override void OnSelectRightSide()
        {
            UseDynamicQuality = true;
        }
        
    }
}
