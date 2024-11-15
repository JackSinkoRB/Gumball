#if UNITY_IOS
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;
using System.IO;

namespace Gumball.Editor
{
    public class IOSCocoaPodsFile : IPreprocessBuildWithReport
    {

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform != BuildTarget.iOS) //check if the build is for iOS 
                return;

            string buildPath = report.summary.outputPath;
            string podfilePath = Path.Combine(buildPath, "iOS", "Podfile");

            // Check if the Podfile already exists, if not, create it
            if (!File.Exists(podfilePath))
                CreatePodfileFile(podfilePath);
            else
                Debug.Log("Podfile exists.");
        }

        // This method generates the Podfile with the necessary entries
        private void CreatePodfileFile(string podfilePath)
        {
            try
            {
                using StreamWriter writer = new StreamWriter(podfilePath);
                writer.WriteLine("platform :ios, '11.0'"); // Set minimum iOS version

                writer.WriteLine("target 'Unity-iPhone' do");

                // Add the required pods for Facebook SDK
                writer.WriteLine("pod \"FBSDKCoreKit\"");
                writer.WriteLine("pod \"FBSDKLoginKit\"");
                writer.WriteLine("pod \"FBSDKShareKit\"");
                writer.WriteLine("pod \"FBSDKMessengerShareKit\"");

                // Enable use_frameworks! to allow dynamic frameworks
                writer.WriteLine("use_frameworks!");

                writer.WriteLine("end");

                Debug.Log("Podfile created successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error creating Podfile: " + ex.Message);
            }
        }

    }
}
#endif