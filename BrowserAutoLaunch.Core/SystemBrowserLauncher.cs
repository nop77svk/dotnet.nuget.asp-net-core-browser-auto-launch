namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

using System.Diagnostics;

internal sealed class SystemBrowserLauncher : IBrowserLauncher
{
    private readonly Func<BrowserPlatform> _getPlatform;
    private readonly Func<string, string?> _findOnPath;
    private readonly Func<ProcessStartInfo, Process?> _startProcess;

    public SystemBrowserLauncher()
        : this(GetCurrentPlatform, FindOnPath, Process.Start)
    {
    }

    internal SystemBrowserLauncher(
        Func<BrowserPlatform> getPlatform,
        Func<string, string?> findOnPath,
        Func<ProcessStartInfo, Process?> startProcess)
    {
        _getPlatform = getPlatform;
        _findOnPath = findOnPath;
        _startProcess = startProcess;
    }

    public void Open(string url)
    {
        ProcessStartInfo processStartInfo;
        BrowserPlatform platform = _getPlatform();

        if (platform == BrowserPlatform.Windows)
        {
            processStartInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };
        }
        else if (platform == BrowserPlatform.Linux)
        {
            var xdgOpenPath = _findOnPath("xdg-open") ?? "/usr/bin/xdg-open";
            processStartInfo = new ProcessStartInfo
            {
                FileName = xdgOpenPath,
                Arguments = url,
                UseShellExecute = false
            };
        }
        else if (platform == BrowserPlatform.MacOS)
        {
            var openPath = _findOnPath("open") ?? "/usr/bin/open";
            processStartInfo = new ProcessStartInfo
            {
                FileName = openPath,
                Arguments = url,
                UseShellExecute = false
            };
        }
        else
        {
            throw new PlatformNotSupportedException($"Don't know how to open a web browser on {Environment.OSVersion}");
        }

        _ = _startProcess(processStartInfo);
    }

    internal static BrowserPlatform DeterminePlatform(bool isWindows, bool isLinux, bool isMacOS)
    {
        if (isWindows)
        {
            return BrowserPlatform.Windows;
        }

        if (isLinux)
        {
            return BrowserPlatform.Linux;
        }

        if (isMacOS)
        {
            return BrowserPlatform.MacOS;
        }

        return BrowserPlatform.Unsupported;
    }

    internal static string? FindOnPath(string exeName, string? pathVar, Func<string, bool> fileExists)
    {
        if (string.IsNullOrEmpty(pathVar))
        {
            return null;
        }

        return pathVar.Split(Path.PathSeparator)
            .Select(dir => Path.Combine(dir, exeName))
            .FirstOrDefault(fileExists);
    }

    internal static string? FindOnPath(string exeName)
        => FindOnPath(
            exeName: exeName,
            pathVar: Environment.GetEnvironmentVariable("PATH"),
            fileExists: File.Exists
        );

    internal static BrowserPlatform GetCurrentPlatform()
        => DeterminePlatform(
            isWindows: OperatingSystem.IsWindows(),
            isLinux: OperatingSystem.IsLinux(),
            isMacOS: OperatingSystem.IsMacOS()
        );
}
