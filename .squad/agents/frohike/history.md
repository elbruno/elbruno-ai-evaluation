# History

## Project Context
- **Project:** ElBruno.AI.Evaluation — AI Testing & Observability Toolkit for .NET
- **User:** Bruno Capuano (Developer Advocate, Microsoft Cloud & GitHub Technologies)
- **Goal:** Build production-grade NuGet packages for AI testing in .NET
- **Stack:** .NET 8+, C#, xUnit, Microsoft.Extensions.AI, SQLite
- **Packages:** ElBruno.AI.Evaluation, ElBruno.AI.Evaluation.Xunit, ElBruno.AI.Evaluation.Reporting

## Work Completed

### Blog Post Series (Session 2025-02-23)
Created 5 publishable blog posts in `blog/` directory:

1. **01-introducing-elbruno-ai-evaluation.md** (5,714 chars)
   - Gap analysis: Why AI testing matters for .NET developers
   - Overview of all 3 packages (Core, Xunit, Reporting)
   - Quick 5-minute demo (install → dataset → pipeline → results)
   - Comparison table vs. Python ecosystem (Ragas, DeepEval)
   - Series roadmap

2. **02-golden-datasets-for-ai-testing.md** (8,707 chars)
   - What golden datasets are and why they matter
   - Fluent API for building datasets programmatically
   - Tagging and filtering with GetByTag() and GetSubset()
   - JSON format specification with real examples
   - CSV bulk import feature
   - Versioning strategy and best practices
   - Dataset statistics and summary

3. **03-ai-evaluators-deep-dive.md** (10,694 chars)
   - All 5 evaluators with real code examples and output
   - RelevanceEvaluator: cosine similarity, term overlap
   - FactualityEvaluator: claim verification vs. reference
   - CoherenceEvaluator: sentence completeness, contradictions
   - HallucinationEvaluator: token grounding
   - SafetyEvaluator: PII, profanity, harmful patterns
   - Combining multiple evaluators for comprehensive testing
   - Custom evaluator example (LengthEvaluator)
   - Threshold selection guide

4. **04-ai-testing-with-xunit.md** (11,349 chars)
   - AIEvaluationTest attribute usage
   - AIAssert fluent assertions (PassesThreshold, AllMetricsPass)
   - Test patterns: single evaluator, golden datasets, tagged subsets
   - CI/CD integration (GitHub Actions, Azure Pipelines)
   - Test project organization
   - Debugging failed evaluations
   - Visual Studio Test Explorer integration

5. **05-from-demo-to-production.md** (12,732 chars)
   - SQLite result persistence and querying
   - BaselineSnapshot for regression detection
   - Export formats: Console, JSON, CSV
   - Cost and token tracking
   - Enterprise pattern: baseline + regression detection
   - Monitoring dashboard concepts
   - Deployment gate via CI/CD
   - 7-phase long-term strategy

### Documentation Suite (Session 2025-02-24)
Created 4 comprehensive developer-facing guides in `docs/` directory:

1. **quickstart.md** (5.6 KB)
   - Installation and first 5-minute setup
   - Golden dataset JSON creation example
   - Full console app demo with explanations
   - Key concepts glossary
   - Links to deeper documentation

2. **evaluation-metrics.md** (14.3 KB)
   - Complete reference for all 5 evaluators
   - HallucinationEvaluator: keyword overlap, grounding verification
   - FactualityEvaluator: claim-support matching (50% threshold)
   - RelevanceEvaluator: cosine similarity on term frequencies
   - CoherenceEvaluator: sentence completeness, contradictions, repetition penalties
   - SafetyEvaluator: blocklist + regex PII detection (email, SSN, phone)
   - Custom evaluator creation template
   - Threshold tables (conservative/balanced/lenient)
   - Combining evaluators for comprehensive coverage

