#pragma warning disable SA1313
namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;

public sealed class BrowserAutoLaunchService
{
    private readonly WebApplication _application;
    private readonly ILogger<BrowserAutoLaunchService> _logger;
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

    internal void RegisterOnApplicationStart(WebApplication application)
    {
        IHostApplicationLifetime lifetime = application.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(LaunchTheBrowserFromApplication);
    }

    internal void RegisterOnApplicationStart() => RegisterOnApplicationStart(_application);

    internal void LaunchTheBrowserFromApplication()
    {
        var server = _application.Services.GetRequiredService<IServer>();
        var serverAddressesFeature = server.Features.Get<IServerAddressesFeature>();

        string? firstAppUrl = serverAddressesFeature?.Addresses.FirstOrDefault();
        if (string.IsNullOrEmpty(firstAppUrl))
        {
            if (ThrowOnNoServerUriDetected)
            {
                throw new BrowserAutoLaunchException("Cannot determine server URL");
            }
            else
            {
                _logger.LogWarning("Cannot determine server URL");
                return;
            }
        }

        _logger.LogInformation("Spawning the web browser with URL {FirstAppUrl}", RedactUrl(firstAppUrl));
        OpenBrowserAndHandleErrors(firstAppUrl);
    }

    private static string RedactUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            ? uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped)
            : "<invalid-url>";
    }

    private void OpenBrowserAndHandleErrors(string url)
    {
        try
        {
            _browserLauncher.Open(url);
        }
        catch (Exception ex)
        {
            string redactedUrl = RedactUrl(url);
            if (ThrowOnBrowserOpenError)
            {
                throw new BrowserAutoLaunchException($"Failed to spawn web browser on URL `{redactedUrl}`", ex);
            }
            else
            {
                _logger.LogError(ex, "Failed to spawn web browser on URL `{Url}`", redactedUrl);
            }
        }
    }
}
