# Decisions

<!-- Team decisions are recorded here by Scribe. Append-only. -->

## Decision: Solution Structure for ElBruno.AI.Evaluation

**Author:** Byers (Senior .NET Developer)  
**Date:** 2025-07-15  
**Status:** Implemented

### Decision
Created the complete .NET solution structure with 7 projects across src/, tests/, and samples/ directories.

### Structure
- **src/ElBruno.AI.Evaluation** — Core library with IEvaluator interface, 5 evaluator stubs, dataset models, metrics, and IChatClient extensions
- **src/ElBruno.AI.Evaluation.Xunit** — xUnit integration (AIAssert, AIEvaluationTestAttribute)
- **src/ElBruno.AI.Evaluation.Reporting** — SQLite persistence and export (JSON, CSV, Console)
- **tests/ElBruno.AI.Evaluation.Tests** — Unit tests
- **tests/ElBruno.AI.Evaluation.IntegrationTests** — Integration tests
- **samples/ChatbotEvaluation** — Console sample
- **samples/RagEvaluation** — Console sample

### Key Design Choices
- `IEvaluator.EvaluateAsync` takes input, output, optional expectedOutput, and CancellationToken
- `EvaluationResult` uses required init properties (Score, Passed) with optional Details and MetricScores dictionary
- `MetricScore.Passed` is a computed property (no threshold = always pass)
- `GoldenDataset` uses `IReadOnlyList` for immutable collections
- `Directory.Build.props` centralizes all shared settings — individual csproj files stay minimal

---

## Decision: Naming Convention - ElBruno Prefix

**Author:** Copilot (User Directive: Bruno Capuano)  
**Date:** 2026-02-24T00:24:00Z  
**Status:** Established

### Decision
The main prefix for everything should be [ElBruno.], e.g. ElBruno.AI.Evaluation

### Rationale
User request — captured for team memory
