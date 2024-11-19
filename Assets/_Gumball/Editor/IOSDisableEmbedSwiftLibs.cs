// #if UNITY_IOS
// using UnityEditor;
// using UnityEditor.iOS.Xcode;
// using UnityEditor.Callbacks;
//
// namespace Gumball.Editor
// {
//     // Automatically set the "Always Embed Swift Standard Libraries" option to "No" in UnityFramework Target in XCode
//     public static class DisableEmbedSwiftLibs
//     {
//         [PostProcessBuild(int.MaxValue)] //We want this code to run last!
//         public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
//         {
//             if (buildTarget != BuildTarget.iOS)
//                 return; // Make sure its iOS build
//             
//             string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
//             PBXProject proj = new PBXProject();
//             proj.ReadFromFile(projPath);
//             string main = proj.GetUnityMainTargetGuid();
//             string framework = proj.GetUnityFrameworkTargetGuid();
//
//             proj.SetBuildProperty(main, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "YES");
//             proj.SetBuildProperty(main, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
//
//             proj.SetBuildProperty(framework, "EMBEDDED_CONTENT_CONTAINS_SWIFT", "NO");
//             proj.SetBuildProperty(framework, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
//                 
//             proj.WriteToFile(projPath);
//         }
//     }
// }
// #endif