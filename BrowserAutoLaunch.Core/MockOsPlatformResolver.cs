namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

internal sealed class MockOsPlatformResolver : IOsPlatformResolver
{
    private readonly BrowserPlatform _browserPlatform;

    public MockOsPlatformResolver(BrowserPlatform browserPlatform = BrowserPlatform.Unsupported)
    {
        _browserPlatform = browserPlatform;
    }

    public BrowserPlatform GetCurrentPlatform()
    {
        return _browserPlatform;
    }
}
