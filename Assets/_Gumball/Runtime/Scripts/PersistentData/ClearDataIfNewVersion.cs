#if !UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class ClearDataIfNewVersion : MonoBehaviour
    {

        [RuntimeInitializeOnLoadMethod]
        private static void Initialise()
        {
            VersionUpdatedDetector.onVersionUpdated += OnVersionUpdated;
        }

        private static void OnVersionUpdated(string oldVersion, string newVersion)
        {
            DataManager.RemoveAllData();
        }
        
    }
}
#endif