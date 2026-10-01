namespace NoP77svk.AspNetCore.BrowserAutoLaunch.Tests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public sealed class BrowserAutoLaunchEndToEndTests
{
    [Fact]
    public async Task UseBrowserAutoLaunch_DoesNotThrowWhenStartupHasNoServerAddress()
    {
        using var server = new AddresslessServer();
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IServer>(server);

        await using var app = builder.Build();
        app.UseBrowserAutoLaunch();

        await app.StartAsync();

        Assert.True(app.Lifetime.ApplicationStarted.IsCancellationRequested);
    }

    [Fact]
    public async Task UseBrowserAutoLaunch_UsesBoundKestrelAddressAfterStartup()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var launcher = new RecordingBrowserLauncher();

        BrowserAutoLaunchService? service = null;
        builder.Services.AddSingleton(_ => service!);

        await using var app = builder.Build();
        service = new BrowserAutoLaunchService(app, NullLogger<BrowserAutoLaunchService>.Instance, launcher);
        app.MapGet("/", () => "e2e-ok");
        app.UseBrowserAutoLaunch();

        Assert.Empty(launcher.OpenedUrls);

        await app.StartAsync();

        var server = app.Services.GetRequiredService<IServer>();
        var address = Assert.Single(server.Features.Get<IServerAddressesFeature>()!.Addresses);
        Assert.Equal(new[] { address }, launcher.OpenedUrls, StringComparer.Ordinal);

        using var client = new HttpClient { BaseAddress = new Uri(address) };
        Assert.Equal("e2e-ok", await client.GetStringAsync("/"));
    }

    private sealed class RecordingBrowserLauncher : IBrowserLauncher
    {
        public List<string> OpenedUrls { get; } = [];

        public void Open(string url) => OpenedUrls.Add(url);
    }

    private sealed class AddresslessServer : IServer
    {
        public IFeatureCollection Features { get; } = new FeatureCollection();

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
            where TContext : notnull => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public void Dispose()
        {
        }
    }
}
