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

## Task: Golden Dataset Management System (Datasets/)

### What was done
- **GoldenDataset.cs**: Expanded from stub to full model with Name, Version, Description, CreatedAt, Tags, Examples. Added AddExample(), GetByTag(), GetSubset(), GetSummary() methods and DatasetSummary class.
- **GoldenExample.cs**: (in same file) Added Metadata dictionary, changed Tags to mutable List<string> for serialization.
- **DatasetLoader.cs**: Replaced stub DatasetLoader with IDatasetLoader interface (LoadAsync, SaveAsync, LoadFromCsvAsync) and JsonDatasetLoader implementation using System.Text.Json.
- **DatasetVersion.cs**: New file with DatasetVersion (Version, CreatedAt, ChangeDescription) and static Diff() method producing DatasetDiff (Added, Removed, Modified).
- **Example datasets**: Created 3 JSON files in datasets/ subfolder: chatbot-examples.json (7 examples), rag-examples.json (7 examples), summarization-examples.json (6 examples).
- All public types have XML doc comments.

### Build status
- Datasets files compile cleanly. Pre-existing build errors (RegressionReport not found) are unrelated.

## Task: Metrics and Scoring System (Metrics/)

### What was done
- **MetricScore.cs**: Added Weight (double, default 1.0) and Timestamp (DateTimeOffset) properties to existing stub. All properties have XML doc comments.
- **BaselineSnapshot.cs**: Replaced stub with full model — DatasetName, CreatedAt, Scores (Dictionary<string, double>), AggregateScore. Added SaveAsync/LoadAsync (JSON serialization via System.Text.Json) and Compare() method delegating to RegressionDetector.
- **RegressionDetector.cs**: Full implementation replacing NotImplementedException stub. Instance method HasRegression() uses configurable Tolerance (default 0.05). Static Compare() overloads accept BaselineSnapshot or raw Dictionary. Categorizes metrics into Improved/Regressed/Unchanged based on tolerance.
- **RegressionReport.cs**: Replaced previous stub (which had broken MetricComparison/Evaluators.EvaluationRun references) with clean model — Improved, Regressed, Unchanged dictionaries with (Baseline, Current) tuples, Tolerance, OverallPassed, HasRegressions.
- **AggregateScorer.cs**: New static class with ComputeWeightedAverage(), ComputeMinimum(), ComputePassRate() methods operating on IReadOnlyList<MetricScore>.
- **EvaluationRun.cs** (Evaluators/): Enhanced existing stub with RunId (Guid), StartedAt, CompletedAt, Duration, TotalTokens, EstimatedCost, AggregateScore, PassRate, AllPassed, and ToBaseline() method. Kept in Evaluators namespace for backward compatibility.
- **ChatClientExtensions.cs**: Fixed Timestamp → StartedAt reference to match updated EvaluationRun.

### Build status
- Full solution builds cleanly with zero warnings and zero errors.

## Task: Evaluators Implementation (Evaluators/)

### What was done
- **HallucinationEvaluator.cs**: Keyword overlap approach — tokenizes output and grounding context (input + expectedOutput), scores by ratio of grounded tokens. Configurable threshold (default 0.7).
- **FactualityEvaluator.cs**: Extracts sentence-level claims from output, checks each against reference tokens (≥50% overlap = supported). Score = supported/total. Threshold 0.8.
- **RelevanceEvaluator.cs**: Cosine similarity of term-frequency vectors between input and output. Threshold 0.6.
- **CoherenceEvaluator.cs**: Structural checks — sentence completeness (≥3 words), contradiction detection (paired antonym scan), repetition detection. Threshold 0.7.
- **SafetyEvaluator.cs**: Default profanity blocklist + configurable additional terms. PII regex detection (email, SSN, phone via GeneratedRegex). Penalty-per-violation model. Threshold 0.9.
- **EvaluationResult.cs**: Added ToString() → `[PASS/FAIL] Score=X.XX — details`.
- **MetricScore.cs**: Added ToString() → `Name=X.XX (threshold=Y.YY, PASS/FAIL)`.
- **RegressionDetector.cs**: Removed duplicate RegressionReport class (already in RegressionReport.cs).
- All evaluators are deterministic (no LLM calls), implement IEvaluator, have XML doc comments, and return EvaluationResult with MetricScores.

