# BrowserAutoLaunch test tiers

The normal test suite uses a fake browser launcher and does not start external browser processes. Run it with:

```powershell
dotnet test BrowserAutoLaunch.Tests/BrowserAutoLaunch.Tests.csproj
```

The `BrowserSmoke` test is skipped unless `RUN_BROWSER_SMOKE_TEST` is set to `1`. It starts a local Kestrel host and asks the operating system to open its default browser. Run it from PowerShell with:

```powershell
$env:RUN_BROWSER_SMOKE_TEST = "1"
dotnet test BrowserAutoLaunch.Tests/BrowserAutoLaunch.Tests.csproj --filter "Category=BrowserSmoke"
Remove-Item Env:RUN_BROWSER_SMOKE_TEST
```

Run this test only in an interactive desktop session with a default browser configured. Linux requires a graphical session and `xdg-open`; macOS uses `open`; Windows uses the shell URL handler. The test may leave a browser window open. It verifies that the OS accepts the browser launch request; it does not automate browser navigation or validate browser rendering.
