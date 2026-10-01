namespace NoP77svk.AspNetCore.BrowserAutoLaunch;

using System.Diagnostics;

internal interface IBrowserLauncher
{
    void Open(string url);
}

internal sealed class SystemBrowserLauncher : IBrowserLauncher
{
    public void Open(string url)
    {
        ProcessStartInfo processStartInfo;

        if (OperatingSystem.IsWindows())
        {
            processStartInfo = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };
        }
        else if (OperatingSystem.IsLinux())
        {
            var xdgOpenPath = FindOnPath("xdg-open") ?? "/usr/bin/xdg-open";
            processStartInfo = new ProcessStartInfo
            {
                FileName = xdgOpenPath,
                Arguments = url,
                UseShellExecute = false
            };
        }
        else if (OperatingSystem.IsMacOS())
        {
            var openPath = FindOnPath("open") ?? "/usr/bin/open";
            processStartInfo = new ProcessStartInfo
            {
                FileName = openPath,
                Arguments = url,
                UseShellExecute = false
            };
        }
        else
        {
            throw new PlatformNotSupportedException($"Don't know how to open a web browser on {Environment.OSVersion}");
        }

        Process.Start(processStartInfo);
    }

    private static string? FindOnPath(string exeName)
    {
        var pathVar = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVar))
        {
            return null;
        }

        return pathVar.Split(Path.PathSeparator)
            .Select(dir => Path.Combine(dir, exeName))
            .FirstOrDefault(File.Exists);
    }
}
