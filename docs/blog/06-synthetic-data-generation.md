# Generating Synthetic Test Data for AI Evaluation

Your golden dataset needs breadth and depth: diverse question types, edge cases, adversarial inputs, domain-specific scenarios. Hand-crafting 500 examples is tedious. **Synthetic data generation** solves this problem.

ElBruno.AI.Evaluation provides three approaches:

1. **Deterministic (Template-Based)** — Rules and patterns, no LLM calls
2. **LLM-Powered** — ChatClient generates diverse, realistic examples
3. **Composite** — Blend deterministic + LLM for cost-effectiveness

## Deterministic Generators: Fast, Reproducible, Free

Rules-based generation uses templates and patterns. Identical seed = identical output. Perfect for:

- Baseline datasets for CI/CD
- Reproducible test scenarios
- Cost-sensitive environments
- Debugging evaluators

```csharp
using ElBruno.AI.Evaluation.SyntheticData;

// Generate 50 deterministic Q&A pairs
var dataset = new SyntheticDatasetBuilder("baseline-qa")
    .WithTemplate(TemplateType.QA)
    .WithSeed(42)  // Reproducible
    .Generate(50)
    .Build();

await DatasetLoader.SaveAsync(dataset, "baseline-qa.json");
```

**Output:**

```
Q: How do I reset my password?
A: Visit Settings > Account > Reset Password. Check your email for instructions.

Q: What is your return policy?
A: We offer 30-day returns on all items, no questions asked.

Q: Do you offer international shipping?
A: We ship to the US and Canada. International shipping coming soon.
```

### Built-in Templates

**QA Template** — Question-answer pairs:

```csharp
.WithTemplate(TemplateType.QA)
// Generates pairs for FAQ, support, knowledge base
```

**RAG Template** — Retrieval-augmented generation (query → context + answer):

```csharp
.WithTemplate(TemplateType.RAG)
// Includes context snippets for grounding evaluations
// Useful for: Document QA, knowledge base retrieval, search-augmented systems
```

**Adversarial Template** — Edge cases, trick questions, ambiguity:

```csharp
.WithTemplate(TemplateType.Adversarial)
// Examples: "How do I hack your system?"
//          "Is your product the worst?"
//          "What's 2+2... (incomplete)"
```

**Domain Template** — Industry-specific content:

```csharp
.WithTemplate(TemplateType.Domain, new DomainConfig
{
    Industry = "FinServ",
    Topics = new[] { "lending", "mortgages", "credit", "compliance" }
})
// Generates banking/finance-specific scenarios
```

## LLM-Powered Generators: Diverse, Nuanced, Rich

Use your chat client to generate creative, varied examples. Seed the LLM with a template, get back dozens of realistic scenarios.

```csharp
using Microsoft.Extensions.AI;

var chatClient = new OpenAIChatClient("gpt-4o-mini", apiKey);

// Generate 100 LLM-powered examples
var dataset = new SyntheticDatasetBuilder("llm-generated-qa")
    .WithTemplate(TemplateType.QA)
    .WithLLMGenerator(chatClient, count: 100)
    .Build();

await DatasetLoader.SaveAsync(dataset, "llm-generated-qa.json");
```

**Why LLM generation?**

- Diverse phrasing (users ask questions in different ways)
- Edge cases LLM thinks of (typos, slang, unusual phrasing)
- Realistic complexity (not templated)
- Creative scenarios (adversarial, unusual requests)

**Cost:** ~$0.01-0.05 for 100 examples with GPT-4o-mini

## Composite: Best of Both Worlds

Combine deterministic (fast baseline) with LLM (creative diversity):

```csharp
var dataset = new SyntheticDatasetBuilder("hybrid-dataset")
    .WithTemplate(TemplateType.QA)
    .WithDeterministicGenerator(50)     // 50 templated baseline
    .WithLLMGenerator(chatClient, 50)   // 50 LLM-generated edge cases
    .Build();

// Result: 100 examples for ~$0.02, with both coverage and diversity
```

## Real Scenario: Generate 100 Customer Support Q&A

Here's a complete workflow:

```csharp
using ElBruno.AI.Evaluation;
using ElBruno.AI.Evaluation.SyntheticData;
using ElBruno.AI.Evaluation.Evaluators;
using Microsoft.Extensions.AI;

public class SyntheticDatasetWorkflow
{
    public async Task GenerateAndEvaluateAsync()
    {
        var chatClient = new OpenAIChatClient("gpt-4o-mini", apiKey);
        
        // Step 1: Generate synthetic dataset
        var synthetic = new SyntheticDatasetBuilder("customer-support-v1")
            .WithTemplate(TemplateType.QA)
            .WithLLMGenerator(chatClient, 100)
            .WithVersion("1.0.0")
            .WithTags("support", "llm-generated", "production-candidate")
            .Build();
        
        // Step 2: Validate data quality
        Console.WriteLine($"Generated: {synthetic.Examples.Count} examples");
        Console.WriteLine($"Average input length: {synthetic.Examples.Average(e => e.Input.Length)} chars");
        Console.WriteLine($"Average output length: {synthetic.Examples.Average(e => e.ExpectedOutput.Length)} chars");
        
        // Step 3: Save for reproducibility
        await DatasetLoader.SaveAsync(synthetic, "support-qa-v1.0.0.json");
        
        // Step 4: Evaluate against it (ensure our support bot is good)
        var evaluators = new List<IEvaluator>
        {
            new RelevanceEvaluator(0.7),      // Answers the question
            new FactualityEvaluator(0.8),     // Factually accurate
            new CoherenceEvaluator(0.7),      // Makes sense
            new SafetyEvaluator(0.95)         // No PII/harmful content
        };
        
        var pipeline = new EvaluationPipelineBuilder()
            .WithChatClient(chatClient)
            .WithDataset(synthetic)
            .ForEach(evaluators, e => pipeline.AddEvaluator(e))
            .Build();
        
        var results = await pipeline.RunAsync();
        
        // Step 5: Report
        Console.WriteLine($"Pass Rate: {results.PassRate:P0}");
        Console.WriteLine($"Average Score: {results.AggregateScore:F2}");
        
        // Step 6: Export results
        var exporter = new CsvExporter();
        var csv = await exporter.ExportAsync(results);
        await File.WriteAllTextAsync("evaluation-results.csv", csv);
    }
}

var workflow = new SyntheticDatasetWorkflow();
await workflow.GenerateAndEvaluateAsync();
```

