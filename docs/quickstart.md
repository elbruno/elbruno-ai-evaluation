# Your First AI Test in 5 Minutes

Learn how to write and run your first AI evaluation in ElBruno.AI.Evaluation.

## Installation

Add the NuGet package to your .NET project:

```bash
dotnet add package ElBruno.AI.Evaluation
```

If you're using xUnit, also install the testing integration:

```bash
dotnet add package ElBruno.AI.Evaluation.Xunit
```

## Create Your First Golden Dataset

A golden dataset is a JSON file containing test cases—inputs and expected outputs that serve as ground truth.

Create a file called `evaluation-dataset.json`:

```json
{
  "name": "Customer Support Evaluation",
  "version": "1.0.0",
  "description": "Test cases for customer support chatbot quality",
  "createdAt": "2025-01-01T00:00:00Z",
  "tags": ["support", "chatbot"],
  "examples": [
    {
      "input": "How do I reset my password?",
      "expectedOutput": "To reset your password, visit the login page and click 'Forgot Password'. Enter your email address and follow the verification steps. You'll receive a link to create a new password.",
      "context": "Password reset documentation",
      "tags": ["account", "password"]
    },
    {
      "input": "What are your business hours?",
      "expectedOutput": "We're open Monday through Friday, 9 AM to 6 PM EST. Weekend support is available via email only.",
      "context": "Company operating hours",
      "tags": ["hours", "contact"]
    }
  ]
}
```

## Write Your First Evaluation Test

> 💡 **New to AI testing?** Check out the [complete blog series](../blog/01-introducing-elbruno-ai-evaluation.md) for a guided developer journey from basics to production!

Here's a simple console app that evaluates an AI model against your golden dataset:

```csharp
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using Microsoft.Extensions.AI;

// 1. Load your golden dataset
var loader = new JsonDatasetLoader();
var dataset = await loader.LoadAsync("evaluation-dataset.json");

Console.WriteLine($"📚 Loaded dataset: {dataset.Name} with {dataset.Examples.Count} examples\n");

// 2. Create evaluators (customize thresholds as needed)
var evaluators = new IEvaluator[]
{
    new RelevanceEvaluator(threshold: 0.6),
    new FactualityEvaluator(threshold: 0.8),
    new CoherenceEvaluator(threshold: 0.7)
};

// 3. Create your AI chat client (example using a mock for demo)
// In production, use: var chatClient = new OpenAIClient(...);
var chatClient = new MockChatClient();

// 4. Run evaluation against the entire dataset
var run = await chatClient.EvaluateAsync(dataset, evaluators);

Console.WriteLine($"🧪 Evaluation Results");
Console.WriteLine($"=====================");
Console.WriteLine($"Total examples: {run.Results.Count}");
Console.WriteLine($"Passed: {run.Results.Count(r => r.Passed)}/{run.Results.Count}\n");

// 5. View detailed results
foreach (var (index, result) in run.Results.Select((r, i) => (i, r)))
{
    Console.WriteLine($"Example {index + 1}: {(result.Passed ? "✅ PASS" : "❌ FAIL")}");
    Console.WriteLine($"  Score: {result.Score:P0}");
    Console.WriteLine($"  Details: {result.Details}\n");
}

// Mock chat client for demonstration
class MockChatClient : IChatClient
{
    public async Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        // Simulate a realistic response
        var userMessage = chatMessages.FirstOrDefault(m => m.Role == ChatRole.User)?.Content ?? "unknown";
        var mockResponse = $"Responding to: {userMessage}";
        
        return new ChatCompletion { Content = [new TextContent(mockResponse)] };
    }

    public IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
```

## Run It

```bash
dotnet run
```

You'll see output like:

```
📚 Loaded dataset: Customer Support Evaluation with 2 examples

🧪 Evaluation Results
=====================
Total examples: 2
Passed: 1/2

Example 1: ✅ PASS
  Score: 85%
  Details: Cosine similarity between input and output terms: 0.850. Input terms: 4, Output terms: 12.

Example 2: ❌ FAIL
  Score: 45%
  Details: Cosine similarity between input and output terms: 0.450. Input terms: 5, Output terms: 8.
```

## What's Next?

- **Read evaluation metrics guide** — Learn what each evaluator measures and when to use it
- **Explore dataset management** — Understand versioning and best practices for golden datasets
- **Try xUnit integration** — Use `AIAssert` to write fluent evaluation assertions
- **View samples** — Check out ChatbotEvaluation and RagEvaluation in the samples/ directory
- **Set up regression testing** — Compare baseline snapshots to detect quality regressions

## Key Concepts

| Term | Meaning |
|------|---------|
| **Golden Dataset** | JSON file with ground-truth test cases (inputs and expected outputs) |
| **Evaluator** | Component that scores AI output (e.g., `RelevanceEvaluator`) |
| **Threshold** | Minimum score (0-1) required to pass an evaluation |
| **EvaluationResult** | Contains `Score`, `Passed`, and `Details` for a single evaluation |
| **EvaluationRun** | Results from evaluating an entire dataset |

## Need Help?

- Check the evaluation-metrics.md guide for detailed evaluator documentation
- Review best-practices.md for common testing patterns
- Explore the samples/ directory for complete working examples
