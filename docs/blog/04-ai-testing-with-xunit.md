# AI Testing in Your CI Pipeline

AI evaluation doesn't have to be separate from your normal testing workflow. ElBruno.AI.Evaluation integrates directly into xUnit—your existing test infrastructure. Both toolkits can be used in CI/CD, but with different patterns.

**ElBruno:** xUnit-native assertions. Deterministic. Fast gates. Perfect for regression detection in CI/CD.  
**Microsoft:** MSTest/xUnit compatible but more reporting-focused. Great for quality dashboards, less for quick gates.

## The AIEvaluationTest Attribute

Mark any test as an AI evaluation test:

```csharp
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Xunit;
using Xunit;

public class SupportBotTests
{
    private readonly IChatClient _chatClient;
    
    public SupportBotTests()
    {
        _chatClient = new OllamaChatClient(
            new Uri("http://localhost:11434"),
            "neural-chat"
        );
    }
    
    [Fact]
    [AIEvaluationTest(MinScore = 0.8, EvaluatorType = "Relevance")]
    public async Task SupportBot_AnswersAccountQuestions()
    {
        var input = "How do I reset my password?";
        var output = await _chatClient.CompleteAsync(input);
        
        var evaluator = new RelevanceEvaluator(threshold: 0.8);
        var result = await evaluator.EvaluateAsync(input, output);
        
        Assert.True(result.Passed);
        Assert.True(result.Score >= 0.8);
    }
}
```

The `AIEvaluationTest` attribute metadata tells the test runner (and your reports) what's being tested and the minimum acceptable score.

## Fluent Assertions with AIAssert

`AIAssert` provides clear, expressive assertion methods:

```csharp
[Fact]
public async Task SupportBot_RelevantAndFactual()
{
    var input = "What's your refund policy?";
    var expectedOutput = "30-day refunds on all purchases, no questions asked.";
    var output = await _chatClient.CompleteAsync(input);
    
    var relevance = new RelevanceEvaluator(0.7);
    var factuality = new FactualityEvaluator(0.8);
    
    var relevanceResult = await relevance.EvaluateAsync(input, output, expectedOutput);
    var factualityResult = await factuality.EvaluateAsync(input, output, expectedOutput);
    
    // Individual assertions
    AIAssert.PassesThreshold(relevanceResult, 0.7);
    AIAssert.PassesThreshold(factualityResult, 0.8);
    
    // All metrics must pass
    AIAssert.AllMetricsPass(relevanceResult);
    AIAssert.AllMetricsPass(factualityResult);
}
```

When an assertion fails, you get clear output:

```
Test failed: Evaluation score 0.62 is below threshold 0.70. 
Cosine similarity between input and output terms: 0.620. 
Input terms: 5, Output terms: 8.
```

## Test Patterns

### Pattern 1: Single Evaluator Per Test

Organize tests around evaluators:

```csharp
public class SupportBotEvaluationTests
{
    private readonly IChatClient _chatClient;
    private readonly GoldenDataset _dataset;
    
    [Theory]
    [InlineData("How do I reset my password?", "Visit Settings > Account > Reset Password.")]
    [InlineData("What's your refund policy?", "30-day refunds on all purchases.")]
    public async Task RelevanceTests(string input, string expected)
    {
        var output = await _chatClient.CompleteAsync(input);
        var evaluator = new RelevanceEvaluator(0.7);
        var result = await evaluator.EvaluateAsync(input, output, expected);
        
        AIAssert.PassesThreshold(result, 0.7);
    }
    
    [Theory]
    [InlineData("How do I reset my password?", "Visit Settings > Account > Reset Password.")]
    [InlineData("What's your refund policy?", "30-day refunds on all purchases.")]
    public async Task FactualityTests(string input, string expected)
    {
        var output = await _chatClient.CompleteAsync(input);
        var evaluator = new FactualityEvaluator(0.8);
        var result = await evaluator.EvaluateAsync(input, output, expected);
        
        AIAssert.PassesThreshold(result, 0.8);
    }
}
```

