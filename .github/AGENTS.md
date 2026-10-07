# Instructions

## Code Guidelines
- No inline comments in the code, except for one-line Arrange/Act/Assert test-phase separators and necessary "why" comments on hard-to-understand code parts.
- No ADRs in the code allowed.
- No compiler+analyzer warnings allowed.
- Use available libraries/NuGet instead of implementing solutions on your own. Must have no vulnerabilities.
- No magic constants allowed. Always use enums and/or named constants.

## Source Control Guidelines
- No secrets in the repository allowed.
- Commit each of the steps from your plan. Use "GitHub Copilot" as your GIT username.
- Commit message has the form of: a bullet character -> whitespace -> literal "[AI]" -> whitespace -> single line on "what has changed" -> newline -> several lines on "why it has changed".
- The bullet characters are: ➕ for feature additions, 🛠 for feature fixes, 🔧 for small code fixes, 🧹 for code cleanups, 💣 for potentially code-breaking changes, 🤘 for feature done.

## Testing Guidelines
- Each test method contains one-line comments separating "arrange," "act," and "assert" phases, if they exist.
- Exempt test projects from code coverage enforcement by [ExcludeFromCodeCoverage] on assembly level.
- Tests must cover edge cases, too.
- You may freely add tests. Updates and deletions of existing tests must be approved by me.

## CI/CD Guidelines
- Everything in CI must be either for bash or for PowerShell Core. Use whichever results in shorter code.
- CI steps longer than 3 lines must be moved to their own script files under .github/ci folder.
