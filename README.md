# ElBruno.AI.Evaluation

[![NuGet](https://img.shields.io/badge/NuGet-ElBruno.AI.Evaluation-blue?logo=nuget)](https://www.nuget.org/packages/ElBruno.AI.Evaluation)
[![.NET 8+](https://img.shields.io/badge/.NET-8.0-blue?logo=.net)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://github.com/elbruno/elbruno-ai-evaluation/actions/workflows/ci.yml/badge.svg)](https://github.com/elbruno/elbruno-ai-evaluation/actions)
[![Docs](https://img.shields.io/badge/Docs-GitHub%20Pages-blue?logo=github)](https://elbruno.github.io/netai-nextwin/)

**AI Testing & Observability Toolkit for .NET** — the xUnit for AI applications

> **.NET already has [Microsoft.Extensions.AI.Evaluation](https://learn.microsoft.com/en-us/dotnet/ai/evaluation/libraries)** — a comprehensive, LLM-powered evaluation framework. **ElBruno.AI.Evaluation complements it** with offline deterministic evaluators, synthetic test data generation, golden dataset management, regression detection, and xUnit-native assertions — scenarios the official libraries don't cover. [See the full comparison →](docs/comparison-with-microsoft-evaluation.md)

## Why ElBruno.AI.Evaluation?

Microsoft's official evaluation libraries are excellent for LLM-powered quality analysis and Azure-based safety checks. But they require external LLM calls, Azure credentials, and don't provide test data generation or dataset lifecycle management. ElBruno.AI.Evaluation fills those gaps: **fast, offline, deterministic evaluators** that run in CI without any external dependencies, **synthetic data generation** to bootstrap your test suites, and **regression detection** to catch quality drops before they ship. Use them together for the best of both worlds.

## Quick Start

### Install

```bash
dotnet add package ElBruno.AI.Evaluation
dotnet add package ElBruno.AI.Evaluation.Xunit       # For xUnit tests
dotnet add package ElBruno.AI.Evaluation.Reporting   # For SQLite & exports
```

### 10-Line Example

```csharp
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using Microsoft.Extensions.AI;

// Create a golden dataset
var dataset = new GoldenDataset 
{
    Name = "customer-support",
    Examples = new()
    {
        new() { 
            Input = "How do I reset my password?",
            ExpectedOutput = "Visit Settings > Security > Reset Password"
        }
    }
};

// Configure evaluators
var evaluators = new IEvaluator[] 
{
    new RelevanceEvaluator(),      // 0.6+ similarity to expected output
    new HallucinationEvaluator(),  // No hallucinated facts
    new SafetyEvaluator()          // No PII or harmful content
};

// Run evaluation pipeline against your AI model
var results = await chatClient.EvaluateAsync(dataset, evaluators);

Console.WriteLine($"Pass Rate: {results.PassRate:P0}");
Console.WriteLine($"Aggregate Score: {results.AggregateScore:F2}");
```

## Packages

| Package | Description | Status |
|---------|-------------|--------|
| **ElBruno.AI.Evaluation** | Core evaluators, metrics, golden datasets, and extension methods for `IChatClient` | ✅ Available |
| **ElBruno.AI.Evaluation.Xunit** | xUnit attribute-based testing integration (`AIEvaluationTest`, `AIAssert`) | ✅ Available |
| **ElBruno.AI.Evaluation.Reporting** | SQLite result persistence, baseline snapshots, and export formats (JSON, CSV, Console) | ✅ Available |
| **ElBruno.AI.Evaluation.SyntheticData** | Synthetic test data generation (template-based and LLM-powered) for evaluation datasets | ✅ Available |

## Features

✨ **5 Built-in Evaluators**

- **RelevanceEvaluator** — Cosine similarity + term overlap; checks if output matches expected content
- **FactualityEvaluator** — Claim-level verification against reference text  
- **CoherenceEvaluator** — Sentence completeness, contradiction detection, repetition penalties
- **HallucinationEvaluator** — Token grounding and fact hallucination detection
- **SafetyEvaluator** — PII masking, profanity detection, harmful content blocklists

🎯 **Golden Datasets**

- Fluent API for building test data programmatically or from JSON/CSV
- Tag-based filtering and subset selection
- Semantic versioning for regression tracking
- Metadata and context support for RAG scenarios

🧬 **Synthetic Data Generation**

- Template-based (deterministic) generation for fast, reproducible test data
- LLM-powered generation via IChatClient for diverse edge cases and adversarial examples
- Composite generation combining deterministic + LLM for cost-effective diversity
- Built-in templates: Q&A, RAG, Adversarial, Domain-specific
- Validation and deduplication utilities for data quality

🔄 **Regression Testing**

- Baseline snapshots for tracking metric trends
- Automatic regression detection with configurable tolerance
- Integration with CI/CD pipelines (GitHub Actions, Azure Pipelines)

🗄️ **Production-Ready Persistence**

- SQLite storage for evaluation runs with queryable metrics
- Export to JSON, CSV, or console
- Cost and token tracking for monitoring LLM expenses
- Batch operations for large-scale evaluations

🧪 **xUnit Integration**

- `AIEvaluationTest` attribute for data-driven tests
- `AIAssert` fluent assertions (`PassesThreshold`, `AllMetricsPass`)
- Native Test Explorer support in Visual Studio
- Dataset-driven test generation

## Documentation

Comprehensive guides for all use cases:

- **[Quick Start](docs/quickstart.md)** — Get up and running in 5 minutes
- **[Evaluation Metrics](docs/evaluation-metrics.md)** — Deep dive into each evaluator with code examples and threshold guidance
- **[Golden Datasets](docs/golden-datasets.md)** — Dataset design, JSON schema, CSV import, versioning, and best practices
- **[Synthetic Data Generation](docs/synthetic-data.md)** — Template-based and LLM-powered test data generation for evaluation
- **[Best Practices](docs/best-practices.md)** — Production patterns, threshold tuning, CI/CD integration, and monitoring
- **[Security & Safety](docs/SECURITY.md)** — File operations safety, data integrity guarantees, and production best practices
- **[Publishing to NuGet](docs/publishing.md)** — How to publish new versions using GitHub Actions + Trusted Publishing

## Blog Series

> 📖 **[Read the full series on our docs site →](https://elbruno.github.io/netai-nextwin/)**

A complete developer journey through AI testing in .NET, covering both ElBruno.AI.Evaluation and Microsoft.Extensions.AI.Evaluation:

1. **[Testing AI in .NET: The Landscape](https://elbruno.github.io/netai-nextwin/blog/01-introducing-elbruno-ai-evaluation/)** — Understanding both official and complementary toolkits
2. **[Building Your Test Foundation: Golden Datasets & Synthetic Data](https://elbruno.github.io/netai-nextwin/blog/02-golden-datasets-for-ai-testing/)** — Creating and managing test data with versioning
3. **[Evaluators: From Quick Checks to Deep Analysis](https://elbruno.github.io/netai-nextwin/blog/03-ai-evaluators-deep-dive/)** — Layering deterministic and LLM-powered evaluation
4. **[AI Testing in Your CI Pipeline](https://elbruno.github.io/netai-nextwin/blog/04-ai-testing-with-xunit/)** — xUnit integration and automation for quality gates
5. **[Production AI Evaluation: Combining Both Toolkits](https://elbruno.github.io/netai-nextwin/blog/05-from-demo-to-production/)** — End-to-end hybrid pipeline with monitoring and cost tracking
6. **[Generating Synthetic Test Data for AI Evaluation](https://elbruno.github.io/netai-nextwin/blog/06-synthetic-data-generation/)** — Deep dive into template-based, LLM-powered, and composite data generation
7. **[A Guide to Choosing the Right Evaluators for Your AI App](https://elbruno.github.io/netai-nextwin/blog/07-choosing-the-right-evaluators/)** — Evaluator selection by scenario (chatbots, RAG, agents, etc.)

## Samples

Real-world examples showing the toolkit in action:

- **[OllamaEvaluation](samples/OllamaEvaluation/)** — Real LLM evaluation using local Ollama (`phi3:mini`) with 5 evaluators against actual model output
- **[FoundryLocalEvaluation](samples/FoundryLocalEvaluation/)** — Real LLM evaluation using Azure AI Foundry Local (`phi-4-mini`) with 5 evaluators against actual model output
- **[ChatbotEvaluation](samples/ChatbotEvaluation/)** — Customer support chatbot evaluation with multi-evaluator testing
- **[EvaluationJourney](samples/EvaluationJourney/)** — Complete pipeline from synthetic data → evaluators → regression detection → JSON export
- **[HybridEvaluation](samples/HybridEvaluation/)** — Fast deterministic first pass with guidance on plugging in Microsoft's LLM-based evaluators
- **[RagEvaluation](samples/RagEvaluation/)** — RAG (Retrieval-Augmented Generation) system evaluation with context-aware metrics
- **[ReportingShowcase](samples/ReportingShowcase/)** — End-to-end result persistence, export, and reporting (SQLite, JSON, CSV, Console)
- **[SyntheticDataGeneration](samples/SyntheticDataGeneration/)** — Generate Q&A, RAG, and adversarial datasets with round-trip JSON persistence

## Roadmap

### v0.5 (Current)

- ✅ 10 evaluators (Relevance, Factuality, Coherence, Hallucination, Safety, Latency, Cost, Conciseness, Consistency, Completeness)
- ✅ Golden datasets with JSON/CSV support
- ✅ xUnit integration
- ✅ SQLite persistence and exports
- ✅ Synthetic data generation (template-based and LLM-powered)
- ✅ Real LLM evaluation samples (Ollama, Foundry Local)

### v1.0 (Planned)

- GitHub Copilot integration for copilot-assisted test generation
- Enhanced evaluator pipeline with streaming support
- Custom evaluator marketplace

### v1.5 (Future)

- Visual debugging dashboard with metric drill-down
- Real-time monitoring and alerting
- Distributed evaluation for large-scale datasets
- Model-specific evaluator profiles (GPT, Claude, Llama, etc.)

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on:

- Submitting bug reports and feature requests
- Creating pull requests
- Writing evaluator plugins
- Adding new dataset formats

## License

MIT License — see [LICENSE](LICENSE) for details.

---

## 👨‍💻 About the Author

**Bruno Capuano** — AI enthusiast, Microsoft MVP, and developer advocate passionate about making AI accessible to everyone.

I create content about AI, .NET, Azure, and developer productivity across multiple platforms. If you found this library useful, you might enjoy:

🌐 **Follow my work:**

- 💻 [More AI Projects on GitHub](https://github.com/elbruno/) — Explore my open-source AI experiments and tools
- 📝 [Technical Blog](https://elbruno.com) — Deep dives into AI, .NET, and cloud development
- 🎙️ [No Tiene Nombre Podcast](https://notienenombre.com) — Tech discussions in Spanish
- 🎥 [YouTube Channel](https://www.youtube.com/elbruno) — Video tutorials, demos, and live coding
- 💼 [LinkedIn](https://www.linkedin.com/in/elbruno/) — Professional updates and industry insights
- 🐦 [Twitter/X](https://www.x.com/elbruno/) — Quick tips, announcements, and tech thoughts

**Got questions or ideas?** Let's connect! Open a [Discussion](https://github.com/elbruno/elbruno-ai-evaluation/discussions) or reach out on any platform above.

✨ *If this project saved you time or helped your AI testing workflow, consider giving it a ⭐ star and sharing it with fellow developers!*
