---
applyTo:
  - "**/*.cs"
  - "**/*.csproj"
---

## General Guidelines

- Use .NET Standard 2.1 where applicable, use .NET 10 elsewhere.
- Private class fields start with a single underscore.
- 100% line coverage and branch coverage.
- Use the latest xUnit v3 via Microsoft.Testing.Platform as test framework.

## Patterns we don't use

- AutoMapper. We use manual mapping instead.
- MediatR.
- Repository Pattern. We use EF Core instead.
