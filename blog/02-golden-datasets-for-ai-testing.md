# Building Your Test Foundation: Golden Datasets & Synthetic Data

## Before You Evaluate

You can't evaluate without something to evaluate against. That something is a **golden dataset**—a curated collection of input-output pairs representing correct behavior. It's your AI's ground truth.

But here's the problem: creating golden datasets manually is tedious. You need dozens or hundreds of examples. ElBruno.AI.Evaluation addresses this with two strategies:

1. **Golden Datasets** — Curated, versioned collections (your hand-crafted ground truth)
2. **Synthetic Data** — Automatically generated test examples (deterministic or LLM-powered)

**Important:** Microsoft.Extensions.AI.Evaluation assumes you *already have* a golden dataset. ElBruno provides the tools to *create and manage* one. This is a critical gap ElBruno fills.

## Golden Datasets: Your Source of Truth

A golden dataset has three layers:

1. **Examples** — Individual test cases with input, expected output, and optional context
2. **Metadata** — Name, version, description, and tags for organization
3. **Summaries** — Quick statistics about what you've got

## Synthetic Data: Generate, Don't Hand-Craft

Creating 100 golden examples by hand? No thanks. ElBruno's `SyntheticDatasetBuilder` generates test data for you. Three strategies:

### 1. Deterministic (Template-Based) Generation

Fast, reproducible, no LLM calls:

```csharp
var synthetic = new SyntheticDatasetBuilder("qa-pairs")
    .WithTemplate(TemplateType.QA)
    .Generate(50);  // 50 deterministic examples

// Output: Standard Q&A pairs like:
// Q: "How do I reset my password?"
// A: "Visit Settings > Account > Reset Password."
```

**Use this for:** Quick iteration, CI/CD, cost-sensitive scenarios, reproducible baselines.

### 2. LLM-Powered Generation

Diverse, nuanced, uses your chat client:

```csharp
using var http = new HttpClient();
var chatClient = new OpenAIChatClient("gpt-4o-mini", apiKey);

var synthetic = new SyntheticDatasetBuilder("customer-support")
    .WithTemplate(TemplateType.QA)
    .WithLLMGenerator(chatClient, count: 50)
    .Build();

// ChatClient generates 50 realistic examples
```

**Use this for:** Production datasets, edge cases, adversarial examples, diversity.

### 3. Composite Generation

Blend deterministic + LLM for cost-effective diversity:

```csharp
var synthetic = new SyntheticDatasetBuilder("rag-qa")
    .WithTemplate(TemplateType.RAG)
    .WithDeterministicGenerator(25)    // 25 templated
    .WithLLMGenerator(chatClient, 25)  // 25 LLM-generated
    .Build();
```

**Use this for:** Balanced approach—fast baseline + realistic edge cases.

## Built-in Templates

ElBruno provides domain-specific templates:

### Q&A Template

```csharp
.WithTemplate(TemplateType.QA)
// Generates: Question → Answer pairs
```

### RAG Template

```csharp
.WithTemplate(TemplateType.RAG)
// Generates: Query → Context + Answer (for retrieval scenarios)
```

### Adversarial Template

```csharp
.WithTemplate(TemplateType.Adversarial)
// Generates: Edge cases, trick questions, ambiguous inputs
```

### Domain Template

```csharp
.WithTemplate(TemplateType.Domain, new DomainConfig { 
    Industry = "FinServ",
    Topics = new[] { "lending", "mortgages", "compliance" }
})
// Generates: Domain-specific examples with context
```

## End-to-End Example: Generate → Evaluate

```csharp
using ElBruno.AI.Evaluation;
using ElBruno.AI.Evaluation.SyntheticData;

// Step 1: Generate synthetic data
var generator = new SyntheticDatasetBuilder("customer-support-v1")
    .WithTemplate(TemplateType.QA)
    .WithLLMGenerator(chatClient, 100)
    .WithVersion("1.0.0")
    .Build();

await DatasetLoader.SaveAsync(generator, "dataset-v1.0.0.json");

// Step 2: Evaluate against it
var dataset = await DatasetLoader.LoadAsync("dataset-v1.0.0.json");

var evaluators = new List<IEvaluator>
{
    new RelevanceEvaluator(0.7),
    new FactualityEvaluator(0.8),
    new SafetyEvaluator(0.95)
};

var pipeline = new EvaluationPipelineBuilder()
    .WithChatClient(chatClient)
    .WithDataset(dataset)
    .ForEach(evaluators, e => pipeline.AddEvaluator(e))
    .Build();

var results = await pipeline.RunAsync();
Console.WriteLine($"Pass Rate: {results.PassRate:P0}");
```

