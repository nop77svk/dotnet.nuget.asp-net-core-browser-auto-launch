namespace NoP77svk.AspNetCore.BrowserAutoLaunch.Tests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
        builder.WebHost.UseUrls("http://127.0.0.1:3277");

        BrowserAutoLaunchService? service = null;
        builder.Services.AddSingleton(_ => service!);

        await using var app = builder.Build();
        service = new BrowserAutoLaunchService(app, NullLogger<BrowserAutoLaunchService>.Instance, new SystemBrowserLauncher())
        {
            ThrowOnBrowserOpenError = true,
            ThrowOnNoServerUriDetected = true
        };

        app.MapGet("/", () => "browser-smoke-ok");
        app.UseBrowserAutoLaunch();

        // Act
        await app.StartAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(app.Lifetime.ApplicationStarted.IsCancellationRequested);
    }
}
