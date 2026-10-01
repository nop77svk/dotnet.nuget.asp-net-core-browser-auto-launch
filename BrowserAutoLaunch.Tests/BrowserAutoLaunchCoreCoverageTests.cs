namespace NoP77svk.AspNetCore.BrowserAutoLaunch.Tests;

using System.Diagnostics;
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
    public async Task BrowserAutoLaunchService_ResolvesLoggerWhenConstructorLoggerIsNull()
    {
        // Arrange
        await using var app = WebApplication.CreateBuilder().Build();
        var launcher = new RecordingBrowserLauncher();
        var service = new BrowserAutoLaunchService(app, null, launcher);

        // Act
        service.LaunchTheBrowserFromApplication();

        // Assert
        Assert.Empty(launcher.StartInfo);
    }

    [Theory]
    [InlineData(true, false, false, "Windows")]
    [InlineData(false, true, false, "Linux")]
    [InlineData(false, false, true, "MacOS")]
    [InlineData(false, false, false, "Unsupported")]
    public void DeterminePlatform_ReturnsExpectedPlatform(
        bool isWindows,
        bool isLinux,
        bool isMacOS,
        string expected)
    {
        // Arrange

        // Act
        BrowserPlatform actual = SystemBrowserLauncher.DeterminePlatform(isWindows, isLinux, isMacOS);

        // Assert
        Assert.Equal(Enum.Parse<BrowserPlatform>(expected), actual);
    }

    [Fact]
    public void GetCurrentPlatform_MatchesOperatingSystemDetection()
    {
        // Arrange
        BrowserPlatform expected = SystemBrowserLauncher.DeterminePlatform(
            OperatingSystem.IsWindows(),
            OperatingSystem.IsLinux(),
            OperatingSystem.IsMacOS());

        // Act
        BrowserPlatform actual = SystemBrowserLauncher.GetCurrentPlatform();

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
        string firstCandidate = Path.Combine("first", "browser");
        string secondCandidate = Path.Combine("second", "browser");
        string path = $"first{Path.PathSeparator}second";

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
        ProcessStartInfo? capturedStartInfo = null;
        var launcher = new SystemBrowserLauncher(
            () => platform,
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
        Assert.Equal(url, capturedStartInfo.Arguments);
        Assert.False(capturedStartInfo.UseShellExecute);
    }

    [Fact]
    public void Open_UsesShellExecuteForWindowsUrls()
    {
        // Arrange
        const string url = "http://localhost:5000";
        ProcessStartInfo? capturedStartInfo = null;
        var launcher = new SystemBrowserLauncher(
            () => BrowserPlatform.Windows,
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
        Assert.Equal(url, capturedStartInfo.FileName);
        Assert.True(capturedStartInfo.UseShellExecute);
    }

    [Fact]
    public void Open_ThrowsForUnsupportedPlatformsWithoutStartingProcess()
    {
        // Arrange
        bool processStarted = false;
        var launcher = new SystemBrowserLauncher(
            () => BrowserPlatform.Unsupported,
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
