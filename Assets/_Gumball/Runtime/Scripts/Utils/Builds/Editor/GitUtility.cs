#if UNITY_EDITOR
using System.IO;
using System.Diagnostics;

public static class GitUtility
{
    public static string GetCommitHash() => RunGitCommand("rev-parse HEAD");
    public static string GetBranchName() => RunGitCommand("rev-parse --abbrev-ref HEAD");

    private static Process GetRepositoryProcess()
    {
        string gitPath = Directory.GetCurrentDirectory();
        var processInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
#if UNITY_EDITOR_WIN
            FileName = "git.exe",
#else //linux/headless mode git path:
            FileName = "/usr/bin/git",
#endif
            CreateNoWindow = true,
            WorkingDirectory = gitPath
        };

        Process process = new Process();
        process.StartInfo = processInfo;
        return process;
    }

    public static string RunGitCommand(string args)
    {
        Process _gitProcess = GetRepositoryProcess();
        _gitProcess.StartInfo.Arguments = args;
        _gitProcess.Start();
        string output = _gitProcess.StandardOutput.ReadToEnd().Trim();
        _gitProcess.WaitForExit();
        return output;
    }
}
#endif