# Evaluation Metrics Guide

This guide explains all available evaluators in ElBruno.AI.Evaluation, including what they measure, how they work, when to use them, and how to configure thresholds.

## Overview

An **evaluator** is a component that scores AI output against criteria. Each evaluator implements `IEvaluator`:

```csharp
public interface IEvaluator
{
    Task<EvaluationResult> EvaluateAsync(
        string input,
        string output,
        string? expectedOutput = null,
        CancellationToken ct = default);
}
```

### EvaluationResult

Every evaluator returns an `EvaluationResult`:

```csharp
public sealed class EvaluationResult
{
    public required double Score { get; init; }           // 0.0 to 1.0
    public required bool Passed { get; init; }            // Score >= threshold
    public string Details { get; init; }                  // Human-readable explanation
    public Dictionary<string, MetricScore> MetricScores { get; init; } // Individual metrics
}
```

---

## HallucinationEvaluator

**Detects if the AI invents facts not supported by the grounding material.**

### What It Measures

- Keyword overlap between output and grounding context (input + expectedOutput)
- Score = proportion of output tokens found in grounding material
- Identifies potentially fabricated content

### How It Works

1. Combines input and expected output as the "grounding corpus"
2. Tokenizes both corpus and output (removes short words, normalizes case)
3. Calculates overlap: `grounded tokens / total output tokens`
4. Compares against threshold (default 0.7)

### When to Use It

- **RAG applications** — Ensure responses only cite retrieved documents
- **Fact-heavy domains** — Law, medicine, finance where accuracy is critical
- **Customer support** — Prevent AI from making up policies or procedures

### Configuration

```csharp
// Default threshold is 0.7 (70% of tokens must be grounded)
var evaluator = new HallucinationEvaluator(threshold: 0.7);

// Stricter: require 80% grounding
var strict = new HallucinationEvaluator(threshold: 0.8);

// Lenient: accept 60% grounding
var lenient = new HallucinationEvaluator(threshold: 0.6);
```

### Example

```csharp
var evaluator = new HallucinationEvaluator(threshold: 0.8);

var result = await evaluator.EvaluateAsync(
    input: "What time does the store open?",
    output: "The store opens at 9 AM. We also have a secret underground vault.",
    expectedOutput: "Store hours are 9 AM to 6 PM daily."
);

// result.Score ≈ 0.75 (most tokens from output are in grounding)
// result.Passed = false (0.75 < 0.8 threshold)
// result.Details = "Keyword overlap: 6/8 tokens grounded (75%). 2 potentially hallucinated tokens."
```

### Caveats

- Uses simple keyword matching, not semantic understanding
- Common words may artificially inflate scores
- Works best with factual, keyword-dense content

---

## FactualityEvaluator

**Verifies that claims in the output are supported by expected output.**

### What It Measures

- How many claims (sentences) in the output are supported by the reference material
- Score = supported claims / total claims
- Identifies unsupported assertions

### How It Works

1. Extracts claims as sentences from output (3+ words each)
2. For each claim, calculates token overlap with reference material
3. Claim is "supported" if overlap ≥ 50%
4. Score = supported / total claims

### When to Use It

- **Content generation** — Blog posts, summaries should cite references
- **Answer systems** — Verify responses match knowledge base
- **Documentation** — Ensure generated docs align with specs

### Configuration

```csharp
// Default threshold is 0.8 (80% of claims must be supported)
var evaluator = new FactualityEvaluator(threshold: 0.8);

// Stricter: all claims must be supported
var strict = new FactualityEvaluator(threshold: 1.0);

// Lenient: 50% of claims is acceptable
var lenient = new FactualityEvaluator(threshold: 0.5);
```

### Example

```csharp
var evaluator = new FactualityEvaluator(threshold: 0.8);

var result = await evaluator.EvaluateAsync(
    input: "Tell me about apples",
    output: "Apples are fruits. Apples grow on trees. Apples are red and sweet. Apples cure cancer.",
    expectedOutput: "Apples are fruits that grow on trees. They come in red, green, and yellow varieties."
);

// Extracts 4 claims. Claims 1-3 are supported, claim 4 is not.
// result.Score = 0.75 (3/4 claims supported)
// result.Passed = false (0.75 < 0.8 threshold)
// result.Details = "3/4 claims supported (75%). Unsupported: [Apples cure cancer.]"
```

---

## RelevanceEvaluator

**Measures if the output directly addresses the input query.**

### What It Measures

- Semantic similarity between input question and output response
- Uses cosine similarity on term frequency vectors
- Score = semantic overlap between input and output

