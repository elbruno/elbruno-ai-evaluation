# Design: ElBruno.AI.Evaluation.SyntheticData

**Status:** Design Phase  
**Version:** 1.0  
**Author:** Mulder (Research Director)  
**Date:** 2025  
**Target Release:** ElBruno.AI.Evaluation v1.5

---

## 1. Executive Summary

`ElBruno.AI.Evaluation.SyntheticData` is the 4th NuGet package in the ElBruno.AI.Evaluation toolkit. It generates synthetic `GoldenDataset` and `GoldenExample` instances for AI evaluation testing, supporting both deterministic (template-based) and AI-powered (IChatClient-based) generation strategies. The package provides a fluent builder API consistent with existing pipeline patterns and handles common evaluation scenarios: Q&A pairs, RAG context+answer, adversarial edge cases, and domain-specific data.

---

## 2. Package Metadata

| Property | Value |
|----------|-------|
| **Package ID** | `ElBruno.AI.Evaluation.SyntheticData` |
| **Target Framework** | `.NET 8.0+` |
| **Language** | C# latest (with nullable enabled) |
| **Root Namespace** | `ElBruno.AI.Evaluation.SyntheticData` |
| **Dependencies** | `ElBruno.AI.Evaluation` (latest), `Microsoft.Extensions.AI.Abstractions` 9.5.0+ |
| **License** | MIT |

---

## 3. Project Structure

```
src/ElBruno.AI.Evaluation.SyntheticData/
├── ElBruno.AI.Evaluation.SyntheticData.csproj
├── SyntheticDatasetBuilder.cs          # Main fluent builder entry point
├── Generators/
│   ├── ISyntheticDataGenerator.cs      # Core interface for all generators
│   ├── DeterministicGenerator.cs       # Template-based generation
│   ├── LlmGenerator.cs                 # IChatClient-based generation
│   └── CompositeGenerator.cs           # Combines multiple generators
├── Templates/
│   ├── IDataTemplate.cs                # Interface for template definitions
│   ├── QaTemplate.cs                   # Q&A pair templates
│   ├── RagTemplate.cs                  # RAG context+answer templates
│   ├── AdversarialTemplate.cs          # Edge-case & adversarial templates
│   └── DomainTemplate.cs               # Domain-specific scenario templates
├── Strategies/
│   ├── GenerationStrategy.cs           # Strategy enum/classes
│   ├── TemplateStrategy.cs             # Template-based settings
│   └── LlmStrategy.cs                  # LLM-based settings
├── Extensions/
│   └── SyntheticDatasetExtensions.cs   # Helper methods for datasets
└── Utilities/
    ├── RandomSeedProvider.cs           # Deterministic seed management
    └── PromptGenerator.cs              # LLM prompt composition
```

---

## 4. Public API Surface

### 4.1 Core Builder

#### `SyntheticDatasetBuilder` (Public Class)

Main entry point for fluent dataset generation. Implements fluent builder pattern consistent with `EvaluationPipelineBuilder`.

**Public Constructor & Methods:**

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData;

/// <summary>
/// Fluent builder for generating synthetic golden datasets.
/// </summary>
public sealed class SyntheticDatasetBuilder
{
    // Initialization
    public SyntheticDatasetBuilder(string datasetName);
    
    // Metadata configuration
    /// <summary>Sets the semantic version of the dataset.</summary>
    public SyntheticDatasetBuilder WithVersion(string version);
    
    /// <summary>Sets the dataset description.</summary>
    public SyntheticDatasetBuilder WithDescription(string description);
    
    /// <summary>Adds tags to categorize the dataset.</summary>
    public SyntheticDatasetBuilder WithTags(params string[] tags);
    
    // Generator selection & configuration
    /// <summary>Uses deterministic template-based generation.</summary>
    public SyntheticDatasetBuilder UseDeterministicGenerator(
        Action<TemplateStrategy> configureStrategy);
    
    /// <summary>Uses LLM-powered generation via IChatClient.</summary>
    public SyntheticDatasetBuilder UseLlmGenerator(
        IChatClient chatClient,
        Action<LlmStrategy> configureStrategy);
    
    /// <summary>Uses composite generation (deterministic + LLM).</summary>
    public SyntheticDatasetBuilder UseCompositeGenerator(
        Action<CompositeGeneratorConfig> configureComposite);
    
    // Scenario-specific builders (syntactic sugar)
    /// <summary>Generates Q&A pairs from a template.</summary>
    public SyntheticDatasetBuilder GenerateQaPairs(
        int count,
        Action<QaTemplate>? configure = null);
    
    /// <summary>Generates RAG context+answer examples.</summary>
    public SyntheticDatasetBuilder GenerateRagExamples(
        int count,
        Action<RagTemplate>? configure = null);
    
    /// <summary>Generates adversarial/edge-case examples.</summary>
    public SyntheticDatasetBuilder GenerateAdversarialExamples(
        int count,
        Action<AdversarialTemplate>? configure = null);
    
