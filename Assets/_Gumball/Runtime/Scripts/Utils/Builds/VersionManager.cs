using System;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gumball
{
    /// <summary>
    /// A tool for identifying builds
    /// </summary>
    public class VersionManager : ScriptableObject
    {
    
        private static VersionManager instance;
    
        public static VersionManager Instance
        {
            get
            {
                if (instance == null)
                    instance = Resources.Load<VersionManager>("Version Manager");
    
                if (instance == null)
                {
                    instance = CreateInstance<VersionManager>();
                    #if UNITY_EDITOR
                    if(!Directory.Exists("Assets/Resources/"))
                        Directory.CreateDirectory("Assets/Resources/");
                    AssetDatabase.CreateAsset(instance, "Assets/Resources/Version Manager.asset");
                    #endif
                }
    
                return instance;
            }
        }

        /// <summary>
        /// The full formatted build name.
        /// </summary>
        [SerializeField] public string FullBuildName = "NOT INITIALISED";
        
        [SerializeField] public string ShortBuildName = "NOT INITIALISED";
        [SerializeField] public string ApplicationNameFormatted = "NOT INITIALISED";
        [SerializeField] public string CommitHash = "NOT INITIALISED";
        public string CommitHashFormatted => CommitHash.Substring(0, 7);
        [SerializeField] public string DateFormatted = "NOT INITIALISED";
        [SerializeField] public string PlatformNameFormatted = "NOT INITIALISED";
        [SerializeField] public string BuildTypeFormatted = "NOT INITIALISED";
        [SerializeField] public string BranchName = "NOT INITIALISED";

#if UNITY_EDITOR
        public void UpdateVersion()
        {
            ApplicationNameFormatted = Application.productName.Replace(' ', '_');
            BuildTypeFormatted = EditorUserBuildSettings.development ? "DEBUG" : "PRODUCTION";
            PlatformNameFormatted = GetFormattedPlatformName(EditorUserBuildSettings.activeBuildTarget.ToString());
            CommitHash = GitUtility.GetCommitHash();
            DateFormatted = GetDateFormatted();
            BranchName = GitUtility.GetBranchName();
            ShortBuildName = DateFormatted + "-" + CommitHashFormatted;
            FullBuildName = ApplicationNameFormatted + "_" + ShortBuildName + "_" +
                            PlatformNameFormatted + "_" + BuildTypeFormatted;
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        private string GetFormattedPlatformName(string platformName)
        {
            string lower = platformName.ToLower();

            if (lower.Contains("windows")) return "Windows";
            if (lower.Contains("android")) return "Android";

            return platformName;
        }

        private string GetDateFormatted()
        {
            string longDateTime = DateTime.Now.ToShortDateString().Replace("/", "");
            int startOfYear = longDateTime.Length - 4;
            string shortDateTime = longDateTime.Substring(0, startOfYear) +
                                   longDateTime.Substring(startOfYear + 2, 2); //cut the year to 2 digits
            return shortDateTime;
        }
#endif
    }
}