# Synthetic Data Generation for AI Evaluation

## What Is Synthetic Data Generation?

Synthetic data generation automatically creates test datasets for evaluating AI systems—saving you from painstaking manual curation. Rather than handwriting thousands of Q&A pairs or edge cases, ElBruno.AI.Evaluation.SyntheticData lets you generate realistic, diverse, and reproducible datasets using **templates** (deterministic) and **LLMs** (AI-powered).

### Why It Matters for AI Evaluation

1. **Speed** — Generate 1,000 test cases in seconds instead of weeks of manual work
2. **Scale** — Build large enough datasets to catch regressions and corner cases
3. **Reproducibility** — Use fixed random seeds to generate the same data every time (for deterministic tests)
4. **Diversity** — Mix template-based and LLM-generated examples to balance cost and variety
5. **Domain Compliance** — Generate data that respects domain constraints (HIPAA, PCI, etc.)

## Quick Start: Generate a Simple Q&A Dataset

```csharp
using ElBruno.AI.Evaluation.SyntheticData;
using ElBruno.AI.Evaluation.SyntheticData.Templates;

// Define Q&A templates (question patterns → answer patterns)
var qaTemplate = new QaTemplate(
    questionTemplates: new[]
    {
        "What is {topic}?",
        "How do I {action}?",
        "Explain {concept} to me.",
    },
    answerTemplates: new[]
    {
        "{topic} is a {definition}.",
        "To {action}, follow these steps: {steps}.",
        "{concept} refers to {explanation}.",
    }
).WithCategory("general-knowledge")
 .AddTags("faq", "quick-start");

// Generate 50 Q&A examples
var dataset = await new SyntheticDatasetBuilder("my-first-qa-dataset")
    .WithVersion("1.0.0")
    .WithDescription("Quick-start Q&A dataset")
    .WithTags("synthetic", "deterministic")
    .UseDeterministicGenerator(strategy =>
    {
        strategy.Template = qaTemplate;
        strategy.RandomSeed = 42; // Reproducible across runs
    })
    .GenerateQaPairs(count: 50)
    .BuildAsync();

Console.WriteLine($"✅ Generated {dataset.Examples.Count} Q&A examples");
```

## Template-Based (Deterministic) Generation

Deterministic generation uses templates to combine question patterns with answer patterns. Outputs are **reproducible** (same seed = same data) and **fast**.

### Use Cases
- Building CI/CD test datasets that must be stable
- Generating baseline datasets for regression testing
- Creating lightweight test data with predictable variation

### Example: FAQ Chatbot

```csharp
var qaTemplate = new QaTemplate(
    questionTemplates: new[]
    {
        "What is the return policy?",
        "How long does shipping take?",
        "Is there a warranty?",
    },
    answerTemplates: new[]
    {
        "Items can be returned within {days} days of purchase.",
        "Standard delivery takes {duration} business days.",
        "All products include a {warranty} warranty.",
    }
).WithCategory("ecommerce")
 .AddTags("returns", "shipping", "warranty");

var dataset = await new SyntheticDatasetBuilder("faq-chatbot")
    .WithVersion("1.0.0")
    .UseDeterministicGenerator(strategy =>
    {
        strategy.Template = qaTemplate;
        strategy.RandomSeed = 99; // Reproducible
    })
    .GenerateQaPairs(count: 30)
    .BuildAsync();
```

### Controlling Randomness

- **With seed** (deterministic): `strategy.RandomSeed = 42` — same examples every run
- **Without seed** (non-deterministic): `strategy.RandomSeed = null` — different examples each run
- **Shuffling**: `strategy.Shuffle = true` — randomize example order (even with a seed)

## LLM-Powered Generation with IChatClient

Use an `IChatClient` (e.g., `OpenAIChatClient`, `AnthropicClient`) to generate diverse, AI-powered synthetic data. Perfect for **edge cases, adversarial testing, and natural-language variation**.

### Use Cases
- Generating adversarial/edge-case examples
- Creating natural language variations
- Domain-specific data with compliance constraints

### Example: LLM-Generated Adversarial Cases