**Benefit:** Easy to see which evaluator is failing. `dotnet test --filter "Relevance" runs only relevance tests.

### Pattern 2: Golden Dataset Tests

Load examples from your golden dataset:

```csharp
public class GoldenDatasetTests : IAsyncLifetime
{
    private readonly IChatClient _chatClient;
    private GoldenDataset _dataset;
    
    public async Task InitializeAsync()
    {
        _chatClient = new OllamaChatClient(
            new Uri("http://localhost:11434"),
            "neural-chat"
        );
        
        var loader = new JsonDatasetLoader();
        _dataset = await loader.LoadAsync("support-bot.json");
    }
    
    public Task DisposeAsync() => Task.CompletedTask;
    
    [Theory]
    [MemberData(nameof(GetGoldenExamples))]
    public async Task EvaluateAgainstGoldenDataset(GoldenExample example)
    {
        var output = await _chatClient.CompleteAsync(example.Input);
        
        var evaluators = new List<IEvaluator>
        {
            new RelevanceEvaluator(0.7),
            new FactualityEvaluator(0.8),
            new SafetyEvaluator(0.95)
        };
        
        foreach (var evaluator in evaluators)
        {
            var result = await evaluator.EvaluateAsync(
                example.Input,
                output,
                example.ExpectedOutput
            );
            
            AIAssert.PassesThreshold(result, 0.7);
        }
    }
    
    public static IEnumerable<object[]> GetGoldenExamples()
    {
        var loader = new JsonDatasetLoader();
        var dataset = loader.LoadAsync("support-bot.json").Result;
        
        return dataset.Examples
            .Select(e => new object[] { e })
            .ToList();
    }
}
```

This runs one test per golden example. If your dataset has 50 examples, you get 50 test cases. Visual Studio Test Explorer shows each one:

```
✓ EvaluateAgainstGoldenDataset [0]
✓ EvaluateAgainstGoldenDataset [1]
✓ EvaluateAgainstGoldenDataset [2]
✗ EvaluateAgainstGoldenDataset [15]  ← Failed on refund question
  Score: 0.52 < threshold 0.70
```

### Pattern 3: Tagged Subset Testing

Run different evaluators for different question types:

```csharp
[Theory]
[MemberData(nameof(GetBillingExamples))]
public async Task BillingQuestionsAreFactual(GoldenExample example)
{
    var output = await _chatClient.CompleteAsync(example.Input);
    var evaluator = new FactualityEvaluator(0.9);  // High bar for billing
    var result = await evaluator.EvaluateAsync(example.Input, output, example.ExpectedOutput);
    
    AIAssert.PassesThreshold(result, 0.9);
}

[Theory]
[MemberData(nameof(GetGeneralExamples))]
public async Task GeneralQuestionsAreRelevant(GoldenExample example)
{
    var output = await _chatClient.CompleteAsync(example.Input);
    var evaluator = new RelevanceEvaluator(0.6);  // Lower bar for general questions
    var result = await evaluator.EvaluateAsync(example.Input, output);
    
    AIAssert.PassesThreshold(result, 0.6);
}

public static IEnumerable<object[]> GetBillingExamples()
{
    var loader = new JsonDatasetLoader();
    var dataset = loader.LoadAsync("support-bot.json").Result;
    return dataset.GetByTag("billing").Select(e => new object[] { e }).ToList();
}

public static IEnumerable<object[]> GetGeneralExamples()
{
    var loader = new JsonDatasetLoader();
    var dataset = loader.LoadAsync("support-bot.json").Result;
    return dataset.GetByTag("general").Select(e => new object[] { e }).ToList();
}
```

## CI/CD Integration

Your AI tests run exactly like regular tests:

**GitHub Actions (ElBruno Pattern):**

```yaml
name: AI Quality Gate
on: [push, pull_request]

