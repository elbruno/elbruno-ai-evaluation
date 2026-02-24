# Golden Datasets: The Foundation of AI Testing in .NET

## What's a Golden Dataset?

A **golden dataset** is a curated collection of input-output pairs that represent correct, expected behavior from your AI system. Think of it as your AI's source of truth—the examples you'd show someone to say, "This is what good looks like."

Golden datasets are the foundation of reliable AI evaluation. Without them, you're guessing. With them, you can measure quality objectively.

In ElBruno.AI.Evaluation, a golden dataset has three layers:

1. **Examples** — Individual test cases with input, expected output, and optional context
2. **Metadata** — Name, version, description, and tags for organization
3. **Summaries** — Quick statistics about what you've got

## Why Golden Datasets Matter

Consider a support chatbot. You want it to:
- Answer billing questions accurately
- Stay on-topic (not answer "How do I hack the Pentagon?")
- Use consistent tone and terminology
- Never expose customer data

A golden dataset captures these requirements as concrete examples. Instead of writing 50 test cases with if-then assertions, you define 50 examples of good behavior and measure how close your LLM gets to those examples.

Golden datasets also:
- **Enable regression testing** — Catch quality drops before they hit production
- **Track evolution** — Version your datasets like you version code
- **Support filtering** — Use tags to test specific scenarios (e.g., "Is my model good at billing questions?")
- **Facilitate team alignment** — Non-engineers can contribute examples

## Creating a Dataset Programmatically

Here's the fluent API for building datasets in code:

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

*Golden datasets are the cornerstone of AI testing. In the next post, we'll explore the five evaluators that measure how well your LLM matches those golden examples.*