```csharp
using Microsoft.Extensions.AI;

var chatClient = new OpenAIChatClient(apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

var dataset = await new SyntheticDatasetBuilder("adversarial-edge-cases")
    .WithVersion("1.0.0")
    .WithDescription("LLM-generated edge cases")
    .WithTags("synthetic", "llm", "adversarial")
    .UseLlmGenerator(chatClient, strategy =>
    {
        strategy.SystemPrompt = 
            "You are an expert at finding edge cases and breaking AI systems. " +
            "Generate adversarial inputs: typos, contradictions, null/empty values, truncated queries.";
        strategy.GenerationTemplate = GenerationTemplate.AdversarialCases;
        strategy.Temperature = 0.9; // Higher creativity
        strategy.MaxTokens = 300;
        strategy.ParallelismDegree = 5; // Generate 5 examples in parallel
        strategy.RetryOnFailure = true; // Retry failed generations
    })
    .GenerateAdversarialExamples(count: 100)
    .BuildAsync();

Console.WriteLine($"✅ Generated {dataset.Examples.Count} adversarial examples");
```

### LLM Strategy Options

```csharp
strategy.SystemPrompt              // Guide the LLM with a system message
strategy.GenerationTemplate        // Predefined output format (see next section)
strategy.Temperature               // 0.0 = deterministic, 2.0 = very creative (default: 0.7)
strategy.MaxTokens                 // Response length limit (default: 500)
strategy.ParallelismDegree         // Parallel requests (default: 1)
strategy.RetryOnFailure            // Auto-retry failed generations (default: true)
strategy.MaxRetries                // Max retry attempts (default: 3)
```

## Composite/Hybrid Generation

Mix deterministic and LLM-powered generation for optimal **cost + diversity**.

### Example: 70% Deterministic + 30% LLM

```csharp
var template = new QaTemplate(
    questionTemplates: new[] { "What is {topic}?", "How do I {action}?" },
    answerTemplates: new[] { "{topic} is a {definition}.", "To {action}: {steps}." }
);

var dataset = await new SyntheticDatasetBuilder("hybrid-qa-dataset")
    .WithVersion("1.0.0")
    .UseCompositeGenerator(config =>
    {
        // 70% deterministic (fast & cheap)
        config.AddDeterministicGenerator(
            template: template,
            weight: 0.7,
            randomSeed: 42);
        
        // 30% LLM-powered (diverse & natural)
        config.AddLlmGenerator(
            chatClient: chatClient,
            systemPrompt: "Generate diverse, natural Q&A pairs with varied phrasings.",
            generationTemplate: GenerationTemplate.SimpleQA,
            weight: 0.3);
    })
    .GenerateQaPairs(count: 100)
    .BuildAsync();

// Result: ~70 deterministic examples + ~30 LLM-powered examples
Console.WriteLine($"✅ Generated {dataset.Examples.Count} hybrid examples");
```

## Built-in Templates

### 1. QaTemplate — Question-Answer Pairs

For FAQ systems, chatbots, and knowledge bases.

```csharp
var template = new QaTemplate(
    questionTemplates: new[] { "What is {topic}?", "How do I {action}?" },
    answerTemplates: new[] { "{topic} is a {definition}.", "To {action}: {steps}." }
).WithCategory("technical-support")
 .AddTags("faq", "support");
```

**Properties:**
- `questionTemplates` — List of question patterns with placeholders
- `answerTemplates` — List of answer patterns with placeholders
- `WithCategory()` — Category/domain for these pairs
- `AddTags()` — Tags for filtering
- `WithMetadata()` — Custom key-value metadata

### 2. RagTemplate — Context + Answer (Retrieval-Augmented Generation)

For RAG systems where answers must be grounded in retrieved documents.

```csharp
var template = new RagTemplate(
    documents: new[]
    {
        "Return Policy: Items can be returned within 30 days of purchase.",
        "Shipping: Free shipping on orders over $50. Standard delivery takes 5-7 business days.",
        "Warranty: All products include a 1-year manufacturer's warranty.",
    },
    qaExamples: new[]
    {
        ("What is the return policy?", "Items can be returned within 30 days of purchase."),
        ("How long does shipping take?", "Standard delivery takes 5-7 business days."),
        ("Is there a warranty?", "All products include a 1-year warranty."),
    }
).WithDocumentsPerExample(2)
 .AddTags("rag", "ecommerce");
```