### Design decisions
- v1 uses heuristic/algorithmic approaches only — no external LLM dependency
- Tokenization strips punctuation and filters tokens ≤2 chars for noise reduction
- SafetyEvaluator uses source-generated regex (GeneratedRegex) for performance
- Factuality claim extraction splits on sentence boundaries, requires ≥3 words per claim

### Build status
- Full solution builds cleanly with zero warnings and zero errors.

## Task: IChatClient Extension Methods & EvaluationPipeline

### What was done
- **ChatClientExtensions.cs**: Replaced single-evaluator stub with three full extension methods:
  - `EvaluateAsync(IChatClient, GoldenExample, IEnumerable<IEvaluator>, CancellationToken)` → EvaluationResult: Sends example input to chat client via GetResponseAsync, runs all evaluators, aggregates scores/metrics/pass status.
  - `EvaluateAsync(IChatClient, GoldenDataset, IEnumerable<IEvaluator>, CancellationToken)` → EvaluationRun: Iterates all dataset examples, collects results into EvaluationRun with dataset name and timestamp.
  - `CompareBaselineAsync(IChatClient, GoldenDataset, IEnumerable<IEvaluator>, BaselineSnapshot, CancellationToken)` → RegressionReport: Runs full evaluation, aggregates per-metric averages, delegates comparison to RegressionDetector.Compare().
- **EvaluationPipeline.cs**: New file with fluent builder pattern:
  - `EvaluationPipelineBuilder` with WithChatClient(), WithDataset(), AddEvaluator(), WithBaseline(), Build() — validates required components.
  - `EvaluationPipeline` with RunAsync() and RunWithBaselineAsync() delegating to ChatClientExtensions.
- All public types and methods have XML doc comments.

### Build status
- Full solution (all projects, tests, samples) builds cleanly with zero warnings and zero errors.

## Task: Sample Projects Implementation (samples/)

### What was done
- **samples/ChatbotEvaluation/Program.cs**: Replaced placeholder with full working sample demonstrating:
  - EchoChatClient mock implementing IChatClient with known Q&A responses
  - Loading chatbot golden dataset via JsonDatasetLoader from bundled datasets/ folder (with inline fallback)
  - Running evaluation with HallucinationEvaluator, RelevanceEvaluator, SafetyEvaluator
  - Console reporting with per-example scores and metric breakdowns
  - SQLite persistence using Microsoft.Data.Sqlite directly (since SqliteResultStore is stub)
  - Baseline creation via EvaluationRun.ToBaseline() and regression detection via CompareBaselineAsync
- **samples/RagEvaluation/Program.cs**: Replaced placeholder with full working sample demonstrating:
  - MockRagChatClient simulating retrieval-augmented generation pipeline
  - Loading RAG golden dataset with context fields
  - Running evaluation with FactualityEvaluator, HallucinationEvaluator, CoherenceEvaluator
  - Baseline creation, regression detection with detailed report (improved/regressed/unchanged)
  - JSON export of full results including metrics, baseline, and regression data
- **Both .csproj files**: Added ProjectReference to ElBruno.AI.Evaluation.Reporting for Microsoft.Data.Sqlite dependency
- **README.md files**: Created for both samples with run instructions, feature list, and API reference tables
- Both mock clients implement full IChatClient interface (GetResponseAsync, GetStreamingResponseAsync, Dispose, GetService)

### Build status
- Both samples build cleanly with zero warnings and zero errors.

## Task: xUnit Integration & Reporting Packages

### What was done

#### ElBruno.AI.Evaluation.Xunit
- **AIEvaluationTestAttribute.cs**: Replaced stub Attribute with FactAttribute subclass. Properties: DatasetPath (string?), Evaluators (Type[]?), Threshold (double, default 0.7). Tests are discoverable in Test Explorer via xUnit's FactAttribute inheritance.
- **AIAssert.cs**: Expanded from 2 methods to 7 fluent assertions: LLMOutputSatisfies(), MeetsBaseline(), NoHallucinations(), IsSafe(), IsRelevant(), IsCoherent(), PassesAllEvaluators(). Each throws Xunit.Sdk.XunitException with descriptive messages on failure.
- **AITestRunner.cs**: New fluent test helper — `new AITestRunner(chatClient).WithDataset("path").AddEvaluator<T>().RunAsync()`. Supports both file-path and pre-loaded GoldenDataset. Delegates to ChatClientExtensions.EvaluateAsync.