### How It Works

1. Extracts meaningful terms (3+ characters) from input and output
2. Counts term frequency in each
3. Calculates cosine similarity of frequency vectors
4. Compares against threshold (default 0.6)

### When to Use It

- **Question-answering** — Ensure answers relate to questions asked
- **Conversational AI** — Detect off-topic or irrelevant responses
- **Customer support** — Verify bot addresses customer concerns

### Configuration

```csharp
// Default threshold is 0.6 (moderate relevance)
var evaluator = new RelevanceEvaluator(threshold: 0.6);

// Strict: only accept highly relevant responses
var strict = new RelevanceEvaluator(threshold: 0.8);

// Lenient: accept loosely related responses
var lenient = new RelevanceEvaluator(threshold: 0.4);
```

### Example

```csharp
var evaluator = new RelevanceEvaluator(threshold: 0.6);

var result = await evaluator.EvaluateAsync(
    input: "What features does your API have?",
    output: "Our API supports REST, GraphQL, real-time webhooks, and automatic rate limiting."
);

// Input and output share terms: api, features
// result.Score ≈ 0.72 (good term overlap)
// result.Passed = true (0.72 >= 0.6 threshold)
// result.Details = "Cosine similarity between input and output terms: 0.720. Input terms: 6, Output terms: 8."
```

---

## CoherenceEvaluator

**Checks if output is well-structured with logical flow and no contradictions.**

### What It Measures

1. **Sentence completeness** — Each sentence should have 3+ words
2. **Contradictions** — Detects opposing claims (e.g., "is" and "is not")
3. **Excessive repetition** — Flags >30% sentence repetition

### How It Works

1. Splits output into sentences
2. Checks each sentence for minimum length (penalties for short sentences)
3. Scans for contradiction pairs (is/is not, yes/no, always/never, etc.)
4. Measures distinct vs. total sentences
5. Score starts at 1.0 and deducts points for each issue

### When to Use It

- **Long-form generation** — Essays, reports, documentation
- **Chatbot responses** — Detect rambling or self-contradictory replies
- **Content quality** — Ensure output reads naturally

### Configuration

```csharp
// Default threshold is 0.7 (70% coherence)
var evaluator = new CoherenceEvaluator(threshold: 0.7);

// Strict: near-perfect coherence required
var strict = new CoherenceEvaluator(threshold: 0.85);

// Lenient: accept somewhat incoherent output
var lenient = new CoherenceEvaluator(threshold: 0.5);
```

### Example

```csharp
var evaluator = new CoherenceEvaluator(threshold: 0.7);

var result = await evaluator.EvaluateAsync(
    input: "Summarize the weather",
    output: "It is sunny. It is not sunny. It is sunny. It is sunny."
);

// Detects: 1 contradiction (is/is not), high repetition (25% distinct sentences)
// result.Score ≈ 0.65 (1.0 - 0.1 for contradiction - 0.25 for repetition penalty)
// result.Passed = false (0.65 < 0.7 threshold)
// result.Details = "Issues: 1 potential contradiction(s), high repetition (75%). Final score: 0.65."
```

### Scoring Details

- **Incomplete sentences** (-0.15 per sentence): Sentences < 3 words
- **Contradictions** (-0.1 each): Detected opposite claim pairs
- **Repetition penalty** (-0.2): If >30% of sentences repeat

---

## SafetyEvaluator

**Detects unsafe content including profanity, PII, and potentially harmful instructions.**

### What It Measures

1. **Profanity blocklist** — Default list includes common inappropriate terms
2. **Personally Identifiable Information (PII)**:
   - Email addresses (user@example.com)
   - Social Security Numbers (XXX-XX-XXXX)
   - Phone numbers (XXX-XXX-XXXX)
3. **Custom blocklist** — Add domain-specific unsafe terms

### How It Works

1. Checks output against blocklist (case-insensitive)
2. Scans for PII patterns (regex-based)
3. Calculates penalty per violation: `score = 1.0 - (violations * 0.15)`
4. Clamps to [0.0, 1.0] range

### When to Use It

- **Customer-facing AI** — Prevent inappropriate language
- **Data protection** — Ensure PII is not leaked in responses
- **Compliance** — Meet safety/moderation requirements
- **Production systems** — Always include as first-line filter

### Configuration

```csharp
// Default threshold is 0.9 (very strict)
var evaluator = new SafetyEvaluator(threshold: 0.9);

// With custom blocklist
var blocklist = new[] { "custom_bad_word", "jargon_to_block" };
var custom = new SafetyEvaluator(threshold: 0.9, additionalBlocklist: blocklist);

// Lenient: allow minor violations
var lenient = new SafetyEvaluator(threshold: 0.7);
```

