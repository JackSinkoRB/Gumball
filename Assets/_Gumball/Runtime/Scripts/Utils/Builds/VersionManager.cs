using System;
using Gumball;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A tool for identifying builds
/// </summary>
[CreateAssetMenu(fileName = "Gumball/Singletons/Version Manager")]
public class VersionManager : SingletonScriptable<VersionManager>
{

    /// <summary>
    /// The full formatted build name.
    /// <remarks>These have full public access, as (for some odd reason) with private setters, the values are null in builds.</remarks>
    /// </summary>
    public string FullBuildName = "NOT INITIALISED";
    public string ShortBuildName = "NOT INITIALISED";
    public string ApplicationNameFormatted = "NOT INITIALISED";
    public string CommitHash = "NOT INITIALISED";
    public string CommitHashFormatted => CommitHash.Substring(0, 7);
    public string DateFormatted = "NOT INITIALISED";
    public string PlatformNameFormatted = "NOT INITIALISED";
    public string BuildTypeFormatted = "NOT INITIALISED";
    public string BranchName = "NOT INITIALISED";

#if UNITY_EDITOR
    public void UpdateVersion()
    {
        ApplicationNameFormatted = Application.productName.Replace(' ','_');
        BuildTypeFormatted = EditorUserBuildSettings.development ? "DEBUG" : "PRODUCTION";
        PlatformNameFormatted = GetFormattedPlatformName(EditorUserBuildSettings.activeBuildTarget.ToString());
        CommitHash = GitUtility.GetCommitHash();
        DateFormatted = GetDateFormatted();
        BranchName = GitUtility.GetBranchName();
        ShortBuildName = DateFormatted + "-" + CommitHashFormatted;
        FullBuildName = ApplicationNameFormatted + "_" + ShortBuildName + "_" +
                        PlatformNameFormatted + "_" + BuildTypeFormatted;
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
                               longDateTime.Substring(startOfYear+2, 2); //cut the year to 2 digits
        return shortDateTime;
    }
#endif
}
