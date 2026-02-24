# Golden Datasets Guide

Learn how to create, manage, and version golden datasets for AI evaluation.

## What Are Golden Datasets?

A **golden dataset** is a collection of ground-truth test cases—inputs paired with expected outputs—that serve as a benchmark for AI system quality. They're the foundation of reproducible evaluation.

### Key Characteristics

- **Versioned** — Use semantic versioning (1.0.0, 1.1.0, 2.0.0)
- **Immutable** — Once released, old versions don't change
- **Tagged** — Categorize examples (e.g., "edge-case", "happy-path")
- **Metadata-rich** — Track context, notes, and provenance
- **JSON-based** — Easy to version control, review, and share

---

## JSON Format

### Basic Structure

```json
{
  "name": "Customer Support Bot Evaluation",
  "version": "1.0.0",
  "description": "Ground truth for evaluating customer support chatbot quality",
  "createdAt": "2025-01-15T10:30:00Z",
  "tags": ["production", "support-v2"],
  "examples": [
    {
      "input": "How do I reset my password?",
      "expectedOutput": "To reset your password, visit the login page and click 'Forgot Password'. Enter your email address and follow the verification steps.",
      "context": "Standard password reset procedure",
      "tags": ["account", "authentication"],
      "metadata": {
        "source": "support-tickets",
        "difficulty": "easy",
        "criticality": "high"
      }
    }
  ]
}
```

### Field Reference

| Field | Type | Required | Purpose |
|-------|------|----------|---------|
| `name` | string | Yes | Dataset identifier |
| `version` | string | No | Semantic version (default: "1.0.0") |
| `description` | string | No | Human-readable purpose |
| `createdAt` | ISO8601 | No | When dataset was created (auto-set to now) |
| `tags` | string[] | No | Dataset-level categories |
| `examples` | Example[] | Yes | Array of test cases |

### GoldenExample Fields

| Field | Type | Required | Purpose |
|-------|------|----------|---------|
| `input` | string | Yes | The query or prompt |
| `expectedOutput` | string | Yes | The reference/correct answer |
| `context` | string | No | Background info for RAG/context-aware evaluation |
| `tags` | string[] | No | Example-level labels (e.g., "edge-case") |
| `metadata` | object | No | Arbitrary key-value pairs |

---

## Loading Datasets

### From JSON File

```csharp
var loader = new JsonDatasetLoader();
var dataset = await loader.LoadAsync("path/to/dataset.json");

Console.WriteLine($"Loaded: {dataset.Name} v{dataset.Version}");
Console.WriteLine($"Examples: {dataset.Examples.Count}");
Console.WriteLine($"Tags: {string.Join(", ", dataset.Tags)}");
```

### From CSV File

Create a CSV with required columns: `Input`, `ExpectedOutput`, and optional `Context`, `Tags`:

```csv
Input,ExpectedOutput,Context,Tags
How do I login?,Visit login.example.com and enter your email.,Login page,account
What are your hours?,We're open 9-6 EST Monday-Friday.,Hours,contact
```

Load it:

```csharp
var loader = new JsonDatasetLoader();
var dataset = await loader.LoadFromCsvAsync("examples.csv");
```

---

## Creating Datasets Programmatically

Build datasets in code:

```csharp
var dataset = new GoldenDataset
{
    Name = "FAQ Evaluation",
    Version = "1.0.0",
    Description = "Common customer questions",
    CreatedAt = DateTimeOffset.UtcNow,
    Tags = new() { "faq", "production" },
    Examples = new()
    {
        new GoldenExample
        {
            Input = "How do I cancel my subscription?",
            ExpectedOutput = "Visit Settings > Billing > Cancel Subscription. Refunds are issued within 5 business days.",
            Context = "Billing FAQ section",
            Tags = new() { "billing", "cancellation" },
            Metadata = new()
            {
                ["source"] = "help-desk",
                ["reviewed_by"] = "support-team"
            }
        },
        new GoldenExample
        {
            Input = "Is there a mobile app?",
            ExpectedOutput = "Yes, download from the App Store or Google Play.",
            Context = "Apps section in knowledge base",
            Tags = new() { "mobile" }
        }
    }
};

// Save for reuse
var loader = new JsonDatasetLoader();
await loader.SaveAsync(dataset, "faq-evaluation.json");
```

