#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Gumball.Editor
{
    public static class IOSDeleteFrameworksDirectory
    {
        [PostProcessBuild(int.MaxValue)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuiltProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;
    
            string unityFrameworkPath = Path.Combine(pathToBuiltProject, "Gumball.app/Frameworks/UnityFramework.framework/Frameworks");
    
            Debug.Log($"unityFrameworkPath: {unityFrameworkPath}");
            if (Directory.Exists(unityFrameworkPath))
            {
                Directory.Delete(unityFrameworkPath, true);
                Debug.Log("Removed disallowed 'Frameworks' directory from UnityFramework.framework");
            }
        }
    }
}
#endif