#if UNITY_IOS
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

namespace Gumball.Editor
{
    public class IOSExemptFromEncryption : IPostprocessBuildWithReport
    {

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) //check if the build is for iOS 
                return;

            string plistPath = report.summary.outputPath + "/Info.plist";
            SetEncryptionToFalse(plistPath);
        }

        private void SetEncryptionToFalse(string plistPath)
        {
            PlistDocument plist = new PlistDocument(); //read Info.plist file into memory
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            File.WriteAllText(plistPath, plist.WriteToString()); //override Info.plist
        }

    }
}
#endif