#if UNITY_IOS

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;

namespace Gumball.Editor
{
    public static class IOSFrameworkSetup
    {
        
        [PostProcessBuild(int.MaxValue)] //We want this code to run last!
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return; // Make sure its iOS build
            
            string projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject project = new PBXProject();
            project.ReadFromFile(projectPath);
            string mainTargetGuid = project.GetUnityMainTargetGuid();
            string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

            //set embedded content
            project.SetBuildProperty(mainTargetGuid, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "YES");
            project.SetBuildProperty(mainTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");

            project.SetBuildProperty(frameworkTargetGuid, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "NO");
            project.SetBuildProperty(frameworkTargetGuid, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            Debug.Log($"Set embedded swift library properties in {projectPath}.");

            //setup frameworks
            List<string> frameworks = new List<string>
            {
                "FBSDKLoginKit"
            };
            foreach (string framework in frameworks)
            {
                string frameworkName = framework + ".xcframework";
                string src = Path.Combine("Pods", framework, "XCFrameworks", frameworkName);
                
                if (project.ContainsFileByProjectPath(src))
                {
                    Debug.Log($"[IOS Framework Setup] {framework} already exists in project. Skipping.");
                    continue; // Skip already added frameworks
                }
                
                string frameworkPath = project.AddFile(src, src);
                project.AddFileToBuild(mainTargetGuid, frameworkPath);
                project.AddFileToEmbedFrameworks(mainTargetGuid, frameworkPath);
                
                Debug.Log($"[IOS Framework Setup] Added {framework} to project.");
            }

            project.WriteToFile(projectPath);
        }
        
    }
}
#endif