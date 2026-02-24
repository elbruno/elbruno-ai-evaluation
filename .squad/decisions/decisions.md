# Strategic Decisions — AI + .NET Initiative

## Current Status
**Phase:** 3 (Validation Complete)  
**Last Updated:** 2026-02-23T2355  
**Repository:** D:\labs\netai-nextwin

---

## PRIMARY DECISION: AI Testing & Observability Toolkit for .NET

**Status:** ✅ APPROVED (Phase 3 Complete)  
**Decision Makers:** Mulder (Synthesis), Doggett (Validation), Reyes (Advocacy)  
**Confidence:** 92-94/100  
**Date:** January 2025  
**Impact:** Primary strategic direction for Bruno Capuano's AI + .NET initiative

### Decision
Build a comprehensive **AI Testing & Observability Toolkit for .NET** (`NetAI.Testing`) as the primary focus for this strategic initiative.

### Rationale: Convergent Evidence

All three independent analysts identified **AI testing/evaluation as the #1 gap** in .NET AI ecosystem:

- **Scully (Community Analyst):** "Very High" signal strength; developers ask "How do I test AI outputs?" across Stack Overflow, GitHub, Reddit
- **Krycek (Trend Analyst):** "VERY HIGH" opportunity; Python has Ragas/DeepEval/TruLens, .NET has nothing equivalent
- **Skinner (Technical Strategist):** True architectural gap; Microsoft.Extensions.AI.Evaluation too basic; lacks regression testing, golden datasets, hallucination detection

**This is the ONLY opportunity identified by all three analysts with maximum severity.**

### Competitive Positioning

**Scoring Summary (out of 100):**
1. AI Testing & Observability: **95**
2. Prompt Management: **88**
3. RAG Patterns Library: **85**
4. Structured Output Toolkit: **82**
5. Beginner Learning Path: **78**

**Key Differentiators:**
- **Universal Need:** Every production AI app requires testing; not niche
- **Enterprise Blocker:** Cannot ship AI without quality assurance; compliance requirement
- **Technical Feasibility:** Well-defined scope, achievable in 3-4 months by small team
- **No Adequate Solution:** Python has mature tools, .NET has nothing comparable
- **Category Creation:** This is "xUnit for AI applications" — foundational infrastructure

### Constraint Compliance

**All hard constraints MET:**
- ✅ NOT generic wrapper (solves quality assurance, not API access)
- ✅ NOT replicating major frameworks (no .NET equivalent to Ragas/DeepEval/TruLens exists)
- ✅ Viable as open-source (SQLite backend, no infrastructure required)
- ✅ No massive infrastructure needed (NuGet package library)

### Evidence Quality

**Signal Strength: 95/100**

- **Cross-validated:** All three analysts independently identified AI testing gap
- **Primary sources:** Stack Overflow recurring questions, GitHub issues, Reddit patterns, conference Q&A
- **Demand-driven:** 12+ months of consistent signals, developers building workarounds, enterprise blockers documented
- **NOT hype:** Authentic pain points, not speculative trend-chasing

### Technical Feasibility

**REALISTIC WITH DISCIPLINE**

- 3-4 month MVP timeline achievable for Bruno's expertise level
- Core dependencies stable (Microsoft.Extensions.AI GA, Semantic Kernel v1+)
- Hidden complexity manageable (hallucination detection via extensibility SDK)
- **Critical risk:** Scope creep → Mitigation: Strict MVP feature freeze

### Strategic Alignment

**Bruno's Unique Positioning:**
- Triple expertise (AI + .NET + GitHub) perfectly aligned
- Developer advocacy mission fits educational nature of testing toolkit
- Microsoft ecosystem authority enables adoption in .NET community
- Azure + Foundry knowledge enables premium integration
- OSS track record demonstrates capability

**Microsoft Foundry Integration:**
- Natural backend for observability dashboards (OpenTelemetry-based tracing)
- DevUI visualization for reasoning chain inspection
- Evaluation run storage for historical tracking and drift detection
- Differentiates from Python tools (Azure-native advantage)

**GitHub Copilot Integration:**
- IDE Assistant generates tests from prompts (productivity multiplier)
- Copilot SDK extension for AI testing patterns
- Copilot CLI shortcuts for test generation/review
- Showcases Copilot's value in AI workflows

