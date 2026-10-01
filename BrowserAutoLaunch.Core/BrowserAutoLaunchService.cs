#pragma warning disable SA1313
namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

public sealed class BrowserAutoLaunchService
{
    private readonly WebApplication _application;
    private readonly ILogger<BrowserAutoLaunchService>? _logger;
    private readonly IBrowserLauncher _browserLauncher;

    public BrowserAutoLaunchService(WebApplication application, ILogger<BrowserAutoLaunchService>? logger = null)
        : this(application, logger, new SystemBrowserLauncher())
    {
    }

    internal BrowserAutoLaunchService(
        WebApplication application,
        ILogger<BrowserAutoLaunchService>? logger,
        IBrowserLauncher browserLauncher)
    {
        _application = application;
        _logger = logger ?? application.Services.GetRequiredService<ILogger<BrowserAutoLaunchService>>();
        _browserLauncher = browserLauncher;
    }

    public bool ThrowOnNoServerUriDetected { get; set; } = false;
    public bool ThrowOnBrowserOpenError { get; set; } = false;

    internal void RegisterTheBrowserAutoLaunchOnApplicationStart()
    {
        IHostApplicationLifetime lifetime = _application.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(LaunchTheBrowserFromApplication);
    }

    internal void LaunchTheBrowserFromApplication()
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
            _browserLauncher.Open(url);
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