#### ElBruno.AI.Evaluation.Reporting
- **SqliteResultStore.cs**: Full async SQLite implementation using Microsoft.Data.Sqlite. Factory method CreateAsync() creates schema (Runs + Results tables). SaveRunAsync() persists runs with transaction. GetRunsAsync() with optional dataset filter and limit. GetLatestBaselineAsync() for regression workflows. Implements IAsyncDisposable.
- **ConsoleReporter.cs**: Pretty-prints with emoji icons (✅/❌). Summary line with pass count, percentage, avg score. Per-evaluator breakdown. Failures section. Optional Verbose mode for per-example detail.
- **JsonExporter.cs**: Exports EvaluationRun to JSON via System.Text.Json with indented formatting. Creates output directories as needed.
- **CsvExporter.cs**: Exports one row per result with dynamic metric columns. Proper CSV escaping. Uses InvariantCulture for numeric formatting.

### Build status
- All three src projects (Core, Xunit, Reporting) and both test projects build cleanly. Pre-existing sample errors (IChatClient.GetStreamingResponseAsync) are unrelated.

## Task: Synthetic Data Generation Library (SyntheticData/)

### What was done
- **ElBruno.AI.Evaluation.SyntheticData.csproj**: New project with PackageId, IsPackable=true, ProjectReference to core, Microsoft.Extensions.AI.Abstractions 9.5.0, pack items for logo and README.
- **ISyntheticDataGenerator.cs**: Core interface with GenerateAsync(count, ct) returning IReadOnlyList<GoldenExample>.
- **DeterministicGenerator.cs**: Template-based generation with optional seed. Supports QaTemplate, RagTemplate, AdversarialTemplate, DomainTemplate dispatch with perturbation logic (null injection, truncation, typos, contradictions, long inputs).
- **LlmGenerator.cs**: IChatClient-powered generation with configurable temperature, max tokens, parallelism. JSON response parsing with fallback to raw content.
- **CompositeGenerator.cs**: Weighted combination of sub-generators, proportional count allocation.
- **IDataTemplate.cs**: Base interface (TemplateType, Tags, Metadata).
- **QaTemplate.cs**: Q&A pair generation with category, tags, metadata. GetPairs() zips question/answer templates.
- **RagTemplate.cs**: RAG context+answer with documents and QA examples.
- **AdversarialTemplate.cs**: Edge-case generation with configurable perturbation types.
- **DomainTemplate.cs**: Domain-specific with vocabulary, constraints, compliance framework.
- **GenerationStrategy.cs**: GenerationTemplate enum (SimpleQA, RagContext, QAWithExplanation, QAVariations, AdversarialCases, DomainSpecific).
- **TemplateStrategy.cs / LlmStrategy.cs**: Strategy configuration classes.
- **SyntheticDatasetBuilder.cs**: Fluent builder with UseDeterministicGenerator, UseLlmGenerator, UseCompositeGenerator, scenario helpers, BuildAsync. Also contains CompositeGeneratorConfig.
- **SyntheticDatasetExtensions.cs**: AugmentWithSyntheticExamplesAsync, Merge, Deduplicate, ValidateExamples with ValidationOptions/ValidationError.
- **RandomSeedProvider.cs**: Static helper for deterministic seed creation and derivation.
- **PromptGenerator.cs**: LLM prompt composition per GenerationTemplate.

### Build status
- Full solution (all projects) builds cleanly with zero warnings and zero errors.

### Learnings
- CompositeGeneratorConfig and SyntheticDatasetBuilder live in the same file since CompositeGeneratorConfig is tightly coupled to the builder API
- AdversarialTemplate needs internal GetEnabledPerturbations() for DeterministicGenerator — used internal access modifier
- LlmGenerator JSON parsing uses flexible field names (input/question, expected_output/answer) for robustness