### Community Validation

**Recurring Questions (Evidence of Pain):**
- Stack Overflow: "How do I test my LLM outputs in .NET?" (asked monthly)
- Reddit: "What tools do you use for AI testing in .NET?" (no canonical answer)
- GitHub: Requests for evaluation examples across Semantic Kernel, Microsoft.Extensions.AI

**Workarounds Developers Use Today:**
- Manual review of samples (unscalable)
- Python scripts called via Process (slow, friction)
- Custom "golden output" comparison (brittle)
- Application Insights custom events (no semantic understanding)
- None/YOLO deployment (production incidents)

**Pattern:** Developers WANT to test rigorously but lack tooling. They're inventing solutions because none exist.

### Differentiation from Common Pitfalls

**NOT "Just Another AI Wrapper":**
- Solves quality assurance, not vendor integration
- Builds on Microsoft.Extensions.AI (abstraction already exists)
- Opinionated best practices, not just API access

**NOT "Just Another Demo Sample":**
- Solves production need (testing is mandatory for real apps)
- Enterprise-grade quality (scale, security, compliance)
- LTS commitment, backward compatibility

**NOT "Just Another Experimental Repo":**
- SemVer commitment (v1.0 = stable API)
- .NET LTS alignment, comprehensive testing
- Community governance, open roadmap

**Positioning:** "NetAI.Testing is the xUnit for AI applications — bringing the same rigor and DX .NET developers expect to LLMs."

### Adoption & Viral Potential

**Why This Spreads:**
- Universal pain point → natural word-of-mouth
- Immediate ROI → first test catches first bug
- Enterprise demand → compliance mandates adoption
- GitHub Copilot integration → effortless test authoring
- Stack Overflow presence → canonical answer to recurring questions
- Microsoft ecosystem → cross-promotion with Azure AI

### Advocacy Alignment Score: 94/100 (A+)

| Dimension | Score | Grade |
|-----------|-------|-------|
| Microsoft Foundry Integration | 19/20 | A+ |
| GitHub Copilot Integration | 20/20 | A+ |
| Cloud Advocate Positioning | 20/20 | A+ |
| Conference-Talkability | 20/20 | A+ |
| Blog-Postability | 19/20 | A+ |
| Community Engagement | 19/20 | A+ |
| DevRel Distribution Fit | 20/20 | A+ |
| Educational Value | 20/20 | A+ |
| Strategic Framing | 20/20 | A+ |

**Overall: 177/180 (98.3%)**

### Key Advocacy Strengths

1. **Organic Microsoft Foundry Integration:** Dual-model approach (SQLite for OSS, Foundry for enterprises) demonstrates value without vendor lock-in. Developers adopt Foundry because it makes their testing better, not because Microsoft says to.

2. **Exceptional GitHub Copilot Showcase:** Integrates all three surfaces (IDE, SDK, CLI) with innovative, productivity-focused use cases. This is SDK/CLI showcase material for conferences.

3. **Perfect Cloud Advocate Positioning:** Establishes Bruno as "the .NET AI testing authority" through category creation. This is authentically Bruno's project — his name will be synonymous with AI testing in .NET.

4. **Conference-Ready Content:** Multiple compelling talk concepts, workshop-ready, keynote-worthy material. Universal pain point ensures strong attendance.

5. **Sustained Content Pipeline:** 14+ blog posts spanning beginner to expert, providing 6-12 months of flagship content from single initiative.

6. **Strong Community Engagement:** Universal pain point drives natural word-of-mouth. Stack Overflow canonical answer potential. High contribution opportunities.

7. **DevRel Distribution Excellence:** Leverages all key channels (NuGet, GitHub, Microsoft Docs, Azure Marketplace) with clear phased rollout strategy.

8. **Educational Impact:** Elevates entire .NET AI ecosystem maturity from "demos work" to "production ready." Teaches fundamental patterns with lasting value.

9. **Strategic Framing Success:** Decisively passes "inevitable in hindsight" test. This feels like the "xUnit moment" for AI applications.

### Success Metrics (12 Months)

