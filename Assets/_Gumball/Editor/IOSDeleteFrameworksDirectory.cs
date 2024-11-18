#if UNITY_IOS
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Gumball.Editor
{
    public class IOSDeleteFrameworksDirectory : MonoBehaviour
    {
        [PostProcessBuild(999)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget != BuildTarget.iOS) return;
    
            string unityFrameworkPath = Path.Combine(path, "Gumball.app/Frameworks/UnityFramework.framework/Frameworks");
    
            if (Directory.Exists(unityFrameworkPath))
            {
                Directory.Delete(unityFrameworkPath, true);
                Debug.Log("Removed disallowed 'Frameworks' directory from UnityFramework.framework");
            }
        }
    }
}
#endif