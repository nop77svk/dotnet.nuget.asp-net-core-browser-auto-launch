namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

using Microsoft.Extensions.Logging;

public static class BrowserAutoLaunchExtensions
{
    public static void UseBrowserAutoLaunch(this WebApplication app, BrowserAutoLaunchService autoLaunchService)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(autoLaunchService);

        autoLaunchService.RegisterOnApplicationStart();
    }

    public static void UseBrowserAutoLaunch(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        ILogger<BrowserAutoLaunchService> logger = app.Services.GetRequiredService<ILogger<BrowserAutoLaunchService>>();

        BrowserAutoLaunchService autoLaunchService = app.Services.GetService<BrowserAutoLaunchService>()
            ?? new BrowserAutoLaunchService(app, logger)
            {
                ThrowOnBrowserOpenError = false,
                ThrowOnNoServerUriDetected = false
            };

        app.UseBrowserAutoLaunch(autoLaunchService);
    }
}