- 50K+ NuGet downloads
- 1.5K+ GitHub stars
- 3+ enterprise case studies
- Microsoft blog feature
- Conference talks at .NET Conf, NDC
- Established as de facto standard for AI quality assurance in .NET

### MVP Scope (3-4 Months)

**NuGet Package: `NetAI.Testing`**

**Core Features:**
1. Golden dataset management (import, version, diff)
2. Test framework integration (xUnit/NUnit/MSTest)
3. Evaluation metrics (hallucination, factuality, safety, regression)
4. CI/CD integration (GitHub Actions, Azure DevOps)
5. Basic observability (test run history, cost/token tracking)

**Exclusions (for v1.0):**
- Visual debugging UI (v2.0)
- A/B testing infrastructure (v2.0)
- Prompt management (separate package)
- Multi-agent testing (v2.0)

### Roadmap

**v1.0 (Months 1-4):** Foundation — golden datasets, test integration, core metrics, CI/CD  
**v2.0 (Months 5-8):** Visual debugging UI, A/B testing, Foundry integration, IDE extensions  
**v3.0 (Months 9-12):** Multi-agent testing, production monitoring, advanced metrics  
**Year 2+:** Ecosystem expansion — complementary packages, community contributions, enterprise services

### Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Microsoft builds similar tool | Speed to market; community focus; complementary positioning |
| Limited .NET AI adoption | Enterprise focus (production need regardless of market size) |
| Framework evolution breaks integration | Abstraction layer; provider pattern; version support |
| Evaluation metrics unreliable | Transparent limitations; human-in-loop; ensemble metrics |
| Copilot CLI integration depth clarity | Needs workflow detail (IDE vs. CLI use case clarity) |
| Foundry adoption prerequisite risk | Strengthen standalone value proposition (mitigated by dual-model) |
| Microsoft partner relationship | Clarify governance model (independent OSS vs. Microsoft-incubated) |

---

## SECONDARY DECISION: Prompt Management Framework (Deferred)

**Status:** ✅ APPROVED (Strategic - Defer to v2.0)  
**Decision Makers:** All analysts  
**Confidence:** 88/100  
**Positioning:** Natural follow-on after NetAI.Testing v1.0 establishes foundation

### Rationale

**Why NOT chosen as primary:**
- Ranked #2 (88/100) but lower signal strength (2/3 analysts vs. 3/3 for testing)
- Complementary to testing (natural follow-on package)
- Testing is MORE foundational (prerequisite for quality)
- DotPrompt exists (basic solution); testing has NO adequate solution

### Future Scope

**Package: `NetAI.Prompts`**

**Core Features:**
- Git-backed version control with semantic diff
- A/B testing infrastructure
- Performance tracking and environment promotion
- Collaboration UI
- Self-hosted, OSS-first, cross-cloud compatible

**Timeline:** Post NetAI.Testing v1.0 (Months 5-8)

---

## RESEARCH FINDINGS: Focus Areas Identified

**Author:** Scully (Community Analyst)  
**Status:** Integrated into Primary Decision  
**Confidence:** Evidence-based, cross-validated

### Recommended Focus Areas

1. **Production-Ready Components** (vs more demos)
   - RAG toolkit with chunking, ranking, monitoring
   - Prompt management library
   - LLM evaluation extensions
   - **Rationale:** Demo-to-production gap is the primary enterprise pain point

2. **Observability/Testing** (cross-cutting concern)
   - Every research area showed observability gaps
   - Critical for enterprise adoption
   - **Rationale:** Can't debug what you can't see
   - **SELECTED AS PRIMARY DIRECTION**

3. **Framework Stability Guidance**
   - Semantic Kernel + AutoGen convergence creating migration anxiety
   - Version compatibility issues widespread
   - **Rationale:** Adoption blocker for risk-averse enterprises

4. **Beginner Onboarding**
   - Opinionated learning paths
   - Real-world project-based learning
   - **Rationale:** Massive audience, minimal quality resources

---

## TECHNICAL FOUNDATION: Framework Landscape

**Author:** Skinner (Technical Strategist)  
**Status:** Integrated into Primary Decision  
**Confidence:** High

### .NET AI Framework Analysis

**Frameworks Reviewed:**
- Semantic Kernel
- ML.NET
- Microsoft.Extensions.AI
- ONNX Runtime
- LangChain.NET
- AutoGen.NET