    /// <summary>Generates domain-specific examples.</summary>
    public SyntheticDatasetBuilder GenerateDomainExamples(
        string domain,
        int count,
        Action<DomainTemplate>? configure = null);
    
    // Output
    /// <summary>Builds and returns the synthetic GoldenDataset.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no generator is configured.</exception>
    public Task<GoldenDataset> BuildAsync(CancellationToken ct = default);
}
```

---

### 4.2 Generator Interfaces & Implementations

#### `ISyntheticDataGenerator` (Public Interface)

Core abstraction for all generation strategies.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Core interface for synthetic data generation strategies.
/// </summary>
public interface ISyntheticDataGenerator
{
    /// <summary>
    /// Generates synthetic GoldenExample instances.
    /// </summary>
    /// <param name="count">Number of examples to generate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of generated GoldenExample instances.</returns>
    Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default);
}
```

---

#### `DeterministicGenerator` (Public Class)

Template-based generation with deterministic results.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Generates synthetic examples deterministically using templates.
/// Suitable for reproducible, lightweight test data generation.
/// </summary>
public sealed class DeterministicGenerator : ISyntheticDataGenerator
{
    /// <summary>
    /// Creates a new deterministic generator with the specified template.
    /// </summary>
    public DeterministicGenerator(IDataTemplate template);
    
    /// <summary>
    /// Creates a new deterministic generator with optional random seed for reproducibility.
    /// </summary>
    public DeterministicGenerator(IDataTemplate template, int? randomSeed);
    
    /// <inheritdoc />
    public Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default);
}
```

---

#### `LlmGenerator` (Public Class)

IChatClient-powered generation with AI-driven content creation.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Generates synthetic examples using an IChatClient.
/// Provides AI-powered, high-variance data suitable for adversarial/edge-case testing.
/// </summary>
public sealed class LlmGenerator : ISyntheticDataGenerator
{
    /// <summary>
    /// Creates a new LLM-based generator.
    /// </summary>
    public LlmGenerator(
        IChatClient chatClient,
        string systemPrompt,
        GenerationTemplate generationTemplate);
    
    /// <summary>
    /// Sets the temperature for generation (0.0-2.0). Default: 0.7.
    /// </summary>
    public LlmGenerator WithTemperature(double temperature);
    
    /// <summary>
    /// Sets the maximum tokens per response. Default: 500.
    /// </summary>
    public LlmGenerator WithMaxTokens(int maxTokens);
    
    /// <summary>
    /// Sets the number of parallel generation requests. Default: 1.
    /// </summary>
    public LlmGenerator WithParallelism(int degree);
    
    /// <inheritdoc />
    public Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default);
}
```

---

#### `CompositeGenerator` (Public Class)

Combines multiple generators for hybrid generation strategies.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Combines multiple ISyntheticDataGenerator instances for hybrid generation.
/// Example: 70% deterministic examples + 30% LLM-powered examples.
/// </summary>
public sealed class CompositeGenerator : ISyntheticDataGenerator
{
    /// <summary>
    /// Creates a new composite generator with weighted sub-generators.
    /// </summary>
    public CompositeGenerator(
        params (ISyntheticDataGenerator generator, double weight)[] generators);
    
    /// <inheritdoc />
    public Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default);
}
```

---

### 4.3 Template Interfaces & Implementations

#### `IDataTemplate` (Public Interface)

Base interface for all template types.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Base interface for data templates.
/// Templates define the structure and content patterns for synthetic data generation.
/// </summary>
public interface IDataTemplate
{
    /// <summary>
    /// Gets the template type/category.
    /// </summary>
    string TemplateType { get; }
    
    /// <summary>
    /// Gets the example tags to apply to generated examples.
    /// </summary>
    IReadOnlyList<string> Tags { get; }
    
    /// <summary>
    /// Gets optional metadata to include in generated examples.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
```

---

#### `QaTemplate` (Public Class)

Generates Q&A pair examples.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for generating Q&A (question-answer) pairs.
/// Suitable for FAQ systems, chatbots, and knowledge-based QA.
/// </summary>
public sealed class QaTemplate : IDataTemplate
{
    /// <summary>
    /// Creates a new Q&A template with question prompts and answer patterns.
    /// </summary>
    public QaTemplate(
        IReadOnlyList<string> questionTemplates,
        IReadOnlyList<string> answerTemplates);
    
    /// <summary>
    /// Sets the category/domain for these Q&A pairs (e.g., "technical-support").
    /// </summary>
    public QaTemplate WithCategory(string category);
    
    /// <summary>
    /// Adds tags to all generated examples.
    /// </summary>
    public QaTemplate AddTags(params string[] tags);
    
    /// <summary>
    /// Adds metadata key-value pairs to all generated examples.
    /// </summary>
    public QaTemplate WithMetadata(Dictionary<string, string> metadata);
    
    /// <summary>
    /// Gets or generates template pairs (question → answer mappings).
    /// </summary>
    public IReadOnlyList<(string Question, string Answer)> GetPairs();
    
