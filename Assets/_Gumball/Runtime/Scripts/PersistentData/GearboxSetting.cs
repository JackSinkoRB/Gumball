using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class GearboxSetting : MonoBehaviour
    {

        public enum GearboxOption
        {
            AUTOMATIC,
            MANUAL
        }
        
        public static event Action<GearboxOption> onSettingChanged;
        
        private const string gearboxSaveKey = "Driving.Gearbox";

        public static GearboxOption Setting => DataManager.Settings.Get<GearboxOption>(gearboxSaveKey);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialise()
        {
            onSettingChanged = null;
        }
        
        public static void Set(GearboxOption option)
        {
            DataManager.Settings.Set(gearboxSaveKey, option);
            onSettingChanged?.Invoke(option);
        }

        /// <summary>
        /// Workaround for unity events.
        /// </summary>
        public void SetForUnityEvent(int option) => Set((GearboxOption)option);

    }
}