**Cross-Cutting Gaps Identified:**
- Evaluation and testing
- Structured output handling
- Prompt management
- Observability and monitoring
- Local model deployment
- Application templates
- Model-agnostic abstractions

### Build Foundation

**Technical Approach for NetAI.Testing:**
- Build on Microsoft.Extensions.AI abstractions
- Modular architecture (framework-agnostic where possible)
- Git-native workflows
- Blazor for visualization/UI components
- ASP.NET Core for APIs
- SQLite or file-based storage for simplicity

**Integration Points:**
- Semantic Kernel plugins for evaluation
- Microsoft.Extensions.AI.Evaluation as foundation
- CI/CD integration (GitHub Actions, Azure DevOps)
- Existing test frameworks (xUnit, NUnit, MSTest)

---

## MARKET & TREND ANALYSIS

**Author:** Krycek (Trend Analyst)  
**Status:** Integrated into Primary Decision  
**Confidence:** VERY HIGH

### Strategic Positioning

**Market Analysis:**
- Python dominates AI research/prototyping (won't change)
- Production deployment is where .NET excels (performance, enterprise, Azure)
- Most AI tools are Python-first, creating .NET gaps
- Enterprise adoption requires production-grade tooling (testing, observability, security)

### .NET Competitive Advantages

- Azure tight integration (AI Search, Monitor, Key Vault, API Management)
- Type safety reduces production errors
- Superior performance/scalability
- Mature security/compliance frameworks
- Enterprise penetration in regulated industries

### Tier 1 Opportunities (Immediate)

1. **LLM Testing Framework** ← **PRIMARY DIRECTION**
   - DeepEval-style testing with xUnit/NUnit integration
   - Synthetic data generation
   - LLM-as-Judge evaluation
   - **STATUS:** Approved for execution

2. Structured Output Library
   - Schema-enforced LLM outputs with JSON Schema + System.Text.Json integration

3. Azure Monitor LLM Integration
   - Native observability for LLM applications in Application Insights

4. Ollama.NET Client
   - Native client for local model deployment

---

## VALIDATION ASSESSMENT

**Validator:** Doggett (Reality-Check Specialist)  
**Date:** January 2025  
**Confidence:** 92/100  
**Status:** ✅ APPROVED

### Validation Results

**Constraint Compliance:** ✅ ALL MET
- NOT generic wrapper (solves quality assurance, not API access)
- NOT replicating major frameworks (no .NET equivalent to Ragas/DeepEval/TruLens exists)
- Viable as open-source (SQLite backend, no infrastructure required)
- No massive infrastructure needed (NuGet package library)

**Evidence Quality:** 95/100 Signal Strength
- Cross-validated by all three analysts
- Primary sources: Stack Overflow, GitHub issues, Reddit patterns
- Demand-driven: 12+ months of consistent signals
- Authentic pain points, not speculative trend-chasing

**Technical Feasibility:** REALISTIC WITH DISCIPLINE
- 3-4 month MVP achievable
- Core dependencies stable
- Manageable complexity
- Scope creep mitigation: Strict MVP feature freeze

**Differentiation:** GENUINE .NET-NATIVE GAP
- Python ecosystem has solutions; .NET has only basic Microsoft.Extensions.AI.Evaluation
- .NET-native advantages: type safety, IDE integration, Azure ecosystem, performance
- Complementary (not competitive) with Microsoft's official tooling
- First-mover opportunity in .NET AI testing space

**Strategic Fit:** PERFECT ALIGNMENT
- Bruno's triple expertise (AI + .NET + GitHub) directly applicable
- Advocacy role enables educational content, community building, conference talks
- Microsoft ecosystem authority provides credibility for adoption
- OSS track record demonstrates execution capability

**Adoption Potential:** HIGH VIRAL COEFFICIENT
- Solves universal pain point (every production AI app needs testing)
- 5-15 minute quickstart meets criterion
- Enterprise appeal (compliance, governance, quality assurance)
- GitHub Copilot integration amplifies productivity
- Foundry integration provides premium enterprise path

### Risk Mitigation

1. **Competitive timing:** Microsoft may expand evaluation library
   - **Mitigation:** Speed to market; community focus; complementary positioning

2. **Scope creep:** v2 features bleeding into v1
   - **Mitigation:** Strict MVP discipline, feature freeze

3. **Evaluation accuracy:** LLM-as-judge limitations
   - **Mitigation:** Extensibility SDK, transparency, hybrid approach

**No showstopper risks identified.**

---

## NEXT ACTIONS

### Immediate (Pre-Development)
1. Secure commitment from Bruno Capuano ← **USER ACTION**
2. Validate resource allocation (3-4 month focused development window)
3. Secure Microsoft DevRel endorsement
4. Validate naming/branding (trademark search)
5. Recruit beta users (5-10 early adopters)

### Development (Months 1-4)
6. Engage Microsoft.Extensions.AI team (collaborative relationship)
7. Spike hallucination detection (prototype LLM-as-judge accuracy)
8. Define v1 feature freeze (lock MVP scope)
9. Build detailed technical design (API surface, extensibility, storage)
10. Build initial community (pre-announce, gather early adopters)
11. Build MVP v1.0 (follow synthesis roadmap)
12. Prepare launch content (blog series, demo videos)

### Launch (Month 3-4)
13. Execute launch event (virtual livestream)
14. Activate community (GitHub discussions, office hours, Testing Champions)

### Post-Launch (Months 5-12)
15. Iterate based on feedback (v2.0 roadmap)
16. Scale advocacy (conference circuit, workshop kit, enterprise webinars)

---

## DECISION CONFIDENCE ASSESSMENT

**Overall Confidence: VERY HIGH**

**This is the right opportunity at the right time for the right sponsor.**

Three independent analysts converged on the same gap. Community validation is overwhelming. Strategic alignment is perfect. Technical feasibility is confirmed. Differentiation is clear.

**This represents category creation, not feature development.** NetAI.Testing has potential to become the de facto standard for AI quality assurance in .NET, analogous to xUnit for traditional testing.

---

## Related Documentation

- **Mulder Synthesis Report:** `D:\labs\netai-nextwin\.squad\agents\mulder\synthesis-report.md` (15,000+ words)
- **Doggett Validation Report:** `D:\labs\netai-nextwin\.squad\agents\doggett\validation-report.md` (28,000+ words)
- **Scully Community Research:** `D:\labs\netai-nextwin\.squad\agents\scully\community-research.md`
- **Krycek Trend Analysis:** `D:\labs\netai-nextwin\.squad\agents\krycek\trend-analysis.md`
- **Skinner Technical Landscape:** `D:\labs\netai-nextwin\.squad\agents\skinner\technical-landscape.md`
- **Reyes Advocacy Alignment:** `D:\labs\netai-nextwin\.squad\agents\reyes\advocacy-verdict.md`
- **Phase 1 Launch Log:** `D:\labs\netai-nextwin\.squad\orchestration-log\2026-02-23T2347-phase1-launch.md`
- **Phase 2-3 Log:** `D:\labs\netai-nextwin\.squad\orchestration-log\2026-02-23T2355-phase2-3.md`

---

---

## PHASE 4 COMPLETION: FINAL DELIVERY

**Date:** 2026-02-24T0000  
**Status:** ✅ DELIVERY COMPLETE

Final report assembled by Mulder and delivered to Bruno Capuano.
Both validators have approved:
- Doggett (Reality-Check): 92/100 confidence ✅
- Reyes (Advocacy Alignment): 94/100 confidence ✅

**Final Artifact:** `D:\labs\netai-nextwin\FINAL_REPORT.md`

**Report Contents:**
- Executive summary with primary recommendation
- Key patterns identified (convergent triple-validated signals)
- Opportunity shortlist (7 ranked opportunities)
- Recommended primary direction with full business case
- Community validation signals and evidence
- Differentiation strategy vs Python tools
- Implementation roadmap and success metrics
- Risk mitigation and next steps for Bruno

**Primary Recommendation:** AI Testing & Observability Toolkit for .NET (NetAI.Testing) — Score: 95/100

---

---

## IMPLEMENTATION DECISIONS: Core Components

### Decision: Evaluator Implementation Strategy

**Author:** Byers (Senior .NET Developer)
**Date:** 2025-01-24
**Status:** Implemented

#### Context
All 5 evaluators (Hallucination, Factuality, Relevance, Coherence, Safety) needed real implementations replacing NotImplementedException stubs.

#### Decisions

1. **Heuristic-only v1**: All evaluators use algorithmic approaches (token overlap, cosine similarity, regex) with no external LLM calls. This keeps them deterministic, fast, and dependency-free. A future v2 could add optional LLM-backed evaluation.

2. **Tokenization strategy**: Shared approach across evaluators — split on whitespace + punctuation, lowercase, filter tokens ≤2 chars. Not extracted to shared helper yet to keep each evaluator self-contained; can refactor if a 6th evaluator is added.

3. **SafetyEvaluator uses GeneratedRegex**: Source-generated regex (`[GeneratedRegex]`) for PII patterns (email, SSN, phone) for compile-time optimization. Requires `partial class`.

4. **Factuality claim granularity**: Claims are extracted at sentence level (split on `.!?`, ≥3 words). A claim is "supported" if ≥50% of its tokens appear in the reference. This threshold is internal and not yet configurable.

5. **Fixed duplicate RegressionReport**: Removed inline class from RegressionDetector.cs since standalone RegressionReport.cs already existed with a superset of properties.

#### Trade-offs
- Keyword overlap is a rough proxy for semantic similarity — adequate for v1 but not production-grade NLU
- CoherenceEvaluator contradiction detection uses simple antonym pairs, not logical inference
- No stopword removal — could improve relevance/factuality precision in future

---

### Decision: Dataset Design Choices

**Author:** Byers (Senior .NET Developer)
**Date:** 2025-07-15
**Status:** Proposed

#### Decisions Made

1. **Renamed DatasetLoader → JsonDatasetLoader**: The old stub was called `DatasetLoader` but only handles JSON. Renamed to `JsonDatasetLoader` for clarity and to allow future loaders (e.g., `YamlDatasetLoader`). The interface `IDatasetLoader` is the contract.

2. **Mutable Lists for Tags/Examples**: Changed `IReadOnlyList<string> Tags` to `List<string> Tags` on both GoldenExample and GoldenDataset. This is required for System.Text.Json deserialization and for AddExample() mutability. Consumers wanting immutability should use the IDatasetLoader.LoadAsync() path.

3. **CSV loader uses simple comma split**: The LoadFromCsvAsync implementation uses basic `String.Split(',')` — it does not handle quoted fields with embedded commas. For production use, a proper CSV parser (CsvHelper) should be added. This is adequate for simple datasets.

4. **Dataset files placed in Datasets/datasets/ subfolder**: Not embedded resources — kept as plain files for easy editing. If they need to ship with the NuGet package, a `<Content>` or `<EmbeddedResource>` item should be added to the csproj.

5. **DatasetVersion.Diff() keyed on Input**: The diff method uses `Input` as the identity key for matching examples between versions. If duplicate inputs are possible, this would need a dedicated ID field.

---

---

## IMPLEMENTATION DECISIONS: Documentation & CI/CD (Phase 5)

### Decision: MkDocs Material Setup for GitHub Pages Publishing

**Author:** Frohike (Technical Writer)  
**Date:** 2026-02-24  
**Status:** ✅ Implemented

#### Scope
Set up MkDocs Material theme and configuration for publishing ElBruno.AI.Evaluation blog posts and documentation as a GitHub Pages site.

#### Key Design Choices

1. **Material Theme with Dark/Light Toggle**
   - Professional, modern design suitable for developer tool
   - Built-in dark/light modes reduce eye strain
   - Excellent mobile support (responsive design)
   - Strong accessibility features (WCAG 2.1 AA)
   - Popular in .NET ecosystem

2. **Markdown Extensions Enabled**
   - `toc` — table of contents with permalinks
   - `admonition` — callout boxes (!!! note, !!! warning)
   - `pymdownx.details` — collapsible sections
   - `pymdownx.superfences` — syntax-highlighted code blocks
   - `pymdownx.tabbed` — content tabs
   - `pymdownx.tasklist` — GitHub-style task lists
   - `attr_list` — HTML attribute assignment
   - `md_in_html` — HTML in Markdown

3. **Explicit Navigation Configuration**
   - Prevents accidental publication of design docs/internal notes
   - Makes information architecture intentional and explicit
   - Easier to add new content without unwanted includes
   - Structure: Home → Blog Series (7 posts) → Library Docs (7 docs)

4. **Three Essential Plugins**
   - `search` — Full-text search in sidebar
   - `glightbox` — Image gallery lightbox (for blog screenshots)
   - `include-markdown` — Markdown inclusion for reusable content

5. **Python Dependencies** (requirements.txt)
   - `mkdocs-material>=9.5` — Material Design theme
   - `mkdocs-glightbox` — Image lightbox plugin
   - `mkdocs-include-markdown-plugin` — Markdown inclusion

#### Deliverables
- `mkdocs.yml` — Complete configuration with theme, extensions, plugins, navigation
- `requirements.txt` — Python dependency declarations
- `docs/index.md` — Welcome landing page with blog/docs sections

#### Rationale

**Why split Blog and Docs?**
- Blog: Narrative, progressive learning (intro → deep-dive → production)
- Docs: Reference material, task-oriented (how-do-I...?)
- Mirrors how developers learn (blogs for concepts, docs for APIs)

**Why explicit navigation?**
- Prevents accidental publication of design docs/internal notes
- Makes information architecture explicit and intentional
- Easier to add new content without worrying about unwanted includes

**Why these plugins?**
- Blog posts include evaluator output screenshots → images need zoom
- Future improvements can reuse examples (DRY via include-markdown)
- Developers need fast topic discovery (search)

#### Impact
- Blog posts and documentation now publication-ready
- GitHub Pages site available at `https://elbruno.github.io/netai-nextwin/`
- Developers can discover ElBruno.AI.Evaluation through searchable documentation
- Content version-controlled alongside code

---

### Decision: GitHub Actions CI/CD Workflow for MkDocs Deployment

**Author:** Byers (Senior .NET Developer)  
**Date:** 2026-02-24  
**Status:** ✅ Implemented

#### Scope
Create GitHub Actions workflow for automated MkDocs build and deploy to GitHub Pages.

#### Key Design Choices

1. **Trigger Strategy**
   - Push to main branch
   - Path filters: `docs/**`, `mkdocs.yml`, `requirements.txt`
   - Conditional build prevents unnecessary runs

2. **Build Environment**
   - Python 3.11
   - `pip install -r requirements.txt` (all MkDocs deps)
   - `mkdocs build` → output to `site/` folder

3. **Deployment Method**
   - Native GitHub Actions: `actions/upload-pages-artifact`, `actions/deploy-pages`
   - Automatic HTTPS with GitHub Pages
   - Custom domain support
   - Zero infrastructure required

4. **Permissions (Least-Privilege)**
   - `id-token: write` — GitHub Pages workflow identity
   - `pages: write` — Deploy to GitHub Pages

#### Deliverables
- `.github/workflows/deploy-docs.yml` — Complete CI/CD workflow

#### Rationale

**Why native GitHub Actions deployment?**
- Native integration, no third-party services
- Automatic HTTPS and custom domain support
- Zero infrastructure to maintain
- Seamless with GitHub ecosystem

**Why path filters?**
- Prevents workflow runs on unrelated changes (src/, tests/, etc.)
- Saves CI/CD minutes
- Faster feedback for documentation edits

#### Impact
- MkDocs site auto-publishes on every push to main
- No manual deployment steps
- Developers get instant feedback on doc changes
- Site always reflects latest main branch content

---

## Cross-Phase Integration Notes

**Documentation Setup (Phase 5):**
- Builds on Phase 4 (FINAL_REPORT.md, strategic decisions)
- Frohike + Byers automated docs publishing
- Coordinator validated build and fixed docs_dir path issues
- Site now live on every main branch push

**Team Coordination:**
- Frohike: Content & configuration (mkdocs.yml, index.md)
- Byers: Infrastructure & automation (deploy-docs.yml)
- Coordinator: Build verification & troubleshooting

---

**Last Updated:** 2026-02-24T1632Z  
**Maintained by:** Scribe  
**Status:** COMPLETE - Phase 4 Delivery + Phase 5 Documentation Setup Merged
