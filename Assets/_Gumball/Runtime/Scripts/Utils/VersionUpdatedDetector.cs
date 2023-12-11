using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class VersionUpdatedDetector
    {

        public delegate void VersionUpdatedDelegate(string oldVersion, string newVersion);
        public static event VersionUpdatedDelegate onVersionUpdated;
        
        private static readonly JsonDataProvider versionHistory = new("VersionHistory");

        private const string versionHistoryKey = "LastKnownVersion";
        
        private static string currentVersion => VersionManager.Instance.ShortBuildName;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialise()
        {
            onVersionUpdated = null;
        }
        
        public static IEnumerator CheckIfNewVersionAsync()
        {
            versionHistory.LoadFromSourceAsync();
            yield return new WaitUntil(() => versionHistory.IsLoaded);
            
            if (!versionHistory.HasKey(versionHistoryKey))
            {
                OnVersionUpdated();
                yield break;
            }

            string savedVersion = versionHistory.Get<string>(versionHistoryKey);
            if (!savedVersion.Equals(currentVersion))
                OnVersionUpdated();
        }

        private static void OnVersionUpdated()
        {
            string savedVersion = versionHistory.HasKey(versionHistoryKey) ? versionHistory.Get<string>(versionHistoryKey) : "UNKNOWN_VERSION";
            
            versionHistory.Set(versionHistoryKey, currentVersion);
            
            onVersionUpdated?.Invoke(savedVersion, currentVersion);
            
            Debug.Log($"Version updated from {savedVersion} to {currentVersion}");
        }
        
    }
}
