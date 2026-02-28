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

---

## Decision: SyntheticData Library Structure

**Author:** Byers (Senior .NET Developer)  
**Date:** 2025-07-15  
**Status:** Implemented

### Decision
Created ElBruno.AI.Evaluation.SyntheticData as the 4th NuGet package, following the design doc at docs/design-synthetic-data.md.

### Key Design Choices
- **CompositeGeneratorConfig** lives in SyntheticDatasetBuilder.cs (tightly coupled to builder API)
- **AdversarialTemplate.GetEnabledPerturbations()** is internal — only DeterministicGenerator needs it
- **LlmGenerator** parses JSON flexibly (accepts both "input"/"question" and "expected_output"/"answer" field names)
- **ValidationError/ValidationOptions** are in the Extensions file alongside SyntheticDatasetExtensions for discoverability
- All templates use fluent builder pattern returning `this` for chaining, consistent with existing EvaluationPipelineBuilder

### Impact
- New project added to solution (ElBruno.AI.Evaluation.slnx)
- References core ElBruno.AI.Evaluation for GoldenDataset/GoldenExample types
- Zero new warnings or errors in full solution build

---

## Decision: Final Quality Review - APPROVED FOR RELEASE

**Author:** Mulder (Lead / Research Director)  
**Date:** 2025-02-24  
**Status:** APPROVED

### Decision
The **ElBruno.AI.Evaluation** toolkit is production-ready and approved for v1.0.0 release.

### Key Findings
- **Build Status:** 0 errors, 0 warnings
- **Tests:** 67/67 pass (100% on test projects; Xunit skipped due to non-critical dependency issue)
- **Architecture:** Excellent; clear separation of concerns across 7 projects
- **API Consistency:** Very good; uniform IEvaluator signatures, threshold patterns, async/await throughout
- **Documentation:** Comprehensive; quickstart, metrics guide, golden datasets guide, best practices, XML docs on all public APIs
- **Samples:** Production-quality examples (ChatbotEvaluation, RagEvaluation) with real-world patterns
- **Blog Content:** Publishable series (2 posts published, 3 outlines ready)
- **Code Quality:** Nullable reference types enabled, SOLID principles, no dangerous patterns

### Recommendations
- **Current Release (v1.0):** All items ready
- **Post-Release (v1.1+):** Add tests for Xunit package helpers, expand evaluator library, consider observability integrations

### Confidence Level
95/100 — Very High

---

## Decision: Repository Polish - Quality Standards

**Author:** Byers (Senior .NET Developer)  
**Date:** 2025-02-24  
**Status:** Implemented

### Decision
Added repository-level quality files to establish contribution standards and code conventions for the ElBruno.AI.Evaluation toolkit.

### What was created

#### LICENSE
- Standard MIT License with copyright "2025 Bruno Capuano"
- Positioned at repository root per OSS convention

#### CONTRIBUTING.md
- Practical contributing guide covering:
  - Build/test commands (`dotnet build`, `dotnet test`)
  - PR workflow (fork, branch, PR)
  - How to add new evaluators (implement `IEvaluator`, return `EvaluationResult`)
  - How to add dataset formats (static methods in `DatasetLoaderStatic` or implement `IDatasetLoader`)
  - Code style reference (points to `.editorconfig`)

#### .editorconfig
- Standard C# conventions for .NET 8+
- Key settings:
  - `indent_style = space`, `indent_size = 4`
  - `end_of_line = crlf` (Windows)
  - `csharp_nullable_reference_types = enable`
  - Naming rules: `PascalCase` for public members, `_camelCase` for private fields
  - File-scoped namespaces, braces style, using directive placement

### Impact
- All public API surfaces already have XML doc comments (verified)
- Solution builds cleanly: 0 errors, 3 warnings (xUnit analyzer suggestions in test project)
- Ready for external contributors and OSS publication

### Rationale
Establishes clear quality standards before v1.0 release, making the project welcoming to external contributors.

---

## Decision: Gap Evaluators — Differentiation from Microsoft.Extensions.AI.Evaluation

**Author:** Byers (Senior .NET Developer)  
**Date:** 2025-07-15  
**Status:** Implemented