---

## Dataset Operations

### Filter by Tag

Get examples matching a specific tag:

```csharp
var accountExamples = dataset.GetByTag("account");
Console.WriteLine($"Account-related examples: {accountExamples.Count}");
```

### Create a Subset

Filter examples using a predicate:

```csharp
// Get only high-criticality examples
var critical = dataset.GetSubset(e => 
    e.Metadata.GetValueOrDefault("criticality") == "high");

Console.WriteLine($"Critical examples: {critical.Examples.Count}");

// Evaluate only the subset
var run = await chatClient.EvaluateAsync(critical, evaluators);
```

### Get Summary Statistics

View dataset composition:

```csharp
var summary = dataset.GetSummary();

Console.WriteLine($"Total examples: {summary.TotalExamples}");
Console.WriteLine($"Examples with context: {summary.ExamplesWithContext}");
Console.WriteLine($"Unique tags: {string.Join(", ", summary.UniqueTags)}");
```

---

## Versioning Strategy

Use semantic versioning:

- **MAJOR** (e.g., 2.0.0) — Breaking changes, incompatible with 1.x evaluations
- **MINOR** (e.g., 1.1.0) — New examples added, backward compatible
- **PATCH** (e.g., 1.0.1) — Bug fixes, typo corrections

### Guidelines

1. **Don't modify released versions** — Create a new version instead
2. **Document changes** — Keep a changelog
3. **Version with your model** — If you release a new model, bump the dataset version too
4. **Branch by use case** — Separate datasets for different domains (support_v1.0.0, rag_v2.1.0)

### Example Changelog

```markdown
# Customer Support Dataset Changelog

## v1.1.0 (2025-01-20)
- Added 5 new edge-case examples for billing inquiries
- Fixed typo in example 12 (expected output)
- Added "escalation" tag for difficult cases
- Examples: 25 → 30

## v1.0.0 (2025-01-15)
- Initial release
- 25 examples covering: account, billing, technical, general
```

---

## Best Practices

### 1. Balance Coverage

Include diverse scenarios:

```json
{
  "examples": [
    { "tags": ["happy-path"], ... },    // Normal, happy scenarios
    { "tags": ["edge-case"], ... },     // Unusual but valid inputs
    { "tags": ["error-handling"], ... }, // Invalid/error cases
    { "tags": ["ambiguous"], ... }      // Unclear or multi-interpretable
  ]
}
```

### 2. Keep Expected Outputs Realistic

Expected outputs should represent **actual human-quality responses**, not idealized perfection:

```json
{
  "input": "What does your product do?",
  "expectedOutput": "Our product is a cloud analytics platform for real-time business intelligence. Key features include dashboards, automated reporting, and integrations with popular data sources.",
  "context": "Standard product description"
}
```

Don't create artificially perfect outputs:

```json
{
  "input": "How does it work?",
  "expectedOutput": "Step 1: Connect your data. Step 2: Build queries. Step 3: Create dashboards. Step 4: Share insights. Step 5: Monitor performance. Step 6: Export results. Step 7: Set up alerts. Step 8: Collaborate. Step 9: Scale usage. Step 10: Achieve success.",
  "context": "DO NOT DO THIS — unrealistic!"
}
```

### 3. Include Context for RAG Evaluation

For retrieval-augmented generation, always include context:

```json
{
  "input": "What year was the company founded?",
  "expectedOutput": "The company was founded in 2019.",
  "context": "Company Overview: Established in 2019, we serve 5000+ customers across 40 countries."
}
```

### 4. Tag Systematically

Use consistent, predefined tags:

```csharp
// Good: Consistent tag naming
tags: ["technical", "urgent", "authentication"]

// Bad: Inconsistent naming
tags: ["tech", "URGENT", "login"]

// Better: Use enum or constants
public static class ExampleTags
{
    public const string Happy = "happy-path";
    public const string EdgeCase = "edge-case";
    public const string Error = "error-handling";
}
```

### 5. Version with Your Models

When you release or update a model, version your dataset too:

```csharp
// If models changed significantly, bump dataset version
// model-v2.0.0 → evaluation-dataset-v2.0.0
// This makes it clear which dataset was used to evaluate which model
```

### 6. Review and Audit

Before release:

- ✅ Have humans review all expected outputs
- ✅ Check for typos, grammatical errors
- ✅ Verify context is accurate and helpful
- ✅ Ensure examples don't contain PII
- ✅ Confirm tags are consistent

### 7. Maintain Lineage

Track where examples came from:

```json
{
  "input": "...",
  "expectedOutput": "...",
  "metadata": {
    "source": "support-tickets",
    "ticket_id": "SUP-12345",
    "reviewed_by": "john.doe@example.com",
    "date_added": "2025-01-15"
  }
}
```

### 8. Size Appropriately

Dataset size depends on use case:

| Scenario | Recommended Size |
|----------|-----------------|
| Development/testing | 10-20 examples |
| Pre-release validation | 50-100 examples |
| Regression testing | 100+ examples |
| Production monitoring | 500+ examples (ongoing) |

Larger datasets catch more issues but take longer to evaluate.

---

## Organizing Multiple Datasets

For complex systems, use multiple focused datasets:

```
datasets/
├── customer-support.json       # v1.3.0 — 50 support examples
├── rag-retrieval.json           # v2.0.0 — 30 retrieval examples
├── safety-moderation.json       # v1.0.0 — 20 safety examples
├── code-generation.json         # v3.1.0 — 40 coding examples
└── multi-language.json          # v1.0.0 — 25 non-English examples
```

Load and evaluate against specific datasets:

```csharp
var supportDataset = await loader.LoadAsync("datasets/customer-support.json");
var ragDataset = await loader.LoadAsync("datasets/rag-retrieval.json");

var supportResults = await chatClient.EvaluateAsync(supportDataset, evaluators);
var ragResults = await chatClient.EvaluateAsync(ragDataset, evaluators);

Console.WriteLine($"Support: {supportResults.Results.Count(r => r.Passed)}/{supportResults.Results.Count} passed");
Console.WriteLine($"RAG: {ragResults.Results.Count(r => r.Passed)}/{ragResults.Results.Count} passed");
```

---

## Troubleshooting

### "Failed to deserialize dataset"

- Verify JSON is valid (use jsonlint.com)
- Ensure `name` and `examples` fields exist
- Check for encoding issues (use UTF-8)

### Missing Examples After CSV Import

- Verify CSV has `Input` and `ExpectedOutput` columns (exact case)
- Check that header row exists
- Ensure rows don't have empty inputs or outputs

### Subset Returns No Results

- Verify examples have the tags you're filtering by
- Use `GetSummary()` to check available tags:

```csharp
var summary = dataset.GetSummary();
Console.WriteLine($"Available tags: {string.Join(", ", summary.UniqueTags)}");
```

---

## Real-World Example: Building an FAQ Dataset

```csharp
// Step 1: Create initial dataset from knowledge base
var dataset = new GoldenDataset
{
    Name = "Product FAQ",
    Version = "1.0.0",
    Description = "FAQ examples extracted from help center",
    Tags = new() { "production" }
};

// Step 2: Add examples with full metadata
var faqs = new[]
{
    ("What's your pricing?", "We offer 3 tiers: Starter ($29), Pro ($99), Enterprise (custom).", "Pricing page"),
    ("Do you have a free trial?", "Yes, 14 days free. No credit card required.", "Signup flow"),
    ("Can I export my data?", "Yes, CSV, JSON, and API access included in all plans.", "Data export docs")
};

foreach (var (q, a, context) in faqs)
{
    dataset.AddExample(new GoldenExample
    {
        Input = q,
        ExpectedOutput = a,
        Context = context,
        Tags = new() { "faq", "general" },
        Metadata = new() { ["source"] = "help-center" }
    });
}

// Step 3: Save
var loader = new JsonDatasetLoader();
await loader.SaveAsync(dataset, "faq.json");

// Step 4: Evaluate against it
var evaluators = new IEvaluator[]
{
    new RelevanceEvaluator(0.65),
    new CoherenceEvaluator(0.70)
};

var run = await chatClient.EvaluateAsync(dataset, evaluators);
var passRate = (double)run.Results.Count(r => r.Passed) / run.Results.Count;
Console.WriteLine($"FAQ Pass Rate: {passRate:P0}");
```

---

## Next Steps

- Review evaluation-metrics.md to understand how examples are scored
- Check best-practices.md for testing patterns and workflows
- Explore samples/ for complete working examples with datasets