**Properties:**
- `documents` — List of document chunks (context sources)
- `qaExamples` — Q&A pairs anchoring the examples
- `WithDocumentsPerExample()` — Number of documents per example (default: 1)
- `AddTags()`, `WithMetadata()` — Standard template options

### 3. AdversarialTemplate — Edge Cases & Perturbations

For stress-testing your AI system with adversarial inputs.

```csharp
var baseExamples = new[]
{
    new GoldenExample { Input = "What is X?", ExpectedOutput = "X is Y." },
    new GoldenExample { Input = "How do I Z?", ExpectedOutput = "To Z: follow steps." },
};

var template = new AdversarialTemplate(baseExamples)
    .WithNullInjection(enabled: true)          // Inject null/empty strings
    .WithTruncation(enabled: true)             // Truncate inputs (incomplete queries)
    .WithTypoInjection(enabled: true)          // Typos and character corruption
    .WithContradictions(enabled: true)         // Contradictory context vs expected output
    .WithLongInputs(enabled: false, maxLength: 2000) // Extremely long inputs
    .AddTags("adversarial", "edge-cases");
```

**Perturbation Types:**
- `WithNullInjection()` — Create null/empty variations
- `WithTruncation()` — Partial/incomplete inputs
- `WithTypoInjection()` — Character-level errors
- `WithContradictions()` — Conflicting context
- `WithLongInputs()` — Stress-test with extremely long inputs

### 4. DomainTemplate — Domain-Specific Generation

For generating data within domain constraints (healthcare, finance, legal, etc.).

```csharp
var template = new DomainTemplate("healthcare")
    .WithVocabulary(new[] { "patient", "diagnosis", "treatment", "medication", "symptom" })
    .WithConstraints(
        "Do not include real patient names or SSNs",
        "Only discuss public medical information",
        "Follow HIPAA guidelines")
    .WithComplianceFramework("HIPAA")
    .AddTags("healthcare", "hipaa-compliant");
```

**Use with LLM generation:**

```csharp
var dataset = await new SyntheticDatasetBuilder("healthcare-qa")
    .WithDescription("HIPAA-compliant healthcare Q&A")
    .UseLlmGenerator(chatClient, strategy =>
    {
        strategy.SystemPrompt = template.Domain + " vocabulary and constraints.";
        strategy.GenerationTemplate = GenerationTemplate.DomainSpecific;
        strategy.Temperature = 0.5; // Lower variance for compliance
    })
    .GenerateDomainExamples("healthcare", count: 50)
    .BuildAsync();
```

## Generation Templates for LLM

When using `UseLlmGenerator()`, specify the output format:

```csharp
public enum GenerationTemplate
{
    SimpleQA,            // Input → Output
    RagContext,          // Input + Context → Output
    QAWithExplanation,   // Input → Output + Explanation
    QAVariations,        // Multiple variations of Input → Output
    AdversarialCases,    // Edge cases and adversarial examples
    DomainSpecific,      // Domain-specific examples with metadata
}
```

Example:

```csharp
strategy.GenerationTemplate = GenerationTemplate.QAWithExplanation;
// Expects: { "input": "...", "output": "...", "explanation": "..." }
```

## Integration with the Evaluation Pipeline

Once you've generated synthetic data, use it in your evaluation pipeline:

```csharp
// Generate synthetic dataset
var syntheticDataset = await new SyntheticDatasetBuilder("test-data")
    .UseDeterministicGenerator(/* ... */)
    .GenerateQaPairs(count: 100)
    .BuildAsync();

// Use in evaluation pipeline
var evaluators = new IEvaluator[] 
{
    new RelevanceEvaluator(),
    new HallucinationEvaluator(),
    new SafetyEvaluator()
};

var results = await chatClient.EvaluateAsync(syntheticDataset, evaluators);
Console.WriteLine($"Pass Rate: {results.PassRate:P0}");
```

## Best Practices for Synthetic Test Data

