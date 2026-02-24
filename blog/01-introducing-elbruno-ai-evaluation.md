# Introducing ElBruno.AI.Evaluation: AI Testing for .NET Developers

## The Gap: Why We Built This

You've probably felt it. Your LLM application works great in demos—clean examples, perfect conditions. But the moment it hits production, something goes wrong. A prompt generates off-topic content. The AI hallucinates facts. Performance degrades in ways you didn't anticipate.

The Python ecosystem has had tools like Ragas and DeepEval for years, letting teams measure AI quality systematically. .NET developers? We've been flying blind.

**ElBruno.AI.Evaluation** closes that gap. It's a production-ready toolkit for testing, evaluating, and observing AI applications built with .NET. Three NuGet packages. Five built-in evaluators. Real metrics that matter.

## What You Get

**ElBruno.AI.Evaluation** (Core Library)
- Five evaluators out of the box: Relevance, Factuality, Coherence, Hallucination, Safety
- Fluent API for building evaluation pipelines
- Support for golden datasets (your ground truth)
- Integration with Microsoft.Extensions.AI

**ElBruno.AI.Evaluation.Xunit**
- `AIEvaluationTest` attribute for xUnit
- Fluent assertions (`AIAssert`) for evaluation results
- First-class AI testing in your test suite

**ElBruno.AI.Evaluation.Reporting**
- SQLite persistence for tracking quality over time
- Baseline snapshots and regression detection
- Export to JSON, CSV, or console
- Cost and token tracking

## A Quick Demo

Let's build a simple chatbot evaluation in 5 minutes.

### Step 1: Install the NuGet Packages

```bash
dotnet add package ElBruno.AI.Evaluation
dotnet add package ElBruno.AI.Evaluation.Xunit
dotnet add package ElBruno.AI.Evaluation.Reporting
```

### Step 2: Create a Golden Dataset

A golden dataset is your ground truth. It's a collection of input-output pairs that define correct behavior.

```csharp
var dataset = new GoldenDataset
{
    Name = "Support Bot",
    Version = "1.0.0",
    Description = "Golden examples for support bot evaluation"
};

dataset.AddExample(new GoldenExample
{
    Input = "How do I reset my password?",
    ExpectedOutput = "Visit the account settings page and click 'Reset Password'. You'll receive an email with instructions.",
    Tags = new() { "onboarding", "account" }
});

dataset.AddExample(new GoldenExample
{
    Input = "What's your refund policy?",
    ExpectedOutput = "We offer 30-day refunds on all purchases, no questions asked.",
    Tags = new() { "billing", "policy" }
});

var loader = new JsonDatasetLoader();
await loader.SaveAsync(dataset, "support-dataset.json");
```

### Step 3: Set Up Your Chat Client

```csharp
using Microsoft.Extensions.AI;
using ElBruno.AI.Evaluation;

// Use your favorite LLM provider (OpenAI, Anthropic, etc.)
var chatClient = new OllamaChatClient(
    new Uri("http://localhost:11434"),
    "neural-chat"
);
```

### Step 4: Build an Evaluation Pipeline

```csharp
var evaluators = new List<IEvaluator>
{
    new RelevanceEvaluator(threshold: 0.6),
    new FactualityEvaluator(threshold: 0.8),
    new SafetyEvaluator(threshold: 0.9)
};

var pipeline = new EvaluationPipelineBuilder()
    .WithChatClient(chatClient)
    .WithDataset(dataset)
    .AddEvaluator(evaluators[0])
    .AddEvaluator(evaluators[1])
    .AddEvaluator(evaluators[2])
    .Build();

var run = await pipeline.RunAsync();
```

### Step 5: See the Results

```csharp
Console.WriteLine($"Run completed: {run.Results.Count} examples evaluated");

foreach (var result in run.Results)
{
    Console.WriteLine($"  Overall Score: {result.Score:F2} [{(result.Passed ? "PASS" : "FAIL")}]");
    Console.WriteLine($"  Details: {result.Details}");
    
    foreach (var metric in result.MetricScores)
    {
        Console.WriteLine($"    {metric.Key}: {metric.Value.Value:F2}");
    }
}
```

## How It Compares to Python

If you're familiar with Ragas or DeepEval, here's the mapping:

| Ragas | DeepEval | ElBruno.AI.Evaluation |
|-------|----------|----------------------|
| Answer Relevance | Answer Relevance | `RelevanceEvaluator` |
| Factuality | Factuality | `FactualityEvaluator` |
| Coherence | Coherence | `CoherenceEvaluator` |
| Hallucination | Hallucination | `HallucinationEvaluator` |
| — | Toxic Input | `SafetyEvaluator` |

The main difference? **ElBruno.AI.Evaluation is built for .NET from the ground up**. It uses C# async/await patterns, integrates with Microsoft.Extensions.AI, and works with your existing xUnit test infrastructure. No Python interop. No dependency bloat.

## What's Next

This is the beginning of a series on AI testing in .NET:

1. **Introducing ElBruno.AI.Evaluation** (this post) — Overview and quick start
2. **Golden Datasets** — Creating, versioning, and organizing ground truth data
3. **AI Evaluators Deep Dive** — Each evaluator explained with real examples
4. **AI Testing with xUnit** — First-class AI testing in your test suite
5. **From Demo to Production** — Observability, baselines, and regression detection

## Try It Yourself

Ready to test your first LLM application? Head to the [GitHub repository](https://github.com/elbruno/ElBruno.AI.Evaluation) for complete samples and documentation. The `ChatbotEvaluation` and `RagEvaluation` samples show real-world patterns.

Start small: create a 5-example golden dataset, run your LLM against it, and see which evaluators matter most for your use case. You'll be surprised how quickly you can build confidence in AI quality.

---

*ElBruno.AI.Evaluation is open source and actively maintained. Questions? Feedback? Open an issue on GitHub or reach out on social media.*
