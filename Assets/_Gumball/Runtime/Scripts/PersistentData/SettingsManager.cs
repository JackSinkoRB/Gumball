using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    [CreateAssetMenu(menuName = "Gumball/Singletons/Settings Manager")]
    public class SettingsManager : SingletonScriptable<SettingsManager>
    {

        public event Action<int> onGearboxSettingChanged;
        
        private const string gearboxSaveKey = "Driving.Gearbox";

        public static int GearboxSetting => DataManager.Settings.Get<int>(gearboxSaveKey);

        public void SetGearboxSetting(int newValue)
        {
            DataManager.Settings.Set(gearboxSaveKey, newValue);
            onGearboxSettingChanged?.Invoke(newValue);
        }

    }
}
