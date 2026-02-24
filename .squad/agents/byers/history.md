# History

## Project Context
- **Project:** ElBruno.AI.Evaluation — AI Testing & Observability Toolkit for .NET
- **User:** Bruno Capuano (Developer Advocate, Microsoft Cloud & GitHub Technologies)
- **Goal:** Build production-grade NuGet packages for AI testing in .NET
- **Stack:** .NET 8+, C#, xUnit, Microsoft.Extensions.AI, SQLite
- **Packages:** ElBruno.AI.Evaluation, ElBruno.AI.Evaluation.Xunit, ElBruno.AI.Evaluation.Reporting

## Learnings
- Solution structure created with 3 src projects, 2 test projects, 2 samples
- Directory.Build.props centralizes TFM (net8.0), nullable, implicit usings, and NuGet metadata
- Microsoft.Extensions.AI.Abstractions 9.5.0 is the latest stable for IChatClient integration
- xunit 2.9.3 requires explicit `using Xunit;` — ImplicitUsings doesn't cover it
- MetricScore.Passed is computed property (Value >= Threshold) — no setter needed
