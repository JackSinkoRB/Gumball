#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Gumball.Editor
{
    public static class BuildShortcuts
    {
        [MenuItem("Gumball/Build/ALL")]
        public static void BuildAll()
        {
            CreateBuild(BuildTargetGroup.Android, BuildTarget.Android, false);
            CreateBuild(BuildTargetGroup.Android, BuildTarget.Android, true);
            CreateBuild(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, false);
            CreateBuild(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, true);
        }

        [MenuItem("Gumball/Build/Debug/ALL")]
        public static void BuildDebug()
        {
            CreateBuild(BuildTargetGroup.Android, BuildTarget.Android, true);
            CreateBuild(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, true);
        }

        [MenuItem("Gumball/Build/Production/ALL")]
        public static void BuildProduction()
        {
            CreateBuild(BuildTargetGroup.Android, BuildTarget.Android, false);
            CreateBuild(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, false);
        }

        [MenuItem("Gumball/Build/Production/Android")]
        public static void BuildAndroid() => CreateBuild(BuildTargetGroup.Android, BuildTarget.Android, false);

        [MenuItem("Gumball/Build/Debug/Android")]
        public static void BuildAndroidDebug() => CreateBuild(BuildTargetGroup.Android, BuildTarget.Android, true);

        [MenuItem("Gumball/Build/Production/Windows")]
        public static void BuildWindows() =>
            CreateBuild(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, false);

        [MenuItem("Gumball/Build/Debug/Windows")]
        public static void BuildWindowsDebug() =>
            CreateBuild(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64, true);
        
        private static void CreateBuild(BuildTargetGroup group, BuildTarget target, bool debug)
        {
            var options = BuildOptions.None;
            if (debug)
                options |= BuildOptions.Development;

            var dir = GetDirectoryForBuildOutput(target, debug);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            EditorUserBuildSettings.development = debug; //set before building
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);

            VersionManager.Instance.UpdateVersion(); //may have already updated, but double check here

            var outDir = GetDirectoryForBuildOutput(target, debug);
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            BuildPipeline.BuildPlayer(GetScenesForBuild(),
                Path.Combine(outDir, GetFileNameForTarget(target)),
                target, options);
        }

        private static string GetDirectoryForBuildOutput(BuildTarget target, bool debug)
        {
            return Path.Combine(Application.dataPath, "../Builds", debug ? "Debug" : "Production", target.ToString());
        }

        private static string GetFileNameForTarget(BuildTarget target)
        {
            string fileName = VersionManager.Instance.ApplicationNameFormatted + "_" +
                              VersionManager.Instance.ShortBuildName;
            switch (target)
            {
                case BuildTarget.Android:
                    return fileName + ".apk";

                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneWindows:
                    return fileName + ".exe";

                default:
                    return string.Empty;
            }
        }

        private static EditorBuildSettingsScene[] GetScenesForBuild()
        {
            return new[]
            {
                new EditorBuildSettingsScene(GetPathToMainScene(), true)
            };
        }

        private static string GetPathToMainScene()
        {
            string pathToMainScene = null;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path.Contains("BootScene"))
                {
                    pathToMainScene = scene.path;
                    break;
                }
            }

            if (pathToMainScene == null)
                throw new BuildFailedException(
                    "Could not find find the boot scene. A scene must contain 'BootScene' in the name.");

            return pathToMainScene;
        }
    }
}
#endif