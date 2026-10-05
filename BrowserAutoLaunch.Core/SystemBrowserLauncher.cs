namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

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
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUri))
        {
            throw new ArgumentException($"Invalid URL: {url}", nameof(url));
        }

        if (parsedUri.Scheme is not "http" and not "https")
        {
            throw new ArgumentException($"Invalid URL scheme: {parsedUri.Scheme}. Only 'http' and 'https' are supported.", nameof(url));
        }

        ProcessStartInfo processStartInfo;
        BrowserPlatform platform = _getPlatform();

        if (platform == BrowserPlatform.Windows)
        {
            processStartInfo = new ProcessStartInfo(parsedUri.AbsoluteUri)
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
                Arguments = parsedUri.AbsoluteUri,
                UseShellExecute = false
            };
        }
        else if (platform == BrowserPlatform.MacOS)
        {
            var openPath = _findOnPath("open") ?? "/usr/bin/open";
            processStartInfo = new ProcessStartInfo
            {
                FileName = openPath,
                Arguments = parsedUri.AbsoluteUri,
                UseShellExecute = false
            };
        }
        else
        {
            throw new PlatformNotSupportedException($"Don't know how to open a web browser on {Environment.OSVersion}");
        }

        using var process = _startProcess(processStartInfo);
    }

    [ExcludeFromCodeCoverage]
    internal static BrowserPlatform GetCurrentPlatform()
    {
        if (OperatingSystem.IsWindows())
        {
            return BrowserPlatform.Windows;
        }

        if (OperatingSystem.IsLinux())
        {
            return BrowserPlatform.Linux;
        }

        if (OperatingSystem.IsMacOS())
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

        return pathVar.Split(Path.PathSeparator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(dir => Path.IsPathRooted(dir))
            .Select(dir => Path.Combine(dir, exeName))
            .FirstOrDefault(fileExists);
    }

    internal static string? FindOnPath(string exeName)
        => FindOnPath(
            exeName: exeName,
            pathVar: Environment.GetEnvironmentVariable("PATH"),
            fileExists: File.Exists
        );
}
