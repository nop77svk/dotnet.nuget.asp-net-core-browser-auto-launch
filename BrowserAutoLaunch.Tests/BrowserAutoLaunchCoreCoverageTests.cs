namespace NoP77svk.AspNetCore.BrowserAutoLaunch.Tests;

using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Builder;
using Xunit;

public sealed class BrowserAutoLaunchCoreCoverageTests
{
    [Fact]
    public void BrowserAutoLaunchException_ParameterlessConstructorCreatesException()
    {
        // Arrange

        // Act
        var exception = new BrowserAutoLaunchException();

        // Assert
        Assert.IsType<BrowserAutoLaunchException>(exception);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void BrowserAutoLaunchService_ResolvesLoggerWhenConstructorLoggerIsNull()
    {
        // Arrange
        using var app = WebApplication.CreateBuilder().Build();
        var launcher = new RecordingBrowserLauncher();
        var service = new BrowserAutoLaunchService(app, null, launcher);

        // Act
        service.LaunchTheBrowserFromApplication();

        // Assert
        Assert.Empty(launcher.StartInfo);
    }

    [Fact]
    public void GetCurrentPlatform_MatchesOperatingSystemDetection()
    {
        // Arrange
        BrowserPlatform expected = OperatingSystem.IsWindows() ? BrowserPlatform.Windows
            : OperatingSystem.IsLinux() ? BrowserPlatform.Linux
            : OperatingSystem.IsMacOS() ? BrowserPlatform.MacOS
            : BrowserPlatform.Unsupported;

        IOsPlatformResolver platformResolver = new DotNetOsPlatformResolver();

        // Act
        BrowserPlatform actual = platformResolver.GetCurrentPlatform();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetCurrentPlatform_ForcedLinuxReturnsLinux()
    {
        // Arrange
        BrowserPlatform expected = BrowserPlatform.Linux;

        IOsPlatformResolver platformResolver = new DotNetOsPlatformResolver()
        {
            IsWindows = () => false,
            IsLinux = () => true,
            IsMacOS = () => false
        };

        // Act
        BrowserPlatform actual = platformResolver.GetCurrentPlatform();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetCurrentPlatform_ForcedWindowsReturnsWindows()
    {
        // Arrange
        BrowserPlatform expected = BrowserPlatform.Windows;

        IOsPlatformResolver platformResolver = new DotNetOsPlatformResolver()
        {
            IsWindows = () => true,
            IsLinux = () => false,
            IsMacOS = () => false
        };

        // Act
        BrowserPlatform actual = platformResolver.GetCurrentPlatform();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetCurrentPlatform_ForcedMacOsReturnsMacOs()
    {
        // Arrange
        BrowserPlatform expected = BrowserPlatform.MacOS;

        IOsPlatformResolver platformResolver = new DotNetOsPlatformResolver()
        {
            IsWindows = () => false,
            IsLinux = () => false,
            IsMacOS = () => true
        };

        // Act
        BrowserPlatform actual = platformResolver.GetCurrentPlatform();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetCurrentPlatform_ForcedUnsupportedReturnsUnsupported()
    {
        // Arrange
        BrowserPlatform expected = BrowserPlatform.Unsupported;

        IOsPlatformResolver platformResolver = new DotNetOsPlatformResolver()
        {
            IsWindows = () => false,
            IsLinux = () => false,
            IsMacOS = () => false
        };

        // Act
        BrowserPlatform actual = platformResolver.GetCurrentPlatform();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FindOnPath_ReturnsNullWhenPathIsMissingOrEmpty(string? path)
    {
        // Arrange
        Func<string, bool> fileExists = _ => false;

        // Act
        string? result = SystemBrowserLauncher.FindOnPath("browser", path, fileExists);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindOnPath_ReturnsFirstMatchingExecutable()
    {
        // Arrange
        string firstDir = Path.Combine(Path.GetTempPath(), "first");
        string secondDir = Path.Combine(Path.GetTempPath(), "second");
        string firstCandidate = Path.Combine(firstDir, "browser");
        string secondCandidate = Path.Combine(secondDir, "browser");
        string path = $"{firstDir}{Path.PathSeparator}{secondDir}";

        // Act
        string? result = SystemBrowserLauncher.FindOnPath(
            "browser",
            path,
            candidate => string.Equals(candidate, secondCandidate, StringComparison.Ordinal));

        // Assert
        Assert.Equal(secondCandidate, result);
        Assert.NotEqual(firstCandidate, result, StringComparer.Ordinal);
    }

    [Fact]
    public void FindOnPath_IgnoresRelativeAndEmptyEntries()
    {
        // Arrange
        string path = $"relative{Path.PathSeparator}{Path.PathSeparator}.{Path.PathSeparator}..";

        // Act
        string? result = SystemBrowserLauncher.FindOnPath("browser", path, _ => true);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindOnPath_ReturnsNullWhenNoExecutableMatches()
    {
        // Arrange
        string path = $"first{Path.PathSeparator}second";

        // Act
        string? result = SystemBrowserLauncher.FindOnPath("browser", path, _ => false);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("Linux", null, "/usr/bin/xdg-open")]
    [InlineData("Linux", "/custom/xdg-open", "/custom/xdg-open")]
    [InlineData("MacOS", null, "/usr/bin/open")]
    [InlineData("MacOS", "/custom/open", "/custom/open")]
    public void Open_UsesPlatformExecutableAndArguments(
        string platformName,
        string? discoveredPath,
        string expectedExecutable)
    {
        // Arrange
        const string url = "http://localhost:5000";
        BrowserPlatform platform = Enum.Parse<BrowserPlatform>(platformName);
        IOsPlatformResolver nonWindowsPlatformResolver = new MockOsPlatformResolver(platform);

        ProcessStartInfo? capturedStartInfo = null;
        var launcher = new SystemBrowserLauncher(
            nonWindowsPlatformResolver,
            _ => discoveredPath,
            startInfo =>
            {
                capturedStartInfo = startInfo;
                return null;
            });

        // Act
        launcher.Open(url);

        // Assert
        Assert.NotNull(capturedStartInfo);
        Assert.Equal(expectedExecutable, capturedStartInfo.FileName);
        Assert.Equal("http://localhost:5000/", capturedStartInfo.Arguments);
        Assert.False(capturedStartInfo.UseShellExecute);
    }

    [Fact]
    public void Open_UsesShellExecuteForWindowsUrls()
    {
        // Arrange
        const string url = "http://localhost:5000";
        ProcessStartInfo? capturedStartInfo = null;
        IOsPlatformResolver windowsPlatformResolver = new MockOsPlatformResolver(BrowserPlatform.Windows);

        var launcher = new SystemBrowserLauncher(
            windowsPlatformResolver,
            _ => null,
            startInfo =>
            {
                capturedStartInfo = startInfo;
                return null;
            });

        // Act
        launcher.Open(url);

        // Assert
        Assert.NotNull(capturedStartInfo);
        Assert.Equal("http://localhost:5000/", capturedStartInfo.FileName);
        Assert.True(capturedStartInfo.UseShellExecute);
    }

    [Theory]
    [InlineData("not a url")]
    [InlineData("")]
    [InlineData("/relative/path")]
    public void Open_ThrowsForInvalidUrlsWithoutStartingProcess(string url)
    {
        // Arrange
        IOsPlatformResolver linuxPlatformResolver = new MockOsPlatformResolver(BrowserPlatform.Linux);

        bool processStarted = false;
        var launcher = new SystemBrowserLauncher(
            linuxPlatformResolver,
            _ => null,
            _ =>
            {
                processStarted = true;
                return null;
            });

        // Act
        Exception? exception = Record.Exception(() => launcher.Open(url));

        // Assert
        Assert.IsType<ArgumentException>(exception);
        Assert.False(processStarted);
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("ftp://localhost/file")]
    [InlineData("calc:")]
    public void Open_ThrowsForNonHttpSchemesWithoutStartingProcess(string url)
    {
        // Arrange
        IOsPlatformResolver windowsPlatformResolver = new MockOsPlatformResolver(BrowserPlatform.Windows);

        bool processStarted = false;
        var launcher = new SystemBrowserLauncher(
            windowsPlatformResolver,
            _ => null,
            _ =>
            {
                processStarted = true;
                return null;
            });

        // Act
        Exception? exception = Record.Exception(() => launcher.Open(url));

        // Assert
        Assert.IsType<ArgumentException>(exception);
        Assert.False(processStarted);
    }

    [Fact]
    public void Open_ThrowsForUnsupportedPlatformsWithoutStartingProcess()
    {
        // Arrange
        IOsPlatformResolver unsupportedPlatformResolver = new MockOsPlatformResolver();

        bool processStarted = false;
        var launcher = new SystemBrowserLauncher(
            unsupportedPlatformResolver,
            _ => null,
            _ =>
            {
                processStarted = true;
                return null;
            });

        // Act
        Exception? exception = Record.Exception(() => launcher.Open("http://localhost:5000"));

        // Assert
        Assert.IsType<PlatformNotSupportedException>(exception);
        Assert.False(processStarted);
    }

    [Fact]
    public void FindOnPath_UsesProcessEnvironmentPath()
    {
        // Arrange
        string executableName = $"browser-autolaunch-{Guid.NewGuid():N}";

        // Act
        string? result = SystemBrowserLauncher.FindOnPath(executableName);

        // Assert
        Assert.Null(result);
    }

    private sealed class RecordingBrowserLauncher : IBrowserLauncher
    {
        public List<ProcessStartInfo> StartInfo { get; } = [];

        public void Open(string url)
        {
            StartInfo.Add(new ProcessStartInfo(url));
        }
    }
}
