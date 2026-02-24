# ElBruno.AI.Evaluation

[![NuGet](https://img.shields.io/badge/NuGet-ElBruno.AI.Evaluation-blue?logo=nuget)](https://www.nuget.org/packages/ElBruno.AI.Evaluation)
[![.NET 8+](https://img.shields.io/badge/.NET-8.0-blue?logo=.net)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://github.com/elbruno/elbruno-ai-evaluation/actions/workflows/ci.yml/badge.svg)](https://github.com/elbruno/elbruno-ai-evaluation/actions)

**AI Testing & Observability Toolkit for .NET** — the xUnit for AI applications

## Why ElBruno.AI.Evaluation?

The .NET ecosystem has a testing gap when it comes to AI applications. While Python developers have frameworks like Ragas and DeepEval to systematically test and monitor LLM outputs, .NET teams lack native, production-grade alternatives. ElBruno.AI.Evaluation bridges this gap by providing comprehensive evaluators for hallucination detection, factuality verification, relevance assessment, coherence checking, and safety screening—all built natively for .NET with golden datasets, regression testing, and SQLite persistence.

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
- **[Best Practices](docs/best-practices.md)** — Production patterns, threshold tuning, CI/CD integration, and monitoring
- **[Publishing to NuGet](docs/publishing.md)** — How to publish new versions using GitHub Actions + Trusted Publishing

## Blog Series

Learn the patterns and philosophy behind the toolkit:

1. **[Introducing ElBruno.AI.Evaluation](blog/01-introducing-elbruno-ai-evaluation.md)** — Why testing AI in .NET matters
2. **[Golden Datasets for AI Testing](blog/02-golden-datasets-for-ai-testing.md)** — Building ground truth for evaluations
3. **[AI Evaluators Deep Dive](blog/03-ai-evaluators-deep-dive.md)** — Understanding each evaluator's strengths
4. **[AI Testing with xUnit](blog/04-ai-testing-with-xunit.md)** — Integrating evaluations into your test suite
5. **[From Demo to Production](blog/05-from-demo-to-production.md)** — Monitoring, baselines, and regression detection

## Samples

Real-world examples showing the toolkit in action:

- **[ChatbotEvaluation](samples/ChatbotEvaluation/)** — Customer support chatbot evaluation with multi-evaluator testing
- **[RagEvaluation](samples/RagEvaluation/)** — RAG (Retrieval-Augmented Generation) system evaluation with context-aware metrics

## Roadmap

### v1.0 (Current)
- ✅ 5 evaluators (Relevance, Factuality, Coherence, Hallucination, Safety)
- ✅ Golden datasets with JSON/CSV support
- ✅ xUnit integration
- ✅ SQLite persistence and exports

### v1.5 (Planned)
- GitHub Copilot integration for copilot-assisted test generation
- Enhanced evaluator pipeline with streaming support
- Custom evaluator marketplace

### v2.0 (Future)
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

**Author:** Bruno Capuano  
**Repository:** [elbruno/elbruno-ai-evaluation](https://github.com/elbruno/elbruno-ai-evaluation)  
**Discussions:** [GitHub Discussions](https://github.com/elbruno/elbruno-ai-evaluation/discussions)