### 1. **Start with Deterministic, Add LLM Later**

Generate baseline data with templates first. It's fast, reproducible, and cheap. Add LLM-powered examples once you need diversity.

```csharp
var dataset = await builder
    .UseDeterministicGenerator(/* ... */)
    .GenerateQaPairs(100)
    .BuildAsync();
```

### 2. **Use Fixed Seeds for CI/CD**

Ensure your test data is stable across runs:

```csharp
strategy.RandomSeed = 42; // Fixed → reproducible tests
```

### 3. **Validate Generated Data**

Check for quality issues before using:

```csharp
var errors = dataset.ValidateExamples(new ValidationOptions
{
    MinInputLength = 5,
    MaxInputLength = 500,
    FlagNullInputs = true,
    FlagDuplicateInputs = true,
});

if (errors.Count > 0)
{
    Console.WriteLine($"⚠️  Found {errors.Count} validation issues");
    foreach (var error in errors)
        Console.WriteLine($"  - Example {error.ExampleIndex}: {error.Message}");
}
```

### 4. **Deduplicate Before Evaluation**

Remove duplicate inputs to avoid biased metrics:

```csharp
var cleanedDataset = dataset.Deduplicate();
Console.WriteLine($"Removed {dataset.Examples.Count - cleanedDataset.Examples.Count} duplicates");
```

### 5. **Augment Existing Golden Datasets**

Expand hand-curated datasets with synthetic examples:

```csharp
var baseDataset = new GoldenDataset { Name = "support", Examples = new() { /* ... */ } };

var augmented = await baseDataset.AugmentWithSyntheticExamplesAsync(
    generator: new DeterministicGenerator(template, randomSeed: 42),
    count: 50);

Console.WriteLine($"✅ Expanded from {baseDataset.Examples.Count} to {augmented.Examples.Count} examples");
```

### 6. **Mix Deterministic + LLM for Cost & Quality**

Deterministic examples are ~100x cheaper than LLM calls. Use them strategically:

```csharp
var dataset = await builder
    .UseCompositeGenerator(config =>
    {
        config.AddDeterministicGenerator(template, weight: 0.8, randomSeed: 42);
        config.AddLlmGenerator(chatClient, systemPrompt, GenerationTemplate.SimpleQA, weight: 0.2);
    })
    .GenerateQaPairs(1000)
    .BuildAsync();

// Result: 800 deterministic examples (free) + 200 LLM examples (low cost)
```

### 7. **Tag and Filter by Use Case**

Organize data with tags for easy selection:

```csharp
var template = new QaTemplate(/* ... */)
    .AddTags("faq", "technical-support", "priority-tier-1");

// Later: filter by tag in evaluation
var filteredExamples = dataset.Examples
    .Where(ex => ex.Tags.Contains("priority-tier-1"))
    .ToList();
```

### 8. **Version Your Synthetic Datasets**

Like golden datasets, version synthetic data for regression tracking:

```csharp
var dataset = await builder
    .WithVersion("1.0.0")
    .WithDescription("Initial synthetic dataset for chatbot")
    .WithTags("v1.0", "baseline")
    ./* ... */
    .BuildAsync();
```

## Summary

| Scenario | Template | Generator | Cost | Reproducibility |
|----------|----------|-----------|------|-----------------|
| Fast CI/CD data | QA/RAG | Deterministic | ✅ Free | ✅ 100% (with seed) |
| Edge cases | Adversarial | LLM | ⚠️ Higher | ❌ Variable |
| Domain-specific | Domain | LLM | ⚠️ Higher | ❌ Variable |
| Balanced approach | QA/RAG | Composite (70% det + 30% LLM) | ✅ Low | ✅ Good |

## See Also

- **[Quick Start](quickstart.md)** — Get started with evaluation
- **[Evaluation Metrics](evaluation-metrics.md)** — Understanding evaluators
- **[Golden Datasets](golden-datasets.md)** — Designing ground truth
- **[Best Practices](best-practices.md)** — Production patterns

---

**Author:** Frohike  
**Package:** ElBruno.AI.Evaluation.SyntheticData  
**License:** MIT
