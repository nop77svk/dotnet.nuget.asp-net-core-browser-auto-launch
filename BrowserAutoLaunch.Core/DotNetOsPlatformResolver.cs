namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

internal sealed class DotNetOsPlatformResolver : IOsPlatformResolver
{
    public Func<bool> IsWindows { get; } = OperatingSystem.IsWindows;
    public Func<bool> IsLinux { get; } = OperatingSystem.IsLinux;
    public Func<bool> IsMacOS { get; } = OperatingSystem.IsMacOS;

    public DotNetOsPlatformResolver()
    {
    }
}
