using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class SettingsPanel : AnimatedPanel
    {
        
        [SerializeField] private StepOptionController gearboxOption;

        private void OnEnable()
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            gearboxOption.Select((int)GearboxSetting.Setting);
        }

    }
}
