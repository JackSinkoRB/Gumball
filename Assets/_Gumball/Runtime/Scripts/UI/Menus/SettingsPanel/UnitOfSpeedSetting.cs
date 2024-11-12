using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Gumball
{
    public class UnitOfSpeedSetting : ToggleUI
    {
        
        public static bool UseMiles
        {
            get => DataManager.Settings.Get("UnitOfSpeedIsMiles", false);
            private set => DataManager.Settings.Set("UnitOfSpeedIsMiles", value);
        }

        protected override void Initialise()
        {
            SetToggle(UseMiles);
        }

        protected override void OnSelectLeftSide()
        {
            UseMiles = false;
        }

        protected override void OnSelectRightSide()
        {
            UseMiles = true;
        }
        
    }
}
