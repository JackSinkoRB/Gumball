#if UNITY_IOS
using System;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Gumball.Editor
{
    // Automatically set the "Always Embed Swift Standard Libraries" option to "No" in UnityFramework Target in XCode
    public static class DisableEmbedSwiftLibs
    {
        [PostProcessBuild(int.MaxValue)] //We want this code to run last!
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return; // Make sure its iOS build
            
            // Getting access to the xcode project file
            string projectPath = pathToBuildProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            PBXProject pbxProject = new PBXProject();
            pbxProject.ReadFromFile(projectPath);
            
            // Getting the UnityFramework Target and changing build settings
            string target = pbxProject.GetUnityFrameworkTargetGuid();
            pbxProject.SetBuildProperty(target, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");

            // After we're done editing the build settings we save it 
            pbxProject.WriteToFile(projectPath);
        }
    }
}
#endif