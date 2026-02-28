# History

## Project Context
- **Project:** Strategic Research Initiative — AI + .NET Opportunities
- **User:** Bruno Capuano (Developer Advocate, Microsoft Cloud & GitHub Technologies)
- **Goal:** Identify high-impact opportunities at AI + .NET intersection → NuGet packages, OSS samples, complementary libraries
- **Stack:** Research & analysis focused on .NET ecosystem, Microsoft Azure, GitHub Copilot, Microsoft Foundry

## Learnings

### .NET AI Framework Landscape (January 2025)

**Key Technical Findings:**

1. **Framework Maturity Assessment:**
   - Semantic Kernel: Strong orchestration but heavy abstraction overhead; production-ready but complex debugging
   - ML.NET: Excellent for classical ML; weak for deep learning and modern architectures
   - Microsoft.Extensions.AI (.NET 9+): Best provider-agnostic abstraction; new but promising
   - ONNX Runtime: Solid inference; limited AOT support and dynamic shape handling
   - LangChain.NET: Feature-rich but unstable APIs; lags Python implementation
   - AutoGen.NET: Powerful multi-agent but complex and costly; better for research than production

2. **Critical Architectural Gaps (NO adequate .NET solution):**
   - **AI Testing & Evaluation Framework:** Microsoft.Extensions.AI.Evaluation is basic; lacks golden dataset management, regression testing, hallucination detection, visual diff tools, prompt A/B testing
   - **Production-Grade Prompt Management:** DotPrompt/Prompt-Engine are minimal; need version control, semantic diff, A/B testing, performance tracking, collaboration features, environment promotion
   - **Local-First Agent Debugging:** Most tools require Azure (Foundry, App Insights); need IDE-integrated debugging with reasoning chain visualization, token tracking, workflow replay
   - **Hybrid Architecture Patterns:** No standardized patterns for local-to-cloud fallback, cost-optimized routing, offline-first applications, model lifecycle management

3. **Developer Pain Points:**
   - Difficult to test and evaluate LLM outputs (no standardized patterns)
   - Prompts treated as strings, not versioned assets
   - Azure-heavy tooling excludes OSS projects and local development
   - Limited guidance on production architecture beyond demos
   - Fragmented ecosystem with unclear framework selection criteria
   - Weak experiment tracking and model lifecycle management vs. Python (MLflow, DVC)

4. **High-Impact Opportunities for Small Team:**
   - **PRIORITY 1:** AI Testing & Evaluation Framework (HIGH impact, HIGH feasibility, LOW competition)
   - **PRIORITY 2:** Prompt Management System (HIGH impact, HIGH feasibility, LOW competition)
   - **PRIORITY 3:** Local Agent Debugging Toolkit (MEDIUM-HIGH impact, MEDIUM feasibility, LOW competition)