jobs:
  evaluate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      
      # Run ElBruno tests (fast, offline gate)
      - run: dotnet test tests/AI.Evaluation.Tests/ --logger "trx"
      
      # If passed, optionally run Microsoft evaluations for reporting
      - name: Run Microsoft Evaluations (Optional)
        if: success()
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: dotnet run --project tests/DeepEval.Tests/
      
      - uses: dorny/test-reporter@v1
        if: always()
        with:
          name: AI Evaluation Results
          path: '**/test-results.trx'
          reporter: 'dotnet trx'
```

**Pattern:** ElBruno first (fast gate), Microsoft optional (detailed analysis).

## Test Organization

Structure your test project like this:

```
tests/
  ElBruno.AI.Evaluation.Tests/
    Evaluators/
      RelevanceEvaluatorTests.cs
      FactualityEvaluatorTests.cs
      SafetyEvaluatorTests.cs
    Integration/
      ChatbotEvaluationTests.cs
      RagEvaluationTests.cs
    Datasets/
      GoldenDatasetTests.cs
```

Each test file focuses on one scenario or evaluator.

## Debugging Failed Tests

When an AI evaluation test fails, here's how to investigate:

```csharp
[Fact]
public async Task DebugFailedEvaluation()
{
    var input = "How do I reset my password?";
    var output = await _chatClient.CompleteAsync(input);
    var expected = "Visit account settings and click reset password.";
    
    var evaluators = new List<IEvaluator>
    {
        new RelevanceEvaluator(0.7),
        new FactualityEvaluator(0.8),
        new SafetyEvaluator(0.95)
    };
    
    foreach (var evaluator in evaluators)
    {
        var result = await evaluator.EvaluateAsync(input, output, expected);
        
        Console.WriteLine($"Evaluator: {evaluator.GetType().Name}");
        Console.WriteLine($"  Score: {result.Score:F2}");
        Console.WriteLine($"  Passed: {result.Passed}");
        Console.WriteLine($"  Details: {result.Details}");
        Console.WriteLine();
        
        foreach (var (metric, score) in result.MetricScores)
        {
            Console.WriteLine($"  Metric {metric}: {score.Value:F2} (threshold: {score.Threshold:F2})");
        }
    }
}
```

xUnit captures console output, so you can see debugging info right in the test output.

## Try It Yourself

Create your first AI evaluation test:

```csharp
[Fact]
[AIEvaluationTest(MinScore = 0.7)]
public async Task MyFirstAITest()
{
    var chatClient = new SimpleChatClient(); // Use your LLM
    var input = "What is 2 + 2?";
    var output = await chatClient.CompleteAsync(input);
    
    var evaluator = new RelevanceEvaluator(0.7);
    var result = await evaluator.EvaluateAsync(input, output);
    
    AIAssert.PassesThreshold(result, 0.7);
}
```

Then run it:

```bash
dotnet test
```

You'll see:

```
Test Run Successful.
Total tests: 1
     Passed: 1
     Failed: 0
```

That's it. You now have AI quality as a first-class testing concern.

---

*Next: Build your complete production pipeline using both ElBruno and Microsoft toolkits together.*

---

## 👨‍💻 About the Author

**Bruno Capuano** is a Microsoft MVP and AI enthusiast who builds practical tools for .NET developers. This is Part 4 of a 7-part series on AI evaluation.

**🌟 Found this helpful?** Let's connect:

- 📘 [Read more on my blog](https://elbruno.com) — Deep technical articles on AI & .NET
- 🎥 [Watch video tutorials on YouTube](https://www.youtube.com/elbruno) — Demos and live coding
- 💼 [Connect on LinkedIn](https://www.linkedin.com/in/elbruno/) — Professional updates
- 🐦 [Follow on Twitter/X](https://www.x.com/elbruno/) — Quick tips and announcements
- 🎙️ [No Tiene Nombre Podcast](https://notienenombre.com) — Tech talks in Spanish
- 💻 [Explore more projects on GitHub](https://github.com/elbruno/) — Open-source AI tools

⭐ *If this series is helping you build better AI applications, give the [repo](https://github.com/elbruno/elbruno-ai-evaluation) a star and share it with your team!*