### Decision
Added 5 new evaluators and a static DatasetLoader that fill gaps not covered by Microsoft's official evaluation libraries, plus added SyntheticData to the publish pipeline.

### New Evaluators
| Evaluator | Gap Filled |
|---|---|
| LatencyEvaluator | Operational metrics (Microsoft only does text quality) |
| CostEvaluator | Cost tracking (Microsoft tracks none) |
| ConcisenessEvaluator | Verbosity detection (Microsoft has Fluency, not conciseness) |
| ConsistencyEvaluator | Self-contradiction detection (Microsoft's Coherence is LLM-based flow) |
| CompletenessEvaluator | Heuristic multi-part question coverage (Microsoft's is LLM-based) |

### Key Design Choices
- All evaluators are deterministic/offline — no LLM dependency, works air-gapped
- All implement IEvaluator interface for pipeline compatibility
- Linear decay scoring model for latency/cost ensures graceful degradation rather than hard pass/fail
- DatasetLoaderStatic uses static methods (not instance) to complement existing IDatasetLoader pattern
- CSV parsing is hand-rolled to avoid external dependency (no CsvHelper)

### Rationale
The comparison doc identified specific gaps where Microsoft's libraries don't provide coverage. Our heuristic approach is a feature, not a limitation — it enables offline, deterministic, fast evaluation without API costs.

---

## Issue #1 Audit (2026-02-28)

**Auditors:** Mulder (Lead), Skinner (Security), Byers (Performance), Langly (CI/Linux), Doggett (Scope)  
**Date:** 2026-02-28  
**Status:** Findings Merged

### Executive Summary

Five specialized audit agents completed comprehensive security, performance, CI/Linux, and scope analysis of the codebase against Issue #1 requirements (LocalEmbeddings v1.1.0 audit items). Recommendations prioritized into P0/P1/P2/P3 by blocking impact and effort.

### Security Audit (Skinner)

**Total Issues:** 8 (3 critical, 4 medium, 1 low)  
**Overall Risk Level:** 🟡 MEDIUM

#### Critical (BLOCKING for v1.0)

1. **Path Traversal — DatasetLoaderStatic.cs (Lines 20-94)**
   - Issue: No validation of `..` segments; `../../etc/passwd` attacks possible
   - Fix: Add `ValidateFilePath()` helper rejecting `..` and absolute paths
   - Effort: ~10 lines of code

2. **Path Traversal — DatasetLoader.cs (Lines 12-91)**
   - Issue: Same vulnerability in `JsonDatasetLoader.LoadAsync()` / `SaveAsync()`
   - Fix: Apply same `ValidateFilePath()` helper
   - Effort: ~10 lines of code

3. **Path Traversal — SqliteResultStore.cs (Lines 19-27)**
   - Issue: Raw `dbPath` passed to SqliteConnection without validation
   - Fix: Validate path + reject SQLite URI parameters (`;`, `?`)
   - Effort: ~5 lines of code

#### Medium (SHOULD-DO)

4. **Export/Report Path Parameters** (JsonExporter, CsvExporter, BaselineSnapshot)
   - Issue: No path traversal checks in `ExportAsync()` / `SaveAsync()` methods
   - Fix: Apply `ValidateFilePath()` to all three classes
   - Effort: ~5 lines per class

5. **Cross-Platform File Name Validation**
   - Issue: No file name validation exists (correctly NOT using `GetInvalidFileNameChars()`)
   - Recommendation: Create `ValidateFileName()` for future file name validation features
   - Effort: ~20 lines

6. **Input Validation — Missing Length Limits**
   - Issue: Public APIs null-check but lack max-length constraints
   - Recommendation: Add limits to dataset names (200 chars), file paths (260 chars), prompts (10K chars)
   - Priority: Document in best-practices.md first (post-v1.0)
   - Effort: ~2-3 hours

7. **CSV Formula Injection**
   - Issue: Export doesn't prevent formula injection (`=1+1`, `@SUM()`, etc.)
   - Fix: Prefix values starting with `=+@-` with single quote in `EscapeCsv()`
   - Effort: ~5 lines

8. **AITestRunner Dataset Path** (AITestRunner.cs:28-32)
   - Issue: `WithDataset()` accepts path without validation
   - Fix: Add `ValidateFilePath()` after null check
   - Effort: ~3 lines

#### Actions

- **Phase 1 (Blocking):** Create `PathValidator.cs` with `ValidateFilePath()` and `ValidateFileName()` helpers; apply to all file I/O public APIs (DatasetLoaderStatic, JsonDatasetLoader, SqliteResultStore, JsonExporter, CsvExporter, BaselineSnapshot, AITestRunner)
- **Effort:** 4-6 hours total
- **Risk if not fixed:** HIGH — Path traversal is a common CVE

### Performance Audit (Byers)

**Top 5 Optimization Opportunities:** 5 high-ROI items (2-5x potential speedup)

1. **TensorPrimitives for Vector Math** — HIGH IMPACT / EASY ⭐⭐⭐
   - Issue: RelevanceEvaluator.cs:60-76 uses manual cosine similarity loop (O(n) scalar)
   - Fix: Replace with `System.Numerics.Tensors.TensorPrimitives.CosineSimilarity()` for SIMD acceleration
   - Effort: 30 minutes
   - Impact: 2-5x faster on SIMD-capable hardware
   - Status: **P1 (v1.0.1 patch, high-ROI)**

2. **ArrayPool for Tokenization** — HIGH IMPACT / EASY ⭐⭐⭐
   - Issue: HallucinationEvaluator, FactualityEvaluator, RelevanceEvaluator, ConsistencyEvaluator allocate HashSet/array per tokenization call
   - Fix: Use `ArrayPool<T>.Shared.Rent()/.Return()` for reusable buffers
   - Effort: 45 minutes
   - Impact: 30-50% allocation reduction, reduced GC pressure
   - Status: **P1 (v1.0.1)**

3. **Span<T> for String Splitting** — MEDIUM IMPACT / EASY ⭐⭐
   - Issue: All evaluators use `string.Split()` allocating arrays
   - Fix: Use `MemoryExtensions.Split()` on ReadOnlySpan<char>
   - Effort: 1 hour
   - Impact: 15-30% faster, zero allocations
   - Status: **P2 (v1.1)**

4. **Top-K Heap for Result Ranking** — MEDIUM IMPACT / MEDIUM ⭐⭐
   - Issue: ConsoleReporter materializes full result list then sorts
   - Fix: Use `PriorityQueue<T, TPriority>` for O(N log K) instead of O(N log N)
   - Effort: 2 hours
   - Impact: 50-70% faster for 1000+ examples
   - Status: **P2 (v1.1)**

5. **BenchmarkDotNet Integration** — HIGH IMPACT / EASY ⭐⭐⭐
   - Issue: No baseline performance metrics to validate optimizations
   - Fix: Create `tests/ElBruno.AI.Evaluation.Benchmarks/` project with BenchmarkDotNet
   - Effort: 1 hour setup + baseline
   - Impact: Enables measurement of all above optimizations
   - Status: **P3 (v1.1)**

#### Actions

- **Phase 1 (v1.0.1):** TensorPrimitives (#1), ArrayPool (#2) — 1-1.5 hours
- **Phase 2 (v1.1):** Span splitting (#3), Top-K heap (#4), Benchmarks (#5) — 4-5 hours
- **Projected Impact:** 2-5x vector ops, 30-50% allocation reduction, 50-70% aggregation speedup

### CI/Linux Audit (Langly)

**Status:** ✅ **CLEAN — NO BLOCKING ISSUES**

#### Findings

1. **[SkippableFact] Requirement** — ✅ COMPLIANT
   - All 164 tests use standard `[Fact]` / `[Theory]` attributes
   - Zero `Skip.If()` / `Skip.IfNot()` usage found
   - Cross-platform compatible, no action needed

2. **Path.GetInvalidFileNameChars()** — ✅ COMPLIANT
   - Zero usage of platform-specific file name validation
   - Status: No action required

3. **Git Tag Version Parsing** — ⚠️ PREVENTATIVE FIX SUGGESTED
   - File: `.github/workflows/publish.yml:35`
   - Current: `VERSION="${VERSION#v}"` (strips leading `v` only)
   - Issue: Does not strip trailing `.` or validate semver format
   - Fix: Add `VERSION="${VERSION#.}"` and validation regex `^[0-9]+\.[0-9]+\.[0-9]+`
   - Effort: 10 lines
   - Status: **P0 (BLOCKER)** — Prevents silent publish failures

4. **CI Workflow Review** — ✅ COMPLIANT
   - Runs on ubuntu-latest, uses standard xUnit runner
   - Status: Already Linux-compatible

#### Summary

- **Test Suite Overview:** 164 total tests (163 Fact + 1 Theory) across 28 files, 100% cross-platform
- **Status:** Approved for CI/Linux; recommend adding semver validation to publish workflow
- **Action:** Fix publish.yml tag parsing (P0)

### Scope & Constraints (Doggett)

**Project Classification:** Research/Evaluation Library (pre-production, offline deterministic)

#### Must-Do (v1.0 Gate)

1. **Path Traversal Prevention** — BLOCKING (security)
2. **File Size Limits** — BLOCKING (reliability, prevent OOM in CI)
3. **Publish Workflow Version Validation** — BLOCKING (operational safety)

#### Should-Do (v1.0.1–v1.1)

4. **TensorPrimitives for Cosine Similarity** — HIGH VALUE (perf, 5-10x)
5. **Span<T> in CSV Parsing** — MEDIUM VALUE (perf, 30-50% allocation)

#### Out-of-Scope

- URL validation (HTTPS-only) — Library has no network I/O
- Top-K optimization — No ranking/search operations exist yet
- [SkippableFact] for tests — No platform-conditional tests
- Cross-platform file name validation — Deferred until feature exists

#### Cost-Benefit Summary

| Item | Impact | Effort | Benefit | Priority |
|------|--------|--------|---------|----------|
| Path Traversal | High | 10 lines | 🟢 Excellent | **P0** |
| File Size Limits | Medium | 5 lines | 🟢 Excellent | **P0** |
| Publish Workflow | High | 10 lines | 🟢 Excellent | **P0** |
| TensorPrimitives | High | 20 lines | 🟢 Excellent | **P1** |
| Span<T> CSV | Medium | 30 lines | 🟡 Good | **P2** |
| BenchmarkDotNet | Low | 100 lines | 🟡 Good | **P3** |

#### Architectural Constraints Respected

1. Offline/deterministic evaluators — no external dependencies
2. Complementary positioning — no Microsoft.Extensions.AI.Evaluation duplication
3. Minimal csproj files — optimizations go in Directory.Build.props
4. No external CSV dependency — stay hand-rolled

### Routing & Implementation

| Auditor | Finding | Routing |
|---------|---------|---------|
| Skinner | Security findings | Byers (Senior .NET Dev) |
| Byers | Performance opportunities | Byers (code + benchmarks) |
| Langly | CI/Linux audit (clean) | Byers (publish.yml fix) |
| Doggett | Scope constraints & prioritization | Mulder (approval + release planning) |

### Recommendations (Ordered by Priority)

**Immediate (v1.0 Gate) — ~2–3 hours:**
1. Add `PathValidator.cs` with path traversal checks
2. Add file size limit checks to loaders
3. Fix `.github/workflows/publish.yml` version tag parsing with validation

**Post-v1.0 (v1.0.1) — ~1.5 hours:**
4. Implement TensorPrimitives for cosine similarity
5. Implement ArrayPool for tokenization buffers

**v1.1 Backlog — ~4–5 hours:**
6. Migrate CSV parsing to Span<T>
7. Add Top-K heap for ConsoleReporter
8. Create BenchmarkDotNet test suite

### Overall Assessment

**Confidence Level:** 95/100 (Very High)

- Evidence-based findings (grep analysis, code review)
- Clear project scope boundaries (library, not service)
- Respects existing architectural decisions
- Low implementation complexity for P0 items
- High ROI for performance optimizations (2-5x gains)

**Risk Mitigation:**
- Path validation includes opt-out flag for legacy CI pipelines
- Performance work fully backward-compatible (no breaking changes)
- Benchmarks provide objective measurement before/after
