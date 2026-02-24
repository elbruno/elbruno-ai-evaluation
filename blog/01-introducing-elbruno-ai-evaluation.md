# Testing AI in .NET: The Landscape

## The Challenge

You're building an AI app in .NET. Your chatbot works great in demos—clean examples, perfect conditions. But production? That's where things get messy. Prompts generate off-topic content. The AI hallucinates facts. Performance degrades in unexpected ways. You need to measure and improve quality systematically.

The Python ecosystem has had frameworks like Ragas and DeepEval for years. .NET developers? Until now, you've had two choices: build it yourself or reach for Python interop. Neither is ideal.

## Two Toolkits, One Goal

This series explores **two complementary evaluation frameworks** for .NET:

1. **Microsoft.Extensions.AI.Evaluation** — Microsoft's official, enterprise-grade framework. LLM-powered evaluators for nuanced quality judgment, agent evaluation, professional reporting, Azure integration.
2. **ElBruno.AI.Evaluation** — A lightweight, deterministic alternative. Offline evaluation, golden dataset versioning, synthetic data generation, xUnit integration, zero external dependencies.

Neither is "better"—they solve different problems. Microsoft excels at sophisticated quality analysis. ElBruno excels at offline scenarios, test automation, and dataset management. **The real power is using both together.**

This series guides your journey: from "I need to test my AI app" to "I have a production evaluation pipeline."

## When to Use Which: Decision Tree

```
Need LLM-powered quality judgment?
  → YES: Use Microsoft (Relevance, Completeness, Fluency, Groundedness)
  → NO: Next question

Need agentic evaluators (IntentResolution, TaskAdherence)?
  → YES: Use Microsoft
  → NO: Next question

Need offline/air-gapped evaluation?
  → YES: Use ElBruno (+ Microsoft for non-safety)
  → NO: Next question

Need synthetic data or golden dataset versioning?
  → YES: Use ElBruno
  → NO: Next question

Need regression detection in CI/CD?
  → YES: Use ElBruno
  → NO: Use either; Microsoft for comprehensiveness
```

**Quick Comparison:**

| Scenario | Best Choice |
|----------|---|
| "I need fast, local eval in CI/CD" | ElBruno |
| "I need nuanced quality judgment" | Microsoft |
| "I'm offline or air-gapped" | ElBruno |
| "I need to generate test data" | ElBruno |
| "I need comprehensive safety analysis" | Microsoft |
| "I want xUnit-native testing" | ElBruno |
| "I need professional HTML reports" | Microsoft |
| "I'm cost-conscious" | ElBruno |

## A Quick Demo

Let's build a simple evaluation in 5 minutes. This example uses ElBruno, but Microsoft's approach is similar.

### Step 1: Install the Packages

```bash
# ElBruno for offline, xUnit-native evaluation
dotnet add package ElBruno.AI.Evaluation
dotnet add package ElBruno.AI.Evaluation.Xunit
dotnet add package ElBruno.AI.Evaluation.Reporting

# OR: Microsoft for LLM-powered evaluation
dotnet add package Microsoft.Extensions.AI.Evaluation
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

## How They Compare

**Microsoft.Extensions.AI.Evaluation** (Official)

- ✅ LLM-powered evaluators (Relevance, Completeness, Fluency, Groundedness, etc.)
- ✅ Agent-focused evaluators (IntentResolution, TaskAdherence, ToolCallAccuracy)
- ✅ Azure AI Foundry safety analysis
- ✅ HTML report generation
- ❌ Requires external LLM calls
- ❌ No golden dataset management
- ❌ No xUnit integration

**ElBruno.AI.Evaluation** (Complementary)

- ✅ Deterministic evaluators (Hallucination, Factuality, Relevance, Coherence, Safety)
- ✅ Golden dataset versioning and management
- ✅ Synthetic data generation
- ✅ xUnit-native test integration
- ✅ Offline, zero external dependencies
- ❌ Not LLM-powered
- ❌ Simpler, less nuanced than Microsoft's evaluators

## What's Next

This is the beginning of a developer journey through AI testing in .NET:

1. **Testing AI in .NET: The Landscape** (this post) — Overview of both toolkits and when to use each
2. **Building Your Test Foundation: Golden Datasets & Synthetic Data** — Preparing test data with ElBruno
3. **Evaluators: From Quick Checks to Deep Analysis** — Layering ElBruno's deterministic evaluators with Microsoft's LLM-powered ones
4. **AI Testing in Your CI Pipeline** — Integrating both toolkits into automated tests
5. **Production AI Evaluation: Combining Both Toolkits** — End-to-end pipeline using both frameworks together
6. **Generating Synthetic Test Data for AI Evaluation** (NEW) — Deep dive into ElBruno's synthetic data generation
7. **A Guide to Choosing the Right Evaluators for Your AI App** (NEW) — Evaluator selection by scenario

**Recommendation:** Start with ElBruno for fast iteration, deterministic baselines, and test automation. Graduate to Microsoft when you need nuanced quality judgment or advanced safety analysis. Use both in your production pipeline.

## Try It Yourself

Ready to test your first LLM application? Head to the [GitHub repository](https://github.com/elbruno/ElBruno.AI.Evaluation) for complete samples and documentation. The `ChatbotEvaluation` and `RagEvaluation` samples show real-world patterns.

Start small: create a 5-example golden dataset, run your LLM against it, and see which evaluators matter most for your use case. You'll be surprised how quickly you can build confidence in AI quality.

---

*ElBruno.AI.Evaluation is open source and actively maintained. Questions? Feedback? Open an issue on GitHub or reach out on social media.*

---

## 👨‍💻 About the Author

**Bruno Capuano** is a Microsoft MVP and AI enthusiast who builds practical tools for .NET developers. This blog is part of a 7-part series on AI evaluation.

**🌟 Found this helpful?** Let's connect:

- 📘 [Read more on my blog](https://elbruno.com) — Deep technical articles on AI & .NET
- 🎥 [Watch video tutorials on YouTube](https://www.youtube.com/elbruno) — Demos and live coding
- 💼 [Connect on LinkedIn](https://www.linkedin.com/in/elbruno/) — Professional updates
- 🐦 [Follow on Twitter/X](https://www.x.com/elbruno/) — Quick tips and announcements
- 🎙️ [No Tiene Nombre Podcast](https://notienenombre.com) — Tech talks in Spanish
- 💻 [Explore more projects on GitHub](https://github.com/elbruno/) — Open-source AI tools

⭐ *If this series is helping you build better AI applications, give the [repo](https://github.com/elbruno/elbruno-ai-evaluation) a star and share it with your team!*
