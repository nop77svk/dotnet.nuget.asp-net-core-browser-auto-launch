# xUnit v3 on MTP and Core Coverage

Created: 2026-10-01 11:09:40

## Goal
Upgrade the .NET 10 test project to the latest xUnit v3 integration on Microsoft.Testing.Platform, and meet the repository's 100% line and branch coverage requirement for BrowserAutoLaunch.Core.

## Scope
- Keep the browser smoke test opt-in and skipped by default.
- Use MTP's `dotnet test` integration and xUnit v3.
- Add deterministic Core tests without launching a browser.
- Retain the coverage package and report Core line/branch coverage.
- Mutation testing was explicitly dropped at the user's request.
- Do not include or alter the user-edited `.github/instructions/dotnet.instructions.md` in the implementation commit.

## Implementation
1. Resolve compatible current packages: xunit.v3 4.0.1, Microsoft.Testing.Platform 2.4.1, Microsoft.Testing.Platform.MSBuild 2.4.1, and Microsoft.Testing.Extensions.CodeCoverage 18.11.2.
2. Configure the test project for MTP executable output and select the MTP runner in the solution-root `global.json`.
3. Preserve the browser smoke test's discovery-time skip with xUnit v3 source-location metadata, and add Arrange/Act/Assert separators to test methods.
4. Add internal test seams for platform detection, path lookup, and process-start configuration; add deterministic Core branch tests.
5. Run MTP coverage until BrowserAutoLaunch.Core reports 100% line and branch coverage.
6. Run the full test suite and build the complete solution with no compiler warnings.

## Validation Results
- Core coverage: 140/140 lines and 38/38 branches (100% / 100%).
- MTP test run: 27 passed, 1 opt-in browser test skipped, 0 failed.
- Full solution build: successful with no warnings.

## Commit
Pending; use the repository's required categorized first-line prefix and explanatory body.
