using System.Diagnostics;

namespace Tesserae.Benchmark.Measure;

internal static class HarnessBuilder
{
    public static async Task BuildAsync(string harnessOutputDir)
    {
        var projectFile = ResolveProjectFile(harnessOutputDir);
        if (projectFile == null)
        {
            Console.WriteLine($"[build] Could not locate Tesserae.Benchmark.Tests.csproj near {harnessOutputDir}, skipping build.");
            return;
        }

        Console.WriteLine($"[build] dotnet build {projectFile}");

        var psi = new ProcessStartInfo("dotnet", $"build \"{projectFile}\" -nologo -v:minimal")
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false
        };

        // h5-compiler is a global tool installed under ~/.dotnet/tools but the
        // current shell may not have that on PATH (Claude Code remote envs).
        var toolsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools");
        if (Directory.Exists(toolsDir))
        {
            var sep  = OperatingSystem.IsWindows() ? ";" : ":";
            var path = psi.EnvironmentVariables["PATH"] ?? "";
            if (!path.Split(sep).Contains(toolsDir))
                psi.EnvironmentVariables["PATH"] = string.IsNullOrEmpty(path) ? toolsDir : $"{path}{sep}{toolsDir}";
        }

        using var p = Process.Start(psi)!;

        var stdoutTask = p.StandardOutput.ReadToEndAsync();
        var stderrTask = p.StandardError.ReadToEndAsync();

        await p.WaitForExitAsync();

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (p.ExitCode != 0)
        {
            Console.WriteLine(stdout);
            Console.Error.WriteLine(stderr);
            throw new InvalidOperationException($"Building harness failed with exit code {p.ExitCode}.");
        }

        Console.WriteLine("[build] succeeded.");
    }

    private static string? ResolveProjectFile(string harnessOutputDir)
    {
        // harnessOutputDir is typically .../Tesserae.Benchmark.Tests/bin/Debug/netstandard2.0/h5
        var dir = new DirectoryInfo(harnessOutputDir);
        for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "Tesserae.Benchmark.Tests.csproj");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }
}