    public string TemplateType => "QA";
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
```

---

#### `RagTemplate` (Public Class)

Generates RAG (Retrieval-Augmented Generation) context+answer examples.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for generating RAG examples (question + context → answer).
/// Suitable for evaluating retrieval-augmented generation systems.
/// </summary>
public sealed class RagTemplate : IDataTemplate
{
    /// <summary>
    /// Creates a new RAG template with document contexts and question-answer pairs.
    /// </summary>
    public RagTemplate(
        IReadOnlyList<string> documents,
        IReadOnlyList<(string Question, string Answer)> qaExamples);
    
    /// <summary>
    /// Sets the number of document chunks per example. Default: 1.
    /// </summary>
    public RagTemplate WithDocumentsPerExample(int count);
    
    /// <summary>
    /// Sets tags for the RAG examples.
    /// </summary>
    public RagTemplate AddTags(params string[] tags);
    
    /// <summary>
    /// Adds metadata to all generated examples.
    /// </summary>
    public RagTemplate WithMetadata(Dictionary<string, string> metadata);
    
    /// <summary>
    /// Gets the underlying documents (context sources).
    /// </summary>
    public IReadOnlyList<string> Documents { get; }
    
    /// <summary>
    /// Gets the Q&A pairs that anchor the examples.
    /// </summary>
    public IReadOnlyList<(string Question, string Answer)> QaExamples { get; }
    
    public string TemplateType => "RAG";
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
```

---

#### `AdversarialTemplate` (Public Class)

Generates edge-case and adversarial examples.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for generating adversarial and edge-case examples.
/// Includes nulls, empty strings, contradictions, typos, and malformed inputs.
/// </summary>
public sealed class AdversarialTemplate : IDataTemplate
{
    /// <summary>
    /// Creates a new adversarial template with base examples to perturb.
    /// </summary>
    public AdversarialTemplate(IReadOnlyList<GoldenExample> baseExamples);
    
    /// <summary>
    /// Enables null/empty input injection. Default: true.
    /// </summary>
    public AdversarialTemplate WithNullInjection(bool enabled = true);
    
    /// <summary>
    /// Enables input truncation (partial/incomplete queries). Default: true.
    /// </summary>
    public AdversarialTemplate WithTruncation(bool enabled = true);
    
    /// <summary>
    /// Enables typo/character-level perturbations. Default: true.
    /// </summary>
    public AdversarialTemplate WithTypoInjection(bool enabled = true);
    
    /// <summary>
    /// Enables contradiction injection (conflicting context vs expected output). Default: true.
    /// </summary>
    public AdversarialTemplate WithContradictions(bool enabled = true);
    
    /// <summary>
    /// Enables extremely long input generation. Default: false.
    /// </summary>
    public AdversarialTemplate WithLongInputs(bool enabled = false, int maxLength = 2000);
    
    /// <summary>
    /// Adds tags to all generated adversarial examples.
    /// </summary>
    public AdversarialTemplate AddTags(params string[] tags);
    
    public string TemplateType => "Adversarial";
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
```

---

#### `DomainTemplate` (Public Class)

Generates domain-specific examples (healthcare, finance, legal, etc.).

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for domain-specific synthetic data generation.
/// Handles domain vocabularies, terminology, and realistic constraints.
/// </summary>
public sealed class DomainTemplate : IDataTemplate
{
    /// <summary>
    /// Creates a template for a specific domain (e.g., "healthcare", "finance", "legal").
    /// </summary>
    public DomainTemplate(string domain);
    
    /// <summary>
    /// Sets domain-specific vocabulary/terms to use in generation.
    /// </summary>
    public DomainTemplate WithVocabulary(IReadOnlyList<string> terms);
    
    /// <summary>
    /// Sets domain-specific constraints (e.g., "only discuss public companies" for finance).
    /// </summary>
    public DomainTemplate WithConstraints(params string[] constraints);
    
    /// <summary>
    /// Sets the regulatory/compliance framework (e.g., "HIPAA" for healthcare).
    /// </summary>
    public DomainTemplate WithComplianceFramework(string framework);
    
    /// <summary>
    /// Adds tags reflecting the domain.
    /// </summary>
    public DomainTemplate AddTags(params string[] tags);
    
    /// <summary>
    /// Adds domain-specific metadata.
    /// </summary>
    public DomainTemplate WithMetadata(Dictionary<string, string> metadata);
    
    /// <summary>
    /// Gets the domain name (e.g., "healthcare", "finance").
    /// </summary>
    public string Domain { get; }
    
    /// <summary>
    /// Gets the vocabulary terms for this domain.
    /// </summary>
    public IReadOnlyList<string> Vocabulary { get; }
    
    public string TemplateType => $"Domain:{Domain}";
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
```

---

### 4.4 Strategy Configuration Classes

#### `TemplateStrategy` (Public Class)

Configuration for deterministic template-based generation.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Strategies;

/// <summary>
/// Configuration strategy for deterministic template-based generation.
/// </summary>
public sealed class TemplateStrategy
{
    /// <summary>
    /// Gets or sets the data template to use for generation.
    /// </summary>
    public IDataTemplate? Template { get; set; }
    
    /// <summary>
    /// Gets or sets the optional random seed for reproducible generation.
    /// Null = non-deterministic.
    /// </summary>
    public int? RandomSeed { get; set; }
    
    /// <summary>
    /// Gets or sets whether to shuffle generated examples. Default: false.
    /// </summary>
    public bool Shuffle { get; set; } = false;
    
    /// <summary>
    /// Gets or sets how to handle missing/null expected outputs.
    /// Options: "skip", "use_input_as_expected", "empty_string".
    /// Default: "empty_string".
    /// </summary>
    public string? NullHandling { get; set; } = "empty_string";
}
```

---

#### `LlmStrategy` (Public Class)

Configuration for LLM-powered generation.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Strategies;

/// <summary>
/// Configuration strategy for LLM-powered synthetic data generation.
/// </summary>
public sealed class LlmStrategy
{
    /// <summary>
    /// Gets or sets the system prompt that guides the LLM.
    /// </summary>
    public string? SystemPrompt { get; set; }
    
    /// <summary>
    /// Gets or sets the generation template specifying output structure.
    /// </summary>
    public GenerationTemplate? GenerationTemplate { get; set; }
    
    /// <summary>
    /// Gets or sets the temperature for generation (0.0-2.0). Default: 0.7.
    /// </summary>
    public double Temperature { get; set; } = 0.7;
    
    /// <summary>
    /// Gets or sets the maximum tokens per response. Default: 500.
    /// </summary>
    public int MaxTokens { get; set; } = 500;
    
    /// <summary>
    /// Gets or sets the degree of parallelism. Default: 1.
    /// </summary>
    public int ParallelismDegree { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets whether to retry failed generations. Default: true.
    /// </summary>
    public bool RetryOnFailure { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum retry count. Default: 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
```

---

#### `GenerationTemplate` (Public Enum)

Predefined output formats for LLM-powered generation.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Strategies;

/// <summary>
/// Predefined output format templates for LLM generation.
/// Guides the LLM to produce structured GoldenExample instances.
/// </summary>
public enum GenerationTemplate
{
    /// <summary>Simple Input → Output pairs.</summary>
    SimpleQA = 0,
    
    /// <summary>Input + Context → Output (RAG-style).</summary>
    RagContext = 1,
    
    /// <summary>Input → Output + Explanation.</summary>
    QAWithExplanation = 2,
    
    /// <summary>Multiple variations of Input → Output for the same concept.</summary>
    QAVariations = 3,
    
    /// <summary>Edge cases and adversarial examples.</summary>
    AdversarialCases = 4,
    
    /// <summary>Domain-specific examples with metadata.</summary>
    DomainSpecific = 5,
}
```

---

#### `CompositeGeneratorConfig` (Public Class)

Configuration for composite/hybrid generation.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Strategies;

/// <summary>
/// Configuration for composite generation (hybrid deterministic + LLM).
/// </summary>
public sealed class CompositeGeneratorConfig
{
    /// <summary>
    /// Adds a deterministic generator with a weight (proportion of total examples).
    /// </summary>
    public void AddDeterministicGenerator(
        IDataTemplate template,
        double weight,
        int? randomSeed = null);
    
    /// <summary>
    /// Adds an LLM generator with a weight.
    /// </summary>
    public void AddLlmGenerator(
        IChatClient chatClient,
        string systemPrompt,
        GenerationTemplate generationTemplate,
        double weight);
    
    /// <summary>
    /// Gets the configured composite generator.
    /// </summary>
    public CompositeGenerator Build();
}
```

---

### 4.5 Extension Methods

#### `SyntheticDatasetExtensions` (Public Class)

Helper methods for working with synthetic datasets.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Extensions;

/// <summary>
/// Extension methods for GoldenDataset and synthetic data operations.
/// </summary>
public static class SyntheticDatasetExtensions
{
    /// <summary>
    /// Augments an existing dataset with synthetically generated examples.
    /// </summary>
    public static async Task<GoldenDataset> AugmentWithSyntheticExamplesAsync(
        this GoldenDataset dataset,
        ISyntheticDataGenerator generator,
        int count,
        CancellationToken ct = default);
    
    /// <summary>
    /// Merges multiple datasets into a single combined dataset.
    /// </summary>
    public static GoldenDataset Merge(
        this GoldenDataset dataset,
        params GoldenDataset[] otherDatasets);
    
    /// <summary>
    /// Deduplicates examples by input hash.
    /// </summary>
    public static GoldenDataset Deduplicate(this GoldenDataset dataset);
    
    /// <summary>
    /// Validates synthetic examples (non-null inputs/outputs, reasonable lengths).
    /// </summary>
    public static IReadOnlyList<ValidationError> ValidateExamples(
        this GoldenDataset dataset,
        ValidationOptions? options = null);
}
```

#### `ValidationError` (Public Class)

Represents a validation issue in synthetic data.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Extensions;

/// <summary>
/// Represents a validation error found in synthetic examples.
/// </summary>
public sealed class ValidationError
{
    /// <summary>Index of the example in the dataset.</summary>
    public int ExampleIndex { get; init; }
    
    /// <summary>Type of validation error (e.g., "null_input", "empty_output").</summary>
    public string ErrorType { get; init; } = string.Empty;
    
    /// <summary>Human-readable error message.</summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>Severity level: "error", "warning".</summary>
    public string Severity { get; init; } = "error";
}
```

#### `ValidationOptions` (Public Class)

Configuration for validation logic.

```csharp
namespace ElBruno.AI.Evaluation.SyntheticData.Extensions;

/// <summary>
/// Options for validating synthetic datasets.
/// </summary>
public sealed class ValidationOptions
{
    /// <summary>Minimum input length (characters). Default: 1.</summary>
    public int MinInputLength { get; set; } = 1;
    
    /// <summary>Maximum input length (characters). Default: 5000.</summary>
    public int MaxInputLength { get; set; } = 5000;
    
    /// <summary>Minimum expected output length (characters). Default: 0 (allow empty).</summary>
    public int MinOutputLength { get; set; } = 0;
    
    /// <summary>Maximum expected output length (characters). Default: 5000.</summary>
    public int MaxOutputLength { get; set; } = 5000;
    
    /// <summary>Whether to flag examples with null inputs. Default: true.</summary>
    public bool FlagNullInputs { get; set; } = true;
    
    /// <summary>Whether to flag examples with null expected outputs. Default: false (allowed).</summary>
    public bool FlagNullOutputs { get; set; } = false;
    
    /// <summary>Whether to check for duplicate inputs. Default: true.</summary>
    public bool FlagDuplicateInputs { get; set; } = true;
}
```

---

## 5. Scenario Examples with Code

### 5.1 Scenario: Generate Simple Q&A Pairs (Deterministic)

**Use Case:** Quick, reproducible test data for chatbot evaluation.

```csharp
using ElBruno.AI.Evaluation.SyntheticData;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using ElBruno.AI.Evaluation.Datasets;

// Define Q&A templates
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
 .AddTags("faq", "general");

// Build dataset
var dataset = await new SyntheticDatasetBuilder("qa-chatbot-examples")
    .WithVersion("1.0.0")
    .WithDescription("Generated Q&A pairs for chatbot evaluation")
    .WithTags("synthetic", "deterministic")
    .UseDeterministicGenerator(strategy =>
    {
        strategy.Template = qaTemplate;
        strategy.RandomSeed = 42; // Reproducible
    })
    .GenerateQaPairs(count: 50)
    .BuildAsync();

Console.WriteLine($"✅ Generated {dataset.Examples.Count} Q&A examples");
```

---

### 5.2 Scenario: Generate RAG Examples with Context (Deterministic)

**Use Case:** Evaluate RAG systems with synthetic documents.

```csharp
var ragTemplate = new RagTemplate(
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

var dataset = await new SyntheticDatasetBuilder("rag-evaluation")
    .WithVersion("1.0.0")
    .WithDescription("RAG evaluation dataset with mock documents")
    .UseDeterministicGenerator(strategy =>
    {
        strategy.Template = ragTemplate;
        strategy.RandomSeed = 123;
    })
    .GenerateRagExamples(count: 25)
    .BuildAsync();

Console.WriteLine($"✅ Generated {dataset.Examples.Count} RAG examples");
```

---

### 5.3 Scenario: Generate LLM-Powered Adversarial Cases

**Use Case:** Find edge cases and corner cases via AI generation.

```csharp
using Microsoft.Extensions.AI;

var chatClient = new OpenAIChatClient(...); // Your LLM client

var dataset = await new SyntheticDatasetBuilder("adversarial-edge-cases")
    .WithVersion("1.0.0")
    .WithDescription("LLM-generated adversarial examples")
    .WithTags("synthetic", "llm", "adversarial")
    .UseLlmGenerator(chatClient, strategy =>
    {
        strategy.SystemPrompt = "You are an expert at generating adversarial inputs to break AI systems. " +
                                "Create edge cases, typos, contradictions, and malformed inputs.";
        strategy.GenerationTemplate = GenerationTemplate.AdversarialCases;
        strategy.Temperature = 0.9; // Higher creativity
        strategy.MaxTokens = 300;
        strategy.ParallelismDegree = 5; // Generate 5 at a time
    })
    .GenerateAdversarialExamples(count: 100)
    .BuildAsync();

Console.WriteLine($"✅ Generated {dataset.Examples.Count} adversarial examples via LLM");
```

---

### 5.4 Scenario: Hybrid Generation (70% Deterministic + 30% LLM)

**Use Case:** Balance cost and diversity; use templates for baseline, LLM for variety.

```csharp
var template = new QaTemplate(...);

var dataset = await new SyntheticDatasetBuilder("hybrid-qa-dataset")
    .WithVersion("1.0.0")
    .UseCompositeGenerator(config =>
    {
        // 70% deterministic
        config.AddDeterministicGenerator(
            template: template,
            weight: 0.7,
            randomSeed: 42);
        
        // 30% LLM-powered
        config.AddLlmGenerator(
            chatClient: chatClient,
            systemPrompt: "Generate diverse, natural Q&A pairs.",
            generationTemplate: GenerationTemplate.SimpleQA,
            weight: 0.3);
    })
    .GenerateQaPairs(count: 100)
    .BuildAsync();

Console.WriteLine($"✅ Generated {dataset.Examples.Count} hybrid examples (70% deterministic, 30% LLM)");
```

---

### 5.5 Scenario: Domain-Specific Generation (Healthcare)

**Use Case:** Generate realistic healthcare examples with compliance constraints.

```csharp
var template = new DomainTemplate("healthcare")
    .WithVocabulary(new[] { "patient", "diagnosis", "treatment", "medication", "symptom", "HIPAA" })
    .WithConstraints("Do not include real patient names or SSNs", 
                     "Only discuss public medical information",
                     "Follow HIPAA guidelines")
    .WithComplianceFramework("HIPAA")
    .AddTags("healthcare", "domain-specific", "hipaa-compliant");

var dataset = await new SyntheticDatasetBuilder("healthcare-qa-examples")
    .WithVersion("1.0.0")
    .WithDescription("Domain-specific healthcare Q&A")
    .WithTags("healthcare", "synthetic")
    .UseLlmGenerator(chatClient, strategy =>
    {
        strategy.SystemPrompt = 
            "Generate healthcare Q&A pairs. Only discuss public medical information. " +
            "Do not include real patient data. Follow HIPAA principles.";
        strategy.GenerationTemplate = GenerationTemplate.DomainSpecific;
        strategy.Temperature = 0.5; // Lower variance for compliance
    })
    .GenerateDomainExamples("healthcare", count: 50)
    .BuildAsync();

Console.WriteLine($"✅ Generated {dataset.Examples.Count} healthcare examples");
```

---

### 5.6 Scenario: Augment Existing Dataset

**Use Case:** Expand a hand-curated golden dataset with synthetic examples.

```csharp
// Start with an existing golden dataset
var baseDataset = new GoldenDataset 
{ 
    Name = "customer-support",
    Examples = new() { /* existing examples */ }
};

// Generate synthetic examples
var syntheticTemplate = new QaTemplate(...);
var augmentedDataset = await baseDataset.AugmentWithSyntheticExamplesAsync(
    generator: new DeterministicGenerator(syntheticTemplate, randomSeed: 42),
    count: 50);

Console.WriteLine($"✅ Augmented dataset now has {augmentedDataset.Examples.Count} examples");
```

---

### 5.7 Scenario: Validate and Deduplicate

**Use Case:** Ensure quality of generated dataset.

```csharp
// Validate examples
var validationErrors = dataset.ValidateExamples(new ValidationOptions
{
    MinInputLength = 5,
    MaxInputLength = 1000,
    MinOutputLength = 10,
    FlagDuplicateInputs = true,
});

if (validationErrors.Count > 0)
{
    foreach (var error in validationErrors)
        Console.WriteLine($"❌ Example {error.ExampleIndex}: {error.Message}");
}
else
{
    Console.WriteLine("✅ All examples passed validation");
}

// Deduplicate
var cleanDataset = dataset.Deduplicate();
Console.WriteLine($"✅ Removed duplicates: {dataset.Examples.Count} → {cleanDataset.Examples.Count}");
```

---

## 6. File-by-File Implementation Breakdown

### Core Builder
- **File:** `SyntheticDatasetBuilder.cs`
  - **Lines:** ~250
  - **Responsibilities:**
    - Fluent builder entry point
    - Configuration management
    - Coordination of generator selection
    - Delegation to scenario-specific methods
    - Async `BuildAsync()` that instantiates generator and calls `GenerateAsync()`

### Generators

- **File:** `Generators/ISyntheticDataGenerator.cs`
  - **Lines:** ~20
  - **Responsibilities:**
    - Core interface definition
    - Single method: `GenerateAsync(int count, CancellationToken ct)`

- **File:** `Generators/DeterministicGenerator.cs`
  - **Lines:** ~100
  - **Responsibilities:**
    - Template-based generation
    - Deterministic RNG (optional seed)
    - Example composition from template data
    - Null handling according to strategy

- **File:** `Generators/LlmGenerator.cs`
  - **Lines:** ~200
  - **Responsibilities:**
    - IChatClient integration
    - System/user prompt construction
    - JSON parsing of LLM responses
    - Parallelism + retry logic
    - Temperature/token configuration

- **File:** `Generators/CompositeGenerator.cs`
  - **Lines:** ~80
  - **Responsibilities:**
    - Weight-based distribution across sub-generators
    - Aggregate results into single list
    - Maintain ordering/shuffling

### Templates

- **File:** `Templates/IDataTemplate.cs`
  - **Lines:** ~20
  - **Responsibilities:**
    - Base interface
    - TemplateType, Tags, Metadata properties

- **File:** `Templates/QaTemplate.cs`
  - **Lines:** ~120
  - **Responsibilities:**
    - Q&A pair storage and management
    - Question/answer interpolation
    - Tag/metadata handling
    - Fluent configuration (WithCategory, AddTags, etc.)

- **File:** `Templates/RagTemplate.cs`
  - **Lines:** ~130
  - **Responsibilities:**
    - Document management
    - Q&A mapping
    - Context chunk selection
    - Fluent configuration

- **File:** `Templates/AdversarialTemplate.cs`
  - **Lines:** ~140
  - **Responsibilities:**
    - Base example perturbation strategies
    - Null injection, truncation, typo injection, contradiction, long input logic
    - Individual enable/disable toggles
    - Fluent configuration

- **File:** `Templates/DomainTemplate.cs`
  - **Lines:** ~130
  - **Responsibilities:**
    - Domain management (healthcare, finance, legal, etc.)
    - Vocabulary/constraint storage
    - Compliance framework tracking
    - Fluent configuration

### Strategies

- **File:** `Strategies/TemplateStrategy.cs`
  - **Lines:** ~30
  - **Responsibilities:**
    - Configuration POCO for deterministic generation
    - Template reference, random seed, shuffle flag, null handling strategy

- **File:** `Strategies/LlmStrategy.cs`
  - **Lines:** ~40
  - **Responsibilities:**
    - Configuration POCO for LLM generation
    - System prompt, generation template, temperature, max tokens, parallelism, retry settings

- **File:** `Strategies/GenerationTemplate.cs`
  - **Lines:** ~20
  - **Responsibilities:**
    - Enum: SimpleQA, RagContext, QAWithExplanation, QAVariations, AdversarialCases, DomainSpecific

- **File:** `Strategies/CompositeGeneratorConfig.cs`
  - **Lines:** ~60
  - **Responsibilities:**
    - Fluent configuration builder for composite generation
    - Methods to add deterministic/LLM sub-generators with weights
    - Build() method to instantiate CompositeGenerator

### Extensions & Utilities

- **File:** `Extensions/SyntheticDatasetExtensions.cs`
  - **Lines:** ~180
  - **Responsibilities:**
    - `AugmentWithSyntheticExamplesAsync()` - add synthetic examples to existing dataset
    - `Merge()` - combine multiple datasets
    - `Deduplicate()` - remove duplicate inputs
    - `ValidateExamples()` - validate dataset quality

- **File:** `Extensions/ValidationError.cs`
  - **Lines:** ~25
  - **Responsibilities:**
    - Result POCO for validation errors
    - ExampleIndex, ErrorType, Message, Severity

- **File:** `Extensions/ValidationOptions.cs`
  - **Lines:** ~40
  - **Responsibilities:**
    - Configuration POCO for validation
    - Length constraints, null/duplicate flags

- **File:** `Utilities/RandomSeedProvider.cs`
  - **Lines:** ~50
  - **Responsibilities:**
    - Centralized random number generation
    - Seed management for reproducibility
    - Helper methods for common RNG patterns

- **File:** `Utilities/PromptGenerator.cs`
  - **Lines:** ~150
  - **Responsibilities:**
    - System prompt composition for LLM generation
    - JSON schema generation for structured output
    - Template-to-prompt translation based on GenerationTemplate enum

### Project File

- **File:** `ElBruno.AI.Evaluation.SyntheticData.csproj`
  - **Lines:** ~30
  - **Responsibilities:**
    - Package metadata
    - Dependencies: ElBruno.AI.Evaluation, Microsoft.Extensions.AI.Abstractions 9.5.0+
    - NuGet configuration

---

## 7. Integration Points with Existing Packages

### With `ElBruno.AI.Evaluation` (Core)

- **Types Used:**
  - `GoldenDataset`, `GoldenExample` — primary output
  - `IChatClient` — for LLM-powered generation
  - `IEvaluator` — future: use evaluators to score synthetic data quality
  
- **Patterns Adopted:**
  - Fluent builder (mirroring `EvaluationPipelineBuilder`)
  - Extension methods on core types
  - Task-based async API

### With `ElBruno.AI.Evaluation.Xunit` (Testing Integration)

- **Future Use:**
  - Synthetic datasets compatible with `AIEvaluationTest` attribute
  - Can be used as data sources for parameterized tests

### With `ElBruno.AI.Evaluation.Reporting` (Persistence)

- **Future Use:**
  - Generated datasets can be exported via JSON/CSV exporters
  - Can be stored in SQLite for tracking synthetic data versions

---

## 8. Dependencies & External Libraries

| Dependency | Version | Purpose |
|------------|---------|---------|
| `ElBruno.AI.Evaluation` | Latest | Core data models (GoldenDataset, GoldenExample) |
| `Microsoft.Extensions.AI.Abstractions` | 9.5.0+ | IChatClient abstraction |
| `System.Text.Json` | Included in .NET 8+ | JSON parsing for LLM responses |
| (Optional) `System.Linq.Async` | If needed | Async LINQ for streams |

---

## 9. Error Handling & Validation

### Key Exception Types

- **`InvalidOperationException`**
  - Thrown when builder is built without required components (no generator configured)
  - Thrown when LLM generation fails after max retries
  - Thrown when composite generator receives invalid weights

- **`ArgumentNullException`**
  - Thrown when required parameters are null (templates, chat clients, etc.)

- **`ArgumentException`**
  - Thrown when invalid values are provided (e.g., negative count, invalid temperature)

### Validation Strategy

- Template methods should validate inputs and throw `ArgumentNullException`
- Generators should catch LLM errors and retry per strategy; log failures
- Extension methods (`ValidateExamples`) return a list of `ValidationError` objects (non-throwing)

---

## 10. Testing Strategy

### Unit Tests (to be in `tests/` folder)

- **Builder tests:** Verify fluent API chaining, required validations
- **Generator tests:** Mock templates, verify output structure and count
- **Template tests:** Verify interpolation, null handling, tag/metadata application
- **Extension tests:** Validate augmentation, deduplication, merging
- **Validation tests:** Verify error detection logic

### Integration Tests

- **E2E:** Generate dataset → Validate → Export → Load
- **LLM integration:** Mock IChatClient responses, verify parsing and retry logic
- **Composite generation:** Verify weight-based distribution

---

## 11. Documentation & Samples

### Package Documentation

- **docs/synthetic-data-quickstart.md** — 5-minute tutorial
- **docs/synthetic-data-advanced.md** — Deep dive on each scenario
- **docs/generation-strategies.md** — Deterministic vs. LLM tradeoffs

### Sample Applications

- **samples/SyntheticDataGeneration/** — Console app demonstrating all scenarios
  - Generate Q&A, RAG, adversarial, domain-specific examples
  - Validate and deduplicate
  - Export to JSON/CSV

---

## 12. Version & Release Planning

| Version | Target | Features |
|---------|--------|----------|
| **1.0.0** | ElBruno.AI.Evaluation v1.5 | All core generators, templates, fluent builder, basic validation |
| **1.1.0** | Q3 2025 | Domain template library (healthcare, finance, legal presets) |
| **1.2.0** | Q4 2025 | Streaming generation for large-scale datasets |
| **2.0.0** | TBD | Fine-tuning integration, custom domain plugins |

---

## 13. Implementation Checklist

- [ ] Create project structure and `.csproj` file
- [ ] Implement `ISyntheticDataGenerator` interface
- [ ] Implement `DeterministicGenerator` with template interpolation
- [ ] Implement `LlmGenerator` with prompt composition and JSON parsing
- [ ] Implement `CompositeGenerator` with weighted distribution
- [ ] Implement all template classes (QA, RAG, Adversarial, Domain)
- [ ] Implement strategy configuration classes
- [ ] Implement `SyntheticDatasetBuilder` with fluent API
- [ ] Implement extension methods and validation logic
- [ ] Implement utility classes (RandomSeedProvider, PromptGenerator)
- [ ] Write comprehensive unit tests
- [ ] Write integration tests
- [ ] Create sample application
- [ ] Write documentation and README
- [ ] Publish to NuGet

---

## 14. Design Rationale

### Why Fluent Builder?
Consistency with existing `EvaluationPipelineBuilder` pattern; enables readable, chainable configuration.

### Why ISyntheticDataGenerator Interface?
Enables pluggability; different strategies (deterministic, LLM, composite) implement the same contract.

### Why IDataTemplate Hierarchy?
Separates concerns: templates define *what data to generate*, generators define *how to generate* it. Allows reuse across generators.

### Why CompositeGenerator?
Real-world datasets benefit from hybrid approaches (cost + diversity). Explicit weighting makes tradeoffs transparent.

### Why Extension Methods for Augmentation?
Non-invasive API; allows adding synthetic data generation capability to any `GoldenDataset` without modifying core types.

### Why Validation Separate from Generation?
Generation is optimistic; validation is defensive. Separating them allows different failure modes and reporting.

---

## 15. Future Enhancements (v1.5+)

- **Domain Presets:** Pre-built templates for healthcare, finance, legal, e-commerce
- **Streaming Generation:** For large-scale datasets (>10K examples)
- **Fine-Tuning Integration:** Fine-tune small models on generated data to improve quality
- **Evaluator-Driven Generation:** Use core evaluators to iteratively improve synthetic data quality
- **Prompt Template Library:** Reusable system prompts for common domains
- **Visualization:** Plot distribution of generated examples (length, complexity, diversity)

---

## End of Design Document

**Next Step:** Hand this document to implementers for phase 1 development.
