$ErrorActionPreference = "Stop"

$reportRelativePath = "TestResults/coverage.cobertura.xml"

# ------------------------------------------------------------------------------------------------
# resolve absolute path to GIT worktree root

$workspace = $env:GITHUB_WORKSPACE

if ([string]::IsNullOrEmpty($workspace))
{
	Push-Location $PSScriptRoot
	$workspace = ( git rev-parse --show-toplevel )
	Pop-Location
}

if ([string]::IsNullOrEmpty($workspace))
{
	$workspace = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path
}

# ------------------------------------------------------------------------------------------------
	
$reportPath = Join-Path $workspace $reportRelativePath
if (-not (Test-Path $reportPath))
{
	throw "Coverage report not found: $reportPath"
}

[xml]$report = Get-Content -Raw $reportPath
$corePackages = @($report.coverage.packages.package
	| Where-Object { -not $_.GetAttribute("name").EndsWith(".Tests") })

if ($corePackages.Count -lt 1)
{
	throw "Expected at least some non-.Tests package in $reportPath; found $($corePackages.Count)"
}

foreach ($core in $corePackages)
{
	$projectName = $core.name

	if (@($core.classes.class).Count -eq 0)
	{
		throw "The ${projectName} project has no class coverage data"
	}

	foreach ($metric in @("line", "branch"))
	{
		$ratePct = [Math]::Floor(100.0 * $core.GetAttribute("$metric-rate"))
		if ([string]::IsNullOrEmpty($ratePct) -or [double]::Parse($ratePct, [Globalization.CultureInfo]::InvariantCulture) -lt 100.0)
		{
			throw "${projectName} $metric coverage is $ratePct%; expected 100%"
		}

		Write-Output "${projectName} $metric coverage: $ratePct%"
	}
}
