namespace NoP77svk.AspNetCore.BrowserAutoLaunch.Tests;

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public sealed class BrowserAutoLaunchUnitTests
{
    [Fact]
    public void UseBrowserAutoLaunch_ThrowsWhenApplicationIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => BrowserAutoLaunchExtensions.UseBrowserAutoLaunch(null!));
    }

    [Fact]
    public async Task UseBrowserAutoLaunch_RegistersStartupCallbackForRegisteredService()
    {
        using var server = new FakeServer("http://localhost:5001", "http://localhost:5002");
        var launcher = new FakeBrowserLauncher();
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSingleton<IServer>(server);

        BrowserAutoLaunchService? service = null;
        builder.Services.AddSingleton(_ => service!);

        await using var app = builder.Build();
        service = new BrowserAutoLaunchService(app, NullLogger<BrowserAutoLaunchService>.Instance, launcher);

        app.UseBrowserAutoLaunch();
        await app.StartAsync();

        Assert.Equal(new[] { "http://localhost:5001" }, launcher.OpenedUrls, StringComparer.Ordinal);
    }

    [Fact]
    public async Task RegisterTheBrowserAutoLaunchOnApplicationStart_InvokesLauncherWhenStarted()
    {
        using var server = new FakeServer("http://localhost:5000");
        var launcher = new FakeBrowserLauncher();

        await using var app = CreateApplication(server);
        var service = CreateService(app, launcher);

        service.RegisterTheBrowserAutoLaunchOnApplicationStart();
        Assert.Empty(launcher.OpenedUrls);

        await app.StartAsync();

        Assert.Equal(new[] { "http://localhost:5000" }, launcher.OpenedUrls, StringComparer.Ordinal);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LaunchTheBrowserFromApplication_HandlesMissingAddressAccordingToSetting(bool shouldThrow)
    {
        using var server = new FakeServer();
        var launcher = new FakeBrowserLauncher();

        using var app = CreateApplication(server);
        var service = CreateService(app, launcher);
        service.ThrowOnNoServerUriDetected = shouldThrow;

        if (shouldThrow)
        {
            var exception = Assert.Throws<BrowserAutoLaunchException>(service.LaunchTheBrowserFromApplication);
            Assert.Equal("Cannot determine server URL", exception.Message);
        }
        else
        {
            service.LaunchTheBrowserFromApplication();
        }

        Assert.Empty(launcher.OpenedUrls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LaunchTheBrowserFromApplication_HandlesLauncherFailureAccordingToSetting(bool shouldThrow)
    {
        using var server = new FakeServer("http://localhost:5000");
        var launcherError = new InvalidOperationException("launcher failed");
        var launcher = new FakeBrowserLauncher { Error = launcherError };

        using var app = CreateApplication(server);
        var service = CreateService(app, launcher);
        service.ThrowOnBrowserOpenError = shouldThrow;

        if (shouldThrow)
        {
            var exception = Assert.Throws<BrowserAutoLaunchException>(service.LaunchTheBrowserFromApplication);
            Assert.Same(launcherError, exception.InnerException);
            Assert.Contains("http://localhost:5000", exception.Message);
        }
        else
        {
            service.LaunchTheBrowserFromApplication();
        }
    }

    private static WebApplication CreateApplication(FakeServer server)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddSingleton<IServer>(server);
        return builder.Build();
    }

    private static BrowserAutoLaunchService CreateService(WebApplication app, FakeBrowserLauncher launcher)
    {
        return new BrowserAutoLaunchService(app, NullLogger<BrowserAutoLaunchService>.Instance, launcher);
    }

    private sealed class FakeBrowserLauncher : IBrowserLauncher
    {
        public List<string> OpenedUrls { get; } = [];
        public Exception? Error { get; init; }

        public void Open(string url)
        {
            if (Error is not null)
            {
                throw Error;
            }

            OpenedUrls.Add(url);
        }
    }

    private sealed class FakeServer : IServer
    {
        private readonly FeatureCollection _features = new();

        public FakeServer(params string[]? addresses)
        {
            if (addresses is not null)
            {
                _features[typeof(IServerAddressesFeature)] = new FakeServerAddressesFeature(addresses);
            }
        }

        public IFeatureCollection Features => _features;

        public void Dispose()
        {
        }

        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
            where TContext : notnull
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeServerAddressesFeature : IServerAddressesFeature
    {
        public FakeServerAddressesFeature(IEnumerable<string> addresses)
        {
            Addresses = addresses.ToList();
        }

        public ICollection<string> Addresses { get; }
        public bool PreferHostingUrls { get; set; }
    }
}
