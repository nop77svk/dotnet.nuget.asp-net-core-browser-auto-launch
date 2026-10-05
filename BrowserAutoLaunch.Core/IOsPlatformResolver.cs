namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

internal interface IOsPlatformResolver
{
    Func<bool> IsWindows { get; }
    Func<bool> IsLinux { get; }
    Func<bool> IsMacOS { get; }

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