## Versioning Your Datasets

Always version datasets like code:

```csharp
// v1.0.0 — Initial synthetic dataset
var v1 = new SyntheticDatasetBuilder("support-bot")
    .WithVersion("1.0.0")
    .WithTemplate(TemplateType.QA)
    .Generate(50)
    .Build();

// v1.1.0 — Added adversarial examples
var v2 = new SyntheticDatasetBuilder("support-bot")
    .WithVersion("1.1.0")
    .WithTemplate(TemplateType.QA)
    .Generate(50)
    .WithTemplate(TemplateType.Adversarial)
    .Generate(20)
    .Build();

// v2.0.0 — LLM-generated for production
var v3 = new SyntheticDatasetBuilder("support-bot")
    .WithVersion("2.0.0")
    .WithLLMGenerator(chatClient, 200)
    .Build();

await DatasetLoader.SaveAsync(v3, "dataset-v2.0.0.json");
```

Store in your repo:

```
/datasets
  /support-bot
    v1.0.0.json  (deterministic baseline)
    v1.1.0.json  (+ adversarial)
    v2.0.0.json  (LLM-generated, production)
```

```csharp
using ElBruno.AI.Evaluation.Datasets;

var dataset = new GoldenDataset
{
    Name = "E-Commerce Support",
    Version = "1.0.0",
    Description = "Golden examples for e-commerce support bot",
    Tags = new() { "support", "production", "v1" }
};

// Add individual examples
dataset.AddExample(new GoldenExample
{
    Input = "Can I return an item after 30 days?",
    ExpectedOutput = "Our standard return window is 30 days from purchase. Items returned after this period may not be eligible for refund, but contact our support team for special circumstances.",
    Context = "Return policy allows 30 days. Extended returns possible with manager approval.",
    Tags = new() { "returns", "policy" },
    Metadata = new()
    {
        { "difficulty", "easy" },
        { "priority", "high" }
    }
});

dataset.AddExample(new GoldenExample
{
    Input = "What's the cheapest shipping option?",
    ExpectedOutput = "We offer standard shipping (5-7 business days) at no extra cost for orders over $50. Express and overnight shipping are available at checkout.",
    Tags = new() { "shipping", "pricing" }
});

dataset.AddExample(new GoldenExample
{
    Input = "Do you ship internationally?",
    ExpectedOutput = "We currently ship to Canada and Mexico. For other countries, please contact our international sales team.",
    Tags = new() { "shipping", "international" }
});
```

## Organizing with Tags and Subsets

Golden datasets often contain hundreds of examples. Tags let you organize and filter them:

```csharp
// Get all billing-related examples
var billingExamples = dataset.GetByTag("billing");
Console.WriteLine($"Found {billingExamples.Count} billing examples");

// Create a test subset for canary testing
var canaryDataset = dataset.GetSubset(e => 
    e.Tags.Contains("priority") && e.Tags.Contains("high")
);

// Only test easy examples in CI/CD, hard ones in staging
var easyExamples = dataset.GetSubset(e => 
    e.Metadata.TryGetValue("difficulty", out var d) && d == "easy"
);
```

## JSON Format

Under the hood, golden datasets are just JSON. Here's what a dataset looks like when persisted:

```json
{
  "name": "Support Bot",
  "version": "1.0.0",
  "description": "Golden examples for support bot",
  "createdAt": "2025-02-23T15:30:00Z",
  "tags": ["support", "production"],
  "examples": [
    {
      "input": "How do I reset my password?",
      "expectedOutput": "Visit Settings > Account > Reset Password. Check your email for instructions.",
      "context": "Password reset is available in account settings.",
      "tags": ["onboarding", "account"],
      "metadata": {
        "difficulty": "easy",
        "priority": "high"
      }
    },
    {
      "input": "Can I use multiple discount codes?",
      "expectedOutput": "No, only one discount code can be applied per order.",
      "tags": ["billing", "discounts"],
      "metadata": {
        "difficulty": "easy",
        "priority": "medium"
      }
    }
  ]
}
```

## Importing from CSV

Need to bulk-import examples? The `JsonDatasetLoader` supports CSV import:

