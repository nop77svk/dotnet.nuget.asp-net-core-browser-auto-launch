# Copilot Instructions

## General Guidelines
- No inline comments in the code, except for one-line Arrange/Act/Assert test-phase separators and necessary "why" comments on hard-to-understand code parts.
- No ADRs in the code.
- No compiler warnings allowed.
- Use available libraries/NuGet instead of implementing solutions on your own.
- No secrets in the repository allowed.

## Commit Guidelines
- Commit each of your code-changing steps.
- Commit message has the first line describing the "what has changed" and a few following lines describing "why it has changed."
- Commit message starts with a bullet character: ➕ for feature additions, 🛠 for feature fixes, 🔧 for small code fixes, 🧹 for code cleanups, 💣 for potentially code-breaking changes, 🤘 for feature done.

## Testing Guidelines
- Each test method contains one-line comments separating "arrange," "act," and "assert" phases, if they exist.
- Exempt test projects from code coverage enforcement.

## CI/CI Guidelines
- Everything in CI must be either for bash or for PowerShell Core. Use whichever results in shorter code.
- CI steps longer than 5 lines must be moved to their own script files under .github/ci folder.
