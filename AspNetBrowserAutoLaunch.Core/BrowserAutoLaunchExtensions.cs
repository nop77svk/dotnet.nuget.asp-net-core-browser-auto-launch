namespace NoP77svk.AspNetBrowserAutoLaunch;

using System;
using System.Diagnostics;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public static class BrowserAutoLaunchExtensions
{
    public static void UseBrowserAutoLaunch(this WebApplication app)
    {
        IHostApplicationLifetime lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() =>
        {
            ILogger logger = app.Logger;

            var server = app.Services.GetRequiredService<IServer>();
            var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();

            string? firstAppUrl = serverAddressesFeature?.Addresses?.FirstOrDefault();
            if (string.IsNullOrEmpty(firstAppUrl))
            {
                logger.LogWarning("Cannot determine server URL");
                return;
            }

            logger.LogInformation("Spawning the web browser to URL {FirstAppUrl}", firstAppUrl);
            OpenBrowser(firstAppUrl);
        });
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
}
