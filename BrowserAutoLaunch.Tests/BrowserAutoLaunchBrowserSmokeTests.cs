namespace NoP77svk.AspNetCore.BrowserAutoLaunch.Tests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public sealed class BrowserAutoLaunchBrowserSmokeTests
{
    [Fact(Explicit = true)]
    [Trait("Category", "BrowserSmoke")]
    public async Task UseBrowserAutoLaunch_RequestsTheDefaultBrowser()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0"); // Use a random available port"

        await using var app = builder.Build();

        var service = new BrowserAutoLaunchService(app, NullLogger<BrowserAutoLaunchService>.Instance, new SystemBrowserLauncher())
        {
            ThrowOnBrowserOpenError = true,
            ThrowOnNoServerUriDetected = true
        };

        app.MapGet("/", () => "browser-smoke-ok");
        app.UseBrowserAutoLaunch(service);

        // Act
        using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await app.StartAsync(cts.Token);
        await Task.Delay(TimeSpan.FromSeconds(1), cts.Token);

        // Assert
        Assert.True(app.Lifetime.ApplicationStarted.IsCancellationRequested);

        await app.StopAsync(cts.Token);
    }
}