3. **golden-datasets.md** (13 KB)
   - Comprehensive dataset management guide
   - JSON schema specification with all field types
   - CSV import (Input, ExpectedOutput, Context, Tags)
   - Programmatic creation and operations
   - Tag filtering and subsetting patterns
   - Semantic versioning (MAJOR/MINOR/PATCH)
   - 8 best practices: balance, realistic outputs, RAG context, consistent tagging
   - Multi-dataset organization for complex systems
   - Troubleshooting common issues

4. **best-practices.md** (19.9 KB)
   - Use-case-specific evaluator matrices (support, RAG, content gen, code gen)
   - Threshold setting methodology (business impact, capability testing, false-positive cost)
   - Regression testing workflow with 3-step implementation
   - CI/CD integration patterns (PR testing, canary deploy, daily health checks)
   - 6 common pitfalls: dataset bias, cargo cult thresholds, missing edge cases, single evaluator, stale baselines, false-positive costs
   - xUnit testing patterns (fixtures, dataset-driven tests)
   - Monitoring metrics and alert thresholds
   - Continuous improvement cycle (collect failures → analyze → improve → test → update baseline)

## Design Approach

All documentation:
- **Code-first** — Every concept has working C# examples
- **Real-world** — Patterns match actual production use cases
- **Progressive** — Quickstart → Metrics → Datasets → Best Practices (increases depth)
- **Actionable** — Specific thresholds, decision matrices, checklists
- **API-faithful** — Examples match actual implementation exactly

### Root README.md (Session 2025-02-24)
Created comprehensive root README.md for the ElBruno.AI.Evaluation repository:

**Structure:**
- Title with build, .NET, license badges
- One-liner: "AI Testing & Observability Toolkit for .NET — the xUnit for AI applications"
- Why section: 2-3 sentences on the .NET testing gap
- Quick Start: Install instructions + 10-line evaluation pipeline example
- Packages table: Core, Xunit, Reporting with descriptions
- Features: Bullet list covering 5 evaluators, golden datasets, regression testing, persistence, xUnit integration
- Documentation: Links to docs/ (quickstart, evaluation-metrics, golden-datasets, best-practices)
- Samples: Links to ChatbotEvaluation and RagEvaluation projects
- Blog Series: Links to all 5 blog posts with titles
- Roadmap: v1.0 (current), v1.5 (Copilot integration), v2.0 (visual debugging)
- Contributing + License (MIT) sections

**Key elements:**
- Professional OSS project presentation
- Code example derived directly from samples/ChatbotEvaluation/Program.cs
- API references grounded in actual source code (IEvaluator, ChatClientExtensions, GoldenDataset)
- Progressive learning path: Quick Start → Docs → Blog → Samples

## Learnings

### ReportingShowcase Sample (Current Session)
Created a new production-ready sample demonstrating the complete reporting workflow:

**Files Created:**
- `samples/ReportingShowcase/ReportingShowcase.csproj` — Project file referencing ElBruno.AI.Evaluation and ElBruno.AI.Evaluation.Reporting
- `samples/ReportingShowcase/Program.cs` — Comprehensive demonstration:
  - Simulates evaluation runs with RelevanceEvaluator and HallucinationEvaluator
  - Stores results in SQLite using SqliteResultStore (query by dataset name)
  - Exports to JSON via JsonExporter (preserves full metadata and metrics)
  - Exports to CSV via CsvExporter (tabular format with one row per example + metric columns)
  - Prints console summary via ConsoleReporter (verbose mode shows per-evaluator breakdown and failures)
- `samples/ReportingShowcase/README.md` — Developer documentation covering:
  - What the sample demonstrates (SQLite persistence + multi-format export)
  - Running instructions and expected output files
  - Code highlights for each reporting API
  - Real-world use cases (CI/CD regression detection, monitoring dashboards, compliance auditing, team communication)

**Files Modified:**
- `README.md` — Added ReportingShowcase entry to Samples section (alphabetically ordered)

**Verification:**
- ✅ ReportingShowcase project builds successfully (0 errors, 0 warnings)
- ✅ All APIs used match actual public signatures from source code
- ✅ Example data is realistic (customer support Q&A scenarios)
