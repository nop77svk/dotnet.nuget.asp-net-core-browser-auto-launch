namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

internal sealed class MockOsPlatformResolver : IOsPlatformResolver
{
    public Func<bool> IsWindows { get; init; } = () => false;
    public Func<bool> IsLinux { get; init; } = () => false;
    public Func<bool> IsMacOS { get; init; } = () => false;

    public MockOsPlatformResolver()
    {
    }
}
