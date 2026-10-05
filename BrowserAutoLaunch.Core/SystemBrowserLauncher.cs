namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

using System.Diagnostics;

internal sealed class SystemBrowserLauncher : IBrowserLauncher
{
    private readonly IOsPlatformResolver _osPlatformResolver;
    private readonly Func<string, string?> _findOnPath;
    private readonly Func<ProcessStartInfo, Process?> _startProcess;

    public SystemBrowserLauncher()
        : this(new DotNetOsPlatformResolver(), FindOnPath, Process.Start)
    {
    }

    internal SystemBrowserLauncher(
        IOsPlatformResolver osPlatformResolver,
        Func<string, string?> findOnPath,
        Func<ProcessStartInfo, Process?> startProcess)
    {
        _osPlatformResolver = osPlatformResolver;
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

        BrowserPlatform platform = _osPlatformResolver.GetCurrentPlatform();
        ProcessStartInfo processStartInfo = platform switch
        {
            BrowserPlatform.Windows => new ProcessStartInfo(parsedUri.AbsoluteUri)
            {
                UseShellExecute = true
            },
            BrowserPlatform.Linux => new ProcessStartInfo
            {
                FileName = _findOnPath("xdg-open") ?? "/usr/bin/xdg-open",
                Arguments = parsedUri.AbsoluteUri,
                UseShellExecute = false
            },
            BrowserPlatform.MacOS => new ProcessStartInfo
            {
                FileName = _findOnPath("open") ?? "/usr/bin/open",
                Arguments = parsedUri.AbsoluteUri,
                UseShellExecute = false
            },
            _ => throw new PlatformNotSupportedException($"Don't know how to open a web browser on {Environment.OSVersion}")
        };

        using var process = _startProcess(processStartInfo);
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
