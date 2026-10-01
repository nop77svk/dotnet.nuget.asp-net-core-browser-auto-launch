$ErrorActionPreference = "Stop"

$workspace = if ($env:GITHUB_WORKSPACE) { $env:GITHUB_WORKSPACE } else { (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path }
$reportPath = Join-Path $workspace "TestResults/coverage.cobertura.xml"
if (-not (Test-Path $reportPath)) {
	throw "Coverage report not found: $reportPath"
}

[xml]$report = Get-Content -Raw $reportPath
$corePackages = @($report.coverage.packages.package | Where-Object { $_.GetAttribute("name").EndsWith("BrowserAutoLaunch.Core") })
if ($corePackages.Count -ne 1) {
	throw "Expected one BrowserAutoLaunch.Core package in $reportPath; found $($corePackages.Count)"
}

$core = $corePackages[0]
if (@($core.classes.class).Count -eq 0) {
	throw "BrowserAutoLaunch.Core has no class coverage data"
}

foreach ($metric in @("line", "branch")) {
	$rate = $core.GetAttribute("$metric-rate")
	if ([string]::IsNullOrEmpty($rate) -or [double]::Parse($rate, [Globalization.CultureInfo]::InvariantCulture) -ne 1.0) {
		throw "BrowserAutoLaunch.Core $metric coverage is $rate; expected 1.0 (100%)"
	}
	Write-Output "BrowserAutoLaunch.Core $metric coverage: $rate (100%)"
}
