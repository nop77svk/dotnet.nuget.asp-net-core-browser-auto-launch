#pragma warning disable SA1313

namespace NoP77svk.AspNetBrowserAutoLaunch;

using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

public sealed class BrowserAutoLaunchService
{
    private readonly WebApplication _application;
    private readonly ILogger<BrowserAutoLaunchService>? _logger;

    public BrowserAutoLaunchService(WebApplication application, ILogger<BrowserAutoLaunchService>? logger = null)
    {
        _application = application;
        _logger = logger ?? application.Services.GetRequiredService<ILogger<BrowserAutoLaunchService>>();
    }

    public bool ThrowOnNoServerUriDetected { get; set; } = false;
    public bool ThrowOnBrowserOpenError { get; set; } = false;

    internal void RegisterTheBrowserAutoLaunchOnApplicationStart()
    {
        IHostApplicationLifetime lifetime = _application.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(LaunchTheBrowserFromApplication);
    }

    private static void OpenBrowser(string url)
    {
        ProcessStartInfo customProcessStartInfo;

        if (OperatingSystem.IsWindows())
        {
            customProcessStartInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            var xdgOpenPath = FindOnPath("xdg-open") ?? "/usr/bin/xdg-open";
            customProcessStartInfo = new ProcessStartInfo()
            {
                FileName = xdgOpenPath,
                Arguments = url,
                UseShellExecute = false
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            var openPath = FindOnPath("open") ?? "/usr/bin/open";
            customProcessStartInfo = new ProcessStartInfo()
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

        Process.Start(customProcessStartInfo);
    }

    private static string? FindOnPath(string exeName)
    {
        var pathVar = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVar))
        {
            return null;
        }

        string? result = pathVar.Split(Path.PathSeparator)
            .Select(dir => Path.Combine(dir, exeName))
            .FirstOrDefault(File.Exists);

        return result;
    }

    private void LaunchTheBrowserFromApplication()
    {
        ILogger logger = _logger ?? _application.Logger;

        var server = _application.Services.GetRequiredService<IServer>();
        var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();

        string? firstAppUrl = serverAddressesFeature?.Addresses?.FirstOrDefault();
        if (string.IsNullOrEmpty(firstAppUrl))
        {
            if (ThrowOnNoServerUriDetected)
            {
                throw new BrowserAutoLaunchException("Cannot determine server URL");
            }
            else
            {
                logger.LogWarning("Cannot determine server URL");
                return;
            }
        }

        logger.LogInformation("Spawning the web browser with URL {FirstAppUrl}", firstAppUrl);
        OpenBrowserAndHandleErrors(firstAppUrl);
    }

    private void OpenBrowserAndHandleErrors(string url)
    {
        ILogger logger = _logger ?? _application.Logger;

        try
        {
            OpenBrowser(url);
        }
        catch (Exception ex)
        {
            if (ThrowOnBrowserOpenError)
            {
                throw new BrowserAutoLaunchException($"Failed to spawn web browser on URL `{url}`", ex);
            }
            else
            {
                logger.LogError(ex, "Failed to spawn web browser on URL `{Url}`", url);
            }
        }
    }
}