```csharp
var loader = new JsonDatasetLoader();

// CSV must have columns: Input, ExpectedOutput, Context (optional), Tags (optional)
var dataset = await loader.LoadFromCsvAsync("support-examples.csv");

// Context and Tags are optional; missing values are handled gracefully
await loader.SaveAsync(dataset, "support-dataset.json");
```

**CSV format example:**

```csv
Input,ExpectedOutput,Context,Tags
"How do I reset my password?","Visit Settings > Account > Reset Password.","Password reset is available in account settings.","onboarding;account"
"What's your refund policy?","30-day refunds on all purchases.","Our standard return period is 30 days from purchase.","billing;policy"
"Do you ship internationally?","We ship to Canada and Mexico only.","International shipping is limited.","shipping;international"
```

## Versioning and Tracking

Always version your datasets. When you improve examples or add new ones, bump the version:

```csharp
// Version 1.0.0 — Initial release
var v1 = new GoldenDataset { Name = "Support Bot", Version = "1.0.0", ... };

// Version 1.1.0 — Added international shipping examples
var v2 = new GoldenDataset { Name = "Support Bot", Version = "1.1.0", ... };

// Version 2.0.0 — Complete rewrite with 200 examples
var v3 = new GoldenDataset { Name = "Support Bot", Version = "2.0.0", ... };
```

This matters because:

- **Reproducibility** — You can always re-evaluate against the exact dataset you used
- **Comparison** — See if quality improves when you move to a newer dataset
- **Debugging** — When a test fails, you know exactly which version of ground truth was used

Store versions in your repository:

```
/datasets
  /support-bot
    v1.0.0.json
    v1.1.0.json
    v2.0.0.json (current)
```

## Dataset Statistics

Quickly understand what's in your dataset:

```csharp
var summary = dataset.GetSummary();

Console.WriteLine($"Total examples: {summary.TotalExamples}");
Console.WriteLine($"With context: {summary.ExamplesWithContext}");
Console.WriteLine($"Tags: {string.Join(", ", summary.UniqueTags)}");
```

Output:

```
Total examples: 42
With context: 28
Tags: billing, shipping, returns, account, priority
```

## Best Practices

**1. Keep Examples Simple and Focused**
Each example should test one thing. A long rambling expected output makes evaluation harder.

**2. Include Edge Cases**
Don't just add happy-path examples. Include misspellings, slang, confusing phrasing, unusual requests.

**3. Version Religiously**
Treat your golden dataset like source code. Commit it to git, tag releases, document changes.

**4. Use Metadata Liberally**
Add fields that help you slice and dice: difficulty, priority, category, language, edge_case_type.

**5. Involve Your Team**
Non-engineers (support, product) can contribute examples. Golden datasets are a bridge between technical and non-technical stakeholders.

## Try It Yourself

Start with a small dataset—10-15 examples covering your main use cases. Then:

1. Run your LLM against them
2. See which evaluators flag issues
3. Refine the dataset based on what you learn
4. Version it in git
5. Use it as a baseline for future improvements

Create your first dataset:

```csharp
var loader = new JsonDatasetLoader();

var dataset = new GoldenDataset
{
    Name = "My App",
    Version = "1.0.0"
};

dataset.AddExample(new GoldenExample
{
    Input = "Your first test case",
    ExpectedOutput = "What good looks like"
});

await loader.SaveAsync(dataset, "my-dataset.json");
```

That's it. You now have a foundation for rigorous AI evaluation.

---

*Next: Learn how to layer multiple evaluators—from ElBruno's fast offline checks to Microsoft's LLM-powered quality judgment.*

---

## 👨‍💻 About the Author

**Bruno Capuano** is a Microsoft MVP and AI enthusiast who builds practical tools for .NET developers. This is Part 2 of a 7-part series on AI evaluation.

**🌟 Found this helpful?** Let's connect:

- 📘 [Read more on my blog](https://elbruno.com) — Deep technical articles on AI & .NET
- 🎥 [Watch video tutorials on YouTube](https://www.youtube.com/elbruno) — Demos and live coding
- 💼 [Connect on LinkedIn](https://www.linkedin.com/in/elbruno/) — Professional updates
- 🐦 [Follow on Twitter/X](https://www.x.com/elbruno/) — Quick tips and announcements
- 🎙️ [No Tiene Nombre Podcast](https://notienenombre.com) — Tech talks in Spanish
- 💻 [Explore more projects on GitHub](https://github.com/elbruno/) — Open-source AI tools

⭐ *If this series is helping you build better AI applications, give the [repo](https://github.com/elbruno/elbruno-ai-evaluation) a star and share it with your team!*
