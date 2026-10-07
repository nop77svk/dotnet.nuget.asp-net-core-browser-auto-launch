namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

internal sealed class DotNetOsPlatformResolver : IOsPlatformResolver
{
    public Func<bool> IsWindows { get; init; } = OperatingSystem.IsWindows;
    public Func<bool> IsLinux { get; init; } = OperatingSystem.IsLinux;
    public Func<bool> IsMacOS { get; init; } = OperatingSystem.IsMacOS;

    public DotNetOsPlatformResolver()
    {
    }

    public BrowserPlatform GetCurrentPlatform()
    {
        if (IsWindows())
        {
            return BrowserPlatform.Windows;
        }

        if (IsLinux())
        {
            return BrowserPlatform.Linux;
        }

        if (IsMacOS())
        {
            return BrowserPlatform.MacOS;
        }

        return BrowserPlatform.Unsupported;
    }
}
