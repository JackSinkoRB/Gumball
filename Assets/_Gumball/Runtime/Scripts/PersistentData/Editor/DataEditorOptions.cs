#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    public static class DataEditorOptions
    {

        private const string ToggleSavingOptionKey = "DataProvidersEnabled";
        private const string ToggleSavingMenuName = "Gumball/PersistentData/Saving Is Enabled";

        public static bool DataProvidersEnabled
        {
            get => EditorPrefs.GetBool(ToggleSavingOptionKey, true);
            set => EditorPrefs.SetBool(ToggleSavingOptionKey, value);
        }

        [MenuItem("Gumball/PersistentData/Reset Data")]
        private static void ResetData()
        {
            //TODO reset the game instead of just clearing the data if the application is playing
            DataManager.RemoveAllData();
            Debug.Log("All data was reset.");
        }

        [MenuItem(ToggleSavingMenuName)]
        private static void ToggleAutoloadOption()
        {
            DataProvidersEnabled = !DataProvidersEnabled;
        }

        [MenuItem(ToggleSavingMenuName, true)]
        private static bool ToggleAutoloadOptionValidation()
        {
            Menu.SetChecked(ToggleSavingMenuName, DataProvidersEnabled);
            return true;
        }

    }
}
#endif