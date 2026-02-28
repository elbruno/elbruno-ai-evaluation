# History

## Project Context
- **Project:** ElBruno.AI.Evaluation — AI Testing & Observability Toolkit for .NET
- **User:** Bruno Capuano (Developer Advocate, Microsoft Cloud & GitHub Technologies)
- **Goal:** Build production-grade NuGet packages for AI testing in .NET
- **Stack:** .NET 8+, C#, xUnit, Microsoft.Extensions.AI, SQLite
- **Packages:** ElBruno.AI.Evaluation, ElBruno.AI.Evaluation.Xunit, ElBruno.AI.Evaluation.Reporting

## Learnings
- SyntheticData generators use `ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count)` — zero count throws, doesn't return empty
- New evaluators (Latency, Cost, Conciseness, Consistency, Completeness) use non-standard signatures — LatencyEvaluator takes `elapsedMs` param, CostEvaluator uses `budgetPerCall`/`costPerToken` constructor args
- DatasetLoader extension methods (SaveToCsvAsync, CsvColumnMapping) are expected additions — tests written ahead of implementation
- Writing tests-first against a spec while implementation is parallel means compile errors are expected and acceptable; tests serve as a contract for the implementer
- LlmGenerator JSON parser expects `expected_output` (snake_case) and `answer`/`question` as fallback field names, not camelCase `expectedOutput`
- LlmGenerator auto-tags examples with `["synthetic", "llm", "<template>"]` and adds generator/template metadata
- All fluent builder/template methods return `this` for chaining (Assert.Same pattern works)
- When writing tests against a design spec while implementation is in parallel, actual behavior may differ — always verify against implementation once available

## Work Log

### 2025-07-15 — Comprehensive Test Suite Implementation
**Task:** Write comprehensive tests for ElBruno.AI.Evaluation toolkit.

**Unit Tests Created (tests/ElBruno.AI.Evaluation.Tests/):**
- `Datasets/GoldenDatasetTests.cs` — 7 tests: create, add examples, null guard, filter by tag (case-insensitive), get subset, summary stats, version tracking
- `Datasets/DatasetLoaderTests.cs` — 6 tests: JSON save/load round-trip, missing file, invalid JSON, CSV parsing, empty CSV, missing CSV columns
- `Evaluators/HallucinationEvaluatorTests.cs` — 5 tests: grounded output, hallucinated output, empty output, null expected, custom threshold
- `Evaluators/FactualityEvaluatorTests.cs` — 5 tests: factual match, non-factual divergence, empty output, null expected, custom threshold
- `Evaluators/RelevanceEvaluatorTests.cs` — 5 tests: relevant output, irrelevant output, empty output, identical I/O, custom threshold
- `Evaluators/CoherenceEvaluatorTests.cs` — 5 tests: coherent text, contradictions, empty output, repetitive text, custom threshold
- `Evaluators/SafetyEvaluatorTests.cs` — 6 tests: safe content, email PII, SSN PII, profanity, empty output, custom blocklist
- `Metrics/AggregateScorerTests.cs` — 8 tests: weighted avg (equal/different weights/empty), minimum (normal/empty), pass rate (all/some/empty)
- `Metrics/RegressionDetectorTests.cs` — 5 tests: no drops, significant drop, classify improved/unchanged/regressed, missing metric, custom tolerance
- `Metrics/BaselineSnapshotTests.cs` — 4 tests: save/load round-trip, missing file, compare with regressions, compare no regressions

**Integration Tests Created (tests/ElBruno.AI.Evaluation.IntegrationTests/):**
- `PipelineIntegrationTests.cs` — 5 tests: full pipeline E2E, multiple evaluators, baseline comparison, fluent builder, missing client validation
- `ReportingIntegrationTests.cs` — 4 tests: run-to-baseline save/reload, JSON round-trip then evaluate, baseline comparison E2E, multi-evaluator workflow
- Includes `MockChatClient` implementing `IChatClient` for deterministic testing

**Results:** 67 tests total (57 unit + 10 integration), all passing.

**Notes:**
- Fixed SmokeTests.cs namespace conflict (fully qualified `ElBruno.AI.Evaluation.Evaluators.EvaluationResult`)
- Added `Microsoft.Extensions.AI.Abstractions` package to integration test csproj for `IChatClient` mock
- Some evaluator behaviors discovered during testing: HallucinationEvaluator returns 1.0 for empty output, RelevanceEvaluator cosine similarity can be low even for semantically related text

### 2025-02-24 — CI/Linux Compatibility Audit (GitHub Issue #1)
**Task:** Audit test suite for CI/Linux compatibility issues per LocalEmbeddings findings.

**Audit Scope:**
- Platform-conditional test skipping (Skip.IfNot/Skip.If requiring [SkippableFact])
- Path.GetInvalidFileNameChars() usage in tests
- File name validation patterns
- Git tag version parsing in publish workflow

**Findings:**
- **[SkippableFact]:** ✅ Zero tests use Skip.* methods — all 164 tests use standard [Fact]/[Theory]
- **File name validation:** ✅ Zero tests use Path.GetInvalidFileNameChars() or InvalidPathChars
- **Platform detection:** ✅ Zero tests use RuntimeInformation or OS detection APIs
- **Cross-platform paths:** ✅ All tests use Path.Combine() and Path.GetTempPath() correctly
- **CI workflow:** ✅ Already runs on ubuntu-latest with standard xUnit runner
- **Publish workflow:** ⚠️ Tag version parsing strips leading 'v' but could add trailing '.' protection (preventative, non-blocking)

**Test Quality Patterns Observed:**
- 28 test files, 164 total tests (163 [Fact] + 1 [Theory])
- All tests are platform-agnostic by design
- No hardcoded Windows paths or backslashes
- DatasetLoader tests demonstrate best practices for temp file handling
- No timing-dependent assertions or Thread.Sleep() found

**Conclusion:** Test suite is **fully CI/Linux compatible**. No blocking issues or required fixes. Confidence: 95/100.

**Output:** `.squad/decisions/inbox/langly-ci-linux-audit.md` — comprehensive audit report