5. **Technical Stack for Recommended Tools:**
   - Build on Microsoft.Extensions.AI.Evaluation (extend, don't replace)
   - Target xUnit/NUnit/MSTest integration
   - Blazor for visualization dashboards
   - Git-backed storage for version control
   - ASP.NET Core APIs for programmatic access
   - SQLite or file-based storage for simplicity

6. **Differentiation Strategy:**
   - Native .NET integration (no Python interop required)
   - First-class IDE integration (Visual Studio, Rider)
   - Type-safe, compiled evaluation logic
   - Self-hosted option (no SaaS lock-in)
   - Git-native workflow
   - Cross-cloud (not Azure-only)
   - Opinionated, focused scope vs. Microsoft's general-purpose tools

**Feasibility Assessment:**
Small team (2-4 developers) can deliver MVP of AI testing framework + prompt management in 3-6 months. Focus areas are well-defined, complementary to Microsoft ecosystem, and address genuine pain points. Risk: Microsoft may build similar tools, but OSS community velocity and focused DX can differentiate.

**Next Steps:**
- Validate findings with .NET AI developer community
- Prototype core features of testing framework
- Define API surface for prompt management
- Assess integration points with Semantic Kernel and Microsoft.Extensions.AI

---

## Security Audit — ElBruno.AI.Evaluation (February 2026)

**Context:** GitHub Issue #1 requested a security audit based on LocalEmbeddings concerns, focusing on: path traversal, file name validation, input validation, URL validation, and file integrity checks.

**Audit Scope:** All file I/O operations, public API entry points, and path/URL handling across 4 projects:
- `ElBruno.AI.Evaluation`
- `ElBruno.AI.Evaluation.SyntheticData`
- `ElBruno.AI.Evaluation.Reporting`
- `ElBruno.AI.Evaluation.Xunit`

### Key Findings

**Critical Vulnerabilities (🔴):**
1. **Path Traversal — Universal Across Codebase**
   - **Files:** DatasetLoaderStatic.cs (4 methods), DatasetLoader.cs (3 methods), SqliteResultStore.cs (1 method)
   - **Issue:** Zero validation of file paths; accepts `../../../sensitive.json` or absolute paths without sanitization
   - **Attack Surface:** All public APIs accepting `string path`, `string filePath`, `string outputPath`, `string dbPath`
   - **Code Pattern:** Direct pass-through to `File.OpenRead()`, `File.Create()`, `Directory.CreateDirectory()` without validation
   - **Risk:** High — users could read/write arbitrary files on the filesystem

2. **SQLite Injection Risk**
   - **File:** SqliteResultStore.cs line 25
   - **Issue:** Accepts raw `dbPath` and constructs connection string without validation; could inject SQLite URI parameters
   - **Example Attack:** `dbPath = "test.db;Mode=Memory"` or `"file::memory:?cache=shared"`

**Medium Risks (🟡):**
3. **Cross-Platform File Name Validation — Missing Entirely**
   - No validation of invalid characters (`<>:|*?"`) in file names
   - Audit correctly noted: DO NOT use `Path.GetInvalidFileNameChars()` (platform-specific)
   - Need manual cross-platform character allowlist

4. **Input Validation — No Length Limits**
   - Null checks ✅ present everywhere (good!)
   - Length limits ❌ missing on dataset names, paths, prompts, domains
   - DoS risk: 10MB dataset name accepted without error

5. **CSV Formula Injection**
   - CSV export correctly escapes commas/quotes but does NOT prevent `=1+1` or `@SUM()` formula injection
   - Risk: Users opening exported CSV in Excel could execute arbitrary formulas

6. **Export Path Validation — JsonExporter, CsvExporter, BaselineSnapshot**
   - Same path traversal issue as dataset loaders

**Low Risk (🟢):**
7. **File Integrity Checks — No Pre-Parsing Size/Format Validation**
   - Could attempt to load 10GB JSON file into memory
   - No magic byte validation (blindly trusts file extensions)
   - Mitigated by: .NET's JsonSerializer has built-in safeguards

**Not Applicable:**
- **URL Validation:** No HTTP/URL operations found in codebase (no HttpClient, no downloads, no remote model fetching)
- **Model Loading:** No embedding or binary weight file operations (this is an evaluation toolkit, not an embedding library)

### Code Patterns Observed

**✅ Security Strengths:**
- Consistent null validation (`ArgumentNullException.ThrowIfNull()`)
- Proper async disposal (`await using`)
- CSV escaping for commas/quotes/newlines
- No SQL injection risk (uses parameterized queries in SqliteResultStore)

**❌ Security Gaps:**
- Zero path validation anywhere in codebase
- No defense-in-depth for file I/O operations
- No documentation of security considerations
- No input length limits

### Recommended Remediation

**Phase 1 (Blocking for v1.0 — 4-6 hours):**
1. Create `PathValidator.cs` helper class with:
   - `ValidateFilePath(string path, bool allowAbsolutePaths = false)` — reject `..` segments, normalize paths, optionally reject absolute paths
   - `ValidateFileName(string fileName)` — cross-platform character allowlist, length check
2. Apply validation to ALL 13+ public API entry points across all projects
3. Add unit tests for path traversal attack vectors

**Phase 2 (Post-v1.0 — 2-3 hours):**
- Add string length limits (dataset names: 200 chars, paths: 260 chars, prompts: 10K chars)
- CSV formula injection protection (prefix `=+@-` with single quote)
- File size limits (100MB default for JSON/CSV)
- Security documentation in XML comments

### Technical Implications

**Why This Audit Matters:**
- File I/O is the **largest public API surface** in this library (dataset loading, export, SQLite persistence)
- Target users are developers testing AI applications — likely running in CI/CD pipelines, local dev environments, possibly in sandboxed containers
- Path traversal vulnerabilities are **CWE-22** (Common Weakness Enumeration) — one of OWASP Top 10 and a common CVE source

**False Positives Avoided:**
- LocalEmbeddings audit referenced "model loading" and "binary validation" — **NOT APPLICABLE** to ElBruno.AI.Evaluation (this is a testing/evaluation toolkit, not an embedding library)
- URL validation — **NOT APPLICABLE** (no HTTP operations in codebase)

**Severity Justification:**
- Downgraded from CRITICAL to MEDIUM overall because:
  - No remote exploitability (requires local filesystem access)
  - Typical usage is developer-controlled paths (test code, samples)
  - No PII or credential exposure risk (operates on synthetic/test data)
- Still **MUST FIX before v1.0** because:
  - Public NuGet package with broad distribution
  - Users may integrate into production pipelines
  - Path traversal is a well-known attack vector

### Action Items for Team

- **Byers (Developer):** Implement PathValidator and apply across codebase
- **Doggett (QA):** Create security test suite (path traversal, CSV injection, length limits)
- **Mulder (Lead):** Review and approve security fixes
- **Scribe:** Document security best practices in `docs/security.md`

---

**Date:** 2026-02-24  
**Status:** Audit complete, findings documented in `.squad/decisions/inbox/skinner-security-audit.md`

## File Integrity Implementation (February 2026)

**Context:** Implemented file integrity checks (Issue #1, Phase 1) as blocking security enhancement for v1.0 release.

**Implementation Details:**

1. **Created FileIntegrityValidator.cs**
   - Location: src/ElBruno.AI.Evaluation/Security/FileIntegrityValidator.cs
   - Public static class with three validation methods:
     - ValidateJsonFile(string path) — 100MB limit + JSON format validation
     - ValidateCsvFile(string path) — 100MB limit + UTF-8 encoding validation
     - ValidateDatabaseFile(string path) — 1GB limit + SQLite magic header validation

2. **File Size Limits Rationale:**
   - JSON/CSV: 100MB (reasonable for golden datasets; prevents loading 10GB files into memory)
   - SQLite: 1GB (evaluation results can accumulate; higher limit appropriate)
   - All limits configurable via constants if needed in future

3. **Format Validation Approach:**
   - **JSON:** Checks for { or [ after skipping UTF-8 BOM and whitespace; detects UTF-16 (rejects with clear error)
   - **CSV:** Validates UTF-8 encoding of first 512 bytes using Encoding.UTF8.GetString()
   - **SQLite:** Validates 16-byte magic header "SQLite format 3\0"

4. **Integration Points (5 files modified):**
   - DatasetLoaderStatic.cs → LoadFromJsonAsync(), LoadFromCsvAsync()
   - DatasetLoader.cs → JsonDatasetLoader.LoadAsync(), LoadFromCsvAsync()
   - SqliteResultStore.cs → CreateAsync()
   - BaselineSnapshot.cs → LoadAsync()

5. **Design Decisions:**
   - **Early return for missing files:** If file doesn't exist, return immediately to let underlying I/O methods throw proper FileNotFoundException (preserves existing error semantics)
   - **No path validation in FileIntegrityValidator:** Path validation is separate concern handled by PathValidator.cs (already implemented by Byers); FileIntegrityValidator focuses purely on size/format
   - **InvalidOperationException:** Used for integrity failures to distinguish from ArgumentException (path issues) and FileNotFoundException (missing files)

6. **Error Messages:**
   - Clear, actionable messages with file size in bytes and limit in MB
   - Examples:
     - "JSON file exceeds maximum size limit. File size: 104,857,600 bytes, Limit: 100 MB"
     - "File does not appear to be valid JSON format. Expected '{' or '[' at start."
     - "File does not appear to be a valid SQLite database. Invalid magic header."

7. **Testing Results:**
   - ✅ Build: Successful (no compilation errors)
   - ⚠️ Tests: 8 failures (unrelated to file integrity — PathValidator rejecting absolute paths in Save methods)
   - File integrity validation working correctly on all Load operations

8. **Known Limitations:**
   - Format validation is basic (magic bytes only); doesn't fully parse files
   - UTF-8 validation only checks first 512 bytes (performance optimization)
   - SQLite validation only checks header (doesn't validate schema integrity)

9. **Future Enhancements (Post-v1.0):**
   - Make size limits configurable via options pattern
   - Add JSON schema validation for golden datasets
   - Add CSV column count/structure validation
   - Consider streaming validation for large files

**Git Commit:** 4f87e91 — "Security: Add file integrity checks before loading"

**Status:** ✅ Complete — File integrity checks implemented and committed

---

**Date:** 2026-02-24  
**Implemented by:** Skinner (Technical Strategist)