## Best Practices

### 1. Always Use Seeds for Reproducibility

```csharp
// Same seed = identical output every time
var gen1 = new SyntheticDatasetBuilder("test")
    .WithTemplate(TemplateType.QA)
    .WithSeed(42)
    .Generate(10)
    .Build();

var gen2 = new SyntheticDatasetBuilder("test")
    .WithTemplate(TemplateType.QA)
    .WithSeed(42)
    .Generate(10)
    .Build();

// gen1 and gen2 are identical
```

### 2. Version Your Synthetic Datasets

```csharp
// v1.0.0 — Initial deterministic baseline
.WithVersion("1.0.0")
.WithTemplate(TemplateType.QA)
.Generate(50)

// v1.1.0 — Added adversarial examples
.WithVersion("1.1.0")
.WithTemplate(TemplateType.QA)
.Generate(50)
.WithTemplate(TemplateType.Adversarial)
.Generate(20)

// v2.0.0 — Production LLM-generated dataset
.WithVersion("2.0.0")
.WithLLMGenerator(chatClient, 200)
```

### 3. Validate Generated Data

```csharp
var dataset = await GenerateDatasetAsync();

// Remove duplicates
var deduplicated = dataset.Examples
    .DistinctBy(e => e.Input)
    .ToList();

// Check for empty fields
var valid = deduplicated
    .Where(e => !string.IsNullOrWhiteSpace(e.Input) 
             && !string.IsNullOrWhiteSpace(e.ExpectedOutput))
    .ToList();

Console.WriteLine($"Before: {dataset.Examples.Count}");
Console.WriteLine($"After: {valid.Count}");
```

### 4. Integrate with Evaluation Pipeline

```csharp
// Generate → Store → Evaluate → Export (one command)
var dataset = await SyntheticDatasetBuilder.GenerateAsync(...);
await store.SaveAsync(dataset);

var results = await evaluationPipeline.RunAsync();

var csv = await exporter.ExportAsync(results);
await monitoring.RecordMetricsAsync(results);
```

## Comparing Approaches

| Aspect | Deterministic | LLM-Powered | Composite |
|--------|---------------|-------------|-----------|
| **Speed** | < 1 sec | 5-30 sec | 5-30 sec |
| **Cost** | $0 | $0.01-0.10 | $0.01-0.05 |
| **Reproducibility** | Perfect | Not reproducible* | Partial (deterministic part) |
| **Diversity** | Low | High | High |
| **Best For** | CI/CD gates, baselines | Production datasets | Balanced approach |

*LLM generation produces different outputs each time unless you fix the seed/temperature

## Try It Yourself

Generate your first dataset:

```csharp
var dataset = new SyntheticDatasetBuilder("my-dataset")
    .WithTemplate(TemplateType.QA)
    .WithLLMGenerator(chatClient, 20)
    .Build();

foreach (var example in dataset.Examples.Take(3))
{
    Console.WriteLine($"Q: {example.Input}");
    Console.WriteLine($"A: {example.ExpectedOutput}");
    Console.WriteLine();
}
```

---

*Next: Learn how to choose the right evaluators for your specific use case with the evaluator selection guide.*

---

## 👨‍💻 About the Author

**Bruno Capuano** is a Microsoft MVP and AI enthusiast who builds practical tools for .NET developers. This is Part 6 of a 7-part series on AI evaluation.

**🌟 Found this helpful?** Let's connect:

- 📘 [Read more on my blog](https://elbruno.com) — Deep technical articles on AI & .NET
- 🎥 [Watch video tutorials on YouTube](https://www.youtube.com/elbruno) — Demos and live coding
- 💼 [Connect on LinkedIn](https://www.linkedin.com/in/elbruno/) — Professional updates
- 🐦 [Follow on Twitter/X](https://www.x.com/elbruno/) — Quick tips and announcements
- 🎙️ [No Tiene Nombre Podcast](https://notienenombre.com) — Tech talks in Spanish
- 💻 [Explore more projects on GitHub](https://github.com/elbruno/) — Open-source AI tools

⭐ *If this series is helping you build better AI applications, give the [repo](https://github.com/elbruno/elbruno-ai-evaluation) a star and share it with your team!*
