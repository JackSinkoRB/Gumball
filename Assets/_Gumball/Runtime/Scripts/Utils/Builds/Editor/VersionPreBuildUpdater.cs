#if UNITY_EDITOR
using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Gumball
{
    public class VersionPreBuildUpdater : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            //delete any existing builds in the folder
            string buildDirectory = GetDesiredBuildDirectory(report.summary.outputPath);
            if (Directory.Exists(buildDirectory) && Directory.GetFiles(buildDirectory).Length > 0)
            {
                Directory.Delete(buildDirectory, true);
            }

            VersionManager.Instance.UpdateVersion();
        }

        /// <summary>
        /// Gets the desired directory to build to from the output path.
        /// </summary>
        private string GetDesiredBuildDirectory(string outputPath)
        {
            //start at the end of the outputpath, and loop back a character until path separater is found. Then return a substring to that index
            for (int count = outputPath.Length - 1; count >= 0; count--)
            {
                char charToCheck = char.Parse(outputPath.Substring(count, 1));
                if (charToCheck.Equals('/'))
                {
                    string directory = outputPath.Substring(0, count + 1);
                    return directory;
                }
            }

            return null; //no build directory ?
        }

        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (Application.isBatchMode)
            {
                // If batchMode is set, then we're running from command line (TeamCity builds) so we don't want to run  version updates or zipping.
                return;
            }

            CreateDetailsFile(pathToBuiltProject);
            ZipBuild(pathToBuiltProject);
        }

        /// <summary>
        /// A file generated to include any details about the build. Feel free to add anything needed
        /// </summary>
        private static void CreateDetailsFile(string pathToBuiltProject)
        {
            string directory = Path.GetDirectoryName(pathToBuiltProject);
            string timestampFile = Path.Combine(directory, "zBuildInfo.txt");

            using var streamWriter = File.CreateText(timestampFile);
            streamWriter.WriteLine($"Build version: {VersionManager.Instance.ShortBuildName}");
            streamWriter.WriteLine($"Git branch: {VersionManager.Instance.BranchName}");
            streamWriter.WriteLine($"Full commit hash: {VersionManager.Instance.CommitHash}");
            streamWriter.WriteLine($"Build type: {VersionManager.Instance.BuildTypeFormatted}");
            streamWriter.WriteLine($"Created at {DateTime.Now} ({TimeZoneInfo.Local.DisplayName})");
        }

        private static void ZipBuild(string pathToBuiltProject)
        {
            //delete zip first if exists
            string buildFolder = Path.GetDirectoryName(pathToBuiltProject);
            string desiredName = Directory.GetParent(buildFolder).FullName + Path.DirectorySeparatorChar +
                                 VersionManager.Instance.FullBuildName + ".zip";
            if (File.Exists(desiredName))
                File.Delete(desiredName);

            string[] filesOrDirContainingStringToIgnoreForZip = { "BackUpThisFolder_ButDontShipItWithYourGame", "BurstDebugInformation_DoNotShip" };
            ZipDirectoryExcludingIgnoredFiles(buildFolder, desiredName, filesOrDirContainingStringToIgnoreForZip);
        }

        private static void ZipDirectoryExcludingIgnoredFiles(string sourceDirectoryName, string destinationArchiveFileName, string[] fileNameStringsContainedToIgnore)
        {
            var filesToAdd = Directory.GetFiles(sourceDirectoryName, "*", SearchOption.AllDirectories);
            var entryNames = GetEntryNames(filesToAdd, sourceDirectoryName, false);

            using var zipFileStream = new FileStream(destinationArchiveFileName, FileMode.Create);
            using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);
            for (int i = 0; i < filesToAdd.Length; i++)
            {
                if (FileMustBeIgnored(filesToAdd[i], fileNameStringsContainedToIgnore))
                {
                    continue;
                }

                archive.CreateEntryFromFile(filesToAdd[i], entryNames[i], CompressionLevel.Fastest);
            }
        }

        private static string[] GetEntryNames(string[] names, string sourceFolder, bool includeBaseName)
        {
            if (names == null || names.Length == 0)
                return Array.Empty<string>();

            if (includeBaseName)
                sourceFolder = Path.GetDirectoryName(sourceFolder);

            int length = string.IsNullOrEmpty(sourceFolder) ? 0 : sourceFolder.Length;
            if (length > 0 && sourceFolder != null && sourceFolder[length - 1] != Path.DirectorySeparatorChar && sourceFolder[length - 1] != Path.AltDirectorySeparatorChar)
                length++;

            var result = new string[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                result[i] = names[i].Substring(length);
            }

            return result;
        }

        private static bool FileMustBeIgnored(string fileName, string[] fileNameStringsContainedToIgnore)
        {
            foreach (string stringToIgnore in fileNameStringsContainedToIgnore)
            {
                if (fileName.Contains(stringToIgnore))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
#endif