$ErrorActionPreference = "Stop"

$workspace = if ($env:GITHUB_WORKSPACE) { $env:GITHUB_WORKSPACE } else { (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path }
$reportPath = Join-Path $workspace "TestResults/coverage.cobertura.xml"
if (-not (Test-Path $reportPath)) {
	throw "Coverage report not found: $reportPath"
}

[xml]$report = Get-Content -Raw $reportPath
$corePackages = @($report.coverage.packages.package | Where-Object { -not $_.GetAttribute("name").EndsWith(".Tests") })
if ($corePackages.Count -lt 1) {
	throw "Expected at least some non-.Tests package in $reportPath; found $($corePackages.Count)"
}

foreach ($core in $corePackages)
{
	$projectName = $core.name

	if (@($core.classes.class).Count -eq 0) {
		throw "The ${projectName} project has no class coverage data"
	}

	foreach ($metric in @("line", "branch"))
	{
		$rate = $core.GetAttribute("$metric-rate")
		if ([string]::IsNullOrEmpty($rate) -or [double]::Parse($rate, [Globalization.CultureInfo]::InvariantCulture) -ne 1.0) {
			throw "${projectName} $metric coverage is $rate; expected 1.0 (100%)"
		}

		Write-Output "${projectName} $metric coverage: $rate (100%)"
	}
}
