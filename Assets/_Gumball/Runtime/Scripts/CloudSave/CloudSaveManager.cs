using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine;

namespace Gumball
{
    public static class CloudSaveManager
    {
    
        public enum SaveMethod
        {
            LOCAL,
            FACEBOOK
        }
        
        public static SaveMethod CurrentSaveMethod
        {
            get => DataManager.Settings.Get("CloudSave.CurrentMethod", SaveMethod.LOCAL);
            private set => DataManager.Settings.Set("CloudSave.CurrentMethod", value);
        }

        public static void SetCurrentSaveMethod(SaveMethod method)
        {
            CurrentSaveMethod = method;
            GlobalLoggers.PlayFabLogger.Log($"Set current cloud save method to {method}");
        }

    }
}
