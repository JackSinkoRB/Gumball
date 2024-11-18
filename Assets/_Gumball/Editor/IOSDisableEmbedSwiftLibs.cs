#if UNITY_IOS
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;

namespace Gumball.Editor
{
    // Automatically set the "Always Embed Swift Standard Libraries" option to "No" in UnityFramework Target in XCode
    public static class DisableEmbedSwiftLibs
    {
        [PostProcessBuild(int.MaxValue)] //We want this code to run last!
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS)
                return; // Make sure its iOS build
            
            string projPath = PBXProject.GetPBXProjectPath(path);
           
            var project = new PBXProject();
            project.ReadFromFile(projPath);

            string mainTargetGuid = project.GetUnityMainTargetGuid();
           
            foreach (var targetGuid in new[] { mainTargetGuid, project.GetUnityFrameworkTargetGuid() })
            {
                project.SetBuildProperty(targetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            }
           
            project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

            project.WriteToFile(projPath);
        }
    }
}
#endif