### Example

```csharp
var evaluator = new SafetyEvaluator(threshold: 0.9);

var result = await evaluator.EvaluateAsync(
    input: "Reply politely",
    output: "Sure! You can reach me at john@example.com or 555-123-4567."
);

// Detects: 1 email, 1 phone number (2 violations)
// result.Score = 1.0 - (2 * 0.15) = 0.7
// result.Passed = false (0.7 < 0.9 threshold)
// result.Details = "2 violation(s): Email address detected; Phone number detected."
```

### Default Blocklist

The evaluator includes these terms by default:
- Common profanities (7 items)
- Expandable via `additionalBlocklist` parameter

---

## Creating Custom Evaluators

Implement `IEvaluator` to create domain-specific evaluators:

```csharp
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Metrics;

public sealed class CustomEvaluator : IEvaluator
{
    private readonly double _threshold;

    public CustomEvaluator(double threshold = 0.8)
    {
        _threshold = threshold;
    }

    public Task<EvaluationResult> EvaluateAsync(
        string input,
        string output,
        string? expectedOutput = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Your custom evaluation logic here
        double score = EvaluateOutput(output);

        return Task.FromResult(new EvaluationResult
        {
            Score = Math.Clamp(score, 0.0, 1.0),
            Passed = score >= _threshold,
            Details = $"Custom evaluation: {score:P0}",
            MetricScores = new()
            {
                ["custom"] = new MetricScore 
                { 
                    Name = "Custom Metric", 
                    Value = score, 
                    Threshold = _threshold 
                }
            }
        });
    }

    private double EvaluateOutput(string output)
    {
        // Implement your logic
        if (output.Length < 10) return 0.0;
        if (output.Contains("ERROR")) return 0.0;
        return 0.9;
    }
}
```

---

## Combining Evaluators

Use multiple evaluators to get comprehensive coverage:

```csharp
var evaluators = new IEvaluator[]
{
    // Safety is always first
    new SafetyEvaluator(threshold: 0.95),
    
    // Correctness metrics
    new RelevanceEvaluator(threshold: 0.65),
    new FactualityEvaluator(threshold: 0.80),
    new HallucinationEvaluator(threshold: 0.70),
    
    // Quality metrics
    new CoherenceEvaluator(threshold: 0.70),
    
    // Custom domain logic
    new CustomEvaluator(threshold: 0.85)
};

var result = await chatClient.EvaluateAsync(example, evaluators);

// Score = average of all evaluators
Console.WriteLine($"Overall: {result.Score:P0}");

// Details from all evaluators combined
Console.WriteLine($"Issues: {result.Details}");

// Individual metric scores
foreach (var (metric, score) in result.MetricScores)
    Console.WriteLine($"  {metric}: {score.Value:P0}");
```

---

## Threshold Recommendations

| Evaluator | Conservative | Balanced | Lenient |
|-----------|--------------|----------|---------|
| **Hallucination** | 0.85 | 0.70 | 0.60 |
| **Factuality** | 0.90 | 0.80 | 0.70 |
| **Relevance** | 0.75 | 0.60 | 0.45 |
| **Coherence** | 0.85 | 0.70 | 0.55 |
| **Safety** | 0.95 | 0.90 | 0.80 |

Use **Conservative** for:
- High-stakes applications (medical, legal, financial)
- Production systems with SLA requirements
- Regulatory compliance scenarios

Use **Balanced** for:
- Most applications
- Reasonable quality vs. false-positive trade-off
- General-purpose chatbots

Use **Lenient** for:
- Development/testing environments
- Exploratory applications
- High-volume, low-stakes interactions

---

## Understanding MetricScore

Each evaluator populates `MetricScores` in the result:

```csharp
public sealed class MetricScore
{
    public required string Name { get; init; }        // Human-readable name
    public required double Value { get; init; }       // Score (0.0 to 1.0)
    public double Threshold { get; init; }            // Passing threshold
    public bool Passed => Value >= Threshold;         // Computed property
}
```

Access individual metric details:

```csharp
var result = await evaluator.EvaluateAsync(input, output, expected);

foreach (var (metricKey, metric) in result.MetricScores)
{
    Console.WriteLine($"{metric.Name}: {metric.Value:P0}");
    Console.WriteLine($"  Threshold: {metric.Threshold:P0}");
    Console.WriteLine($"  Passed: {metric.Passed}");
}
```
