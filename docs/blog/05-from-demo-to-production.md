# Production AI Evaluation: Combining Both Toolkits

You've learned each toolkit separately. Now let's build a **complete production pipeline** using both together. Neither toolkitalone is sufficient—the real power comes from using them in combination.

## The Hybrid Pattern

```
Input Data
    ↓
[ElBruno Synthetic Generator] → Creates test data if needed
    ↓
[ElBruno Deterministic Evals] → Fast offline gate (CI/CD)
    ↓
    IF PASS: [Optional: Microsoft LLM Evals] → Deep analysis for reporting
    ↓
[ElBruno Baseline Snapshot] → Detect regressions
    ↓
[Production Deployment]
```

**Layers:**

1. **Data layer:** ElBruno generates or manages golden datasets
2. **Quality gate:** ElBruno's deterministic evaluators (fast, offline)
3. **Deep analysis:** Microsoft's LLM evaluators (optional, for insights)
4. **Regression:** ElBruno's baseline snapshots (CI/CD safety net)
5. **Reporting:** ElBruno's SQLite + exports, Microsoft's HTML reports

## Complete Production Workflow

```csharp
public class ProductionEvaluationPipeline
{
    private readonly IChatClient _chatClient;
    private readonly GoldenDataset _dataset;
    private readonly BaselineSnapshot _baseline;
    private readonly SqliteResultStore _store;
    private readonly IMonitoringClient _monitoring;
    
    public async Task<DeploymentGate> EvaluateBeforeDeployAsync(string newModelVersion)
    {
        // Step 1: Generate or load test data
        var testData = await LoadOrGenerateDatasetAsync();
        
        // Step 2: Run ElBruno deterministic evaluators (FAST, OFFLINE)
        var elbrunoEvaluators = new List<IEvaluator>
        {
            new RelevanceEvaluator(0.7),
            new HallucinationEvaluator(0.75),
            new SafetyEvaluator(0.95),
            new CoherenceEvaluator(0.7),
            new FactualityEvaluator(0.8)
        };
        
        var elbrunoPipeline = new EvaluationPipelineBuilder()
            .WithChatClient(_chatClient)
            .WithDataset(testData)
            .ForEach(elbrunoEvaluators, e => elbrunoPipeline.AddEvaluator(e))
            .WithBaseline(_baseline)
            .Build();
        
        var regressionReport = await elbrunoPipeline.RunWithBaselineAsync();
        
        // Step 3: Store results
        await _store.SaveAsync(new EvaluationRun
        {
            ModelVersion = newModelVersion,
            Results = regressionReport.Results,
            RegressionDetected = regressionReport.HasRegressions,
            StartedAt = DateTime.UtcNow
        });
        
        // Step 4: Check gate—fail if regression
        if (regressionReport.HasRegressions)
        {
            Console.WriteLine("❌ REGRESSION DETECTED");
            foreach (var detail in regressionReport.RegressionDetails)
            {
                Console.WriteLine($"  {detail.MetricName}: {detail.BaselineValue:F2} → {detail.CurrentValue:F2}");
            }
            return DeploymentGate.Blocked;
        }
        
        // Step 5 (Optional): For critical releases, run Microsoft's LLM evaluators
        if (ShouldRunDeepEvaluation(newModelVersion))
        {
            var deepResults = await RunMicrosoftEvaluatorsAsync(testData);
            // Generate comprehensive HTML report for stakeholders
            await GenerateMicrosoftReportAsync(deepResults);
        }
        
        // Step 6: Record metrics for monitoring
        await RecordProductionMetricsAsync(regressionReport);
        
        Console.WriteLine("✅ ALL GATES PASSED");
        return DeploymentGate.Approved;
    }
    
    private async Task RecordProductionMetricsAsync(RegressionReport report)
    {
        var metrics = new Dictionary<string, double>
        {
            ["ai.relevance.avg"] = report.Results.Average(r => r.MetricScores["relevance"].Value),
            ["ai.hallucination.avg"] = report.Results.Average(r => r.MetricScores["hallucination"].Value),
            ["ai.safety.min"] = report.Results.Min(r => r.MetricScores["safety"].Value),
            ["ai.pass_rate"] = report.Results.Count(r => r.Passed) / (double)report.Results.Count
        };
        
        // Send to DataDog, Prometheus, CloudWatch, etc.
        await _monitoring.RecordMetricsAsync(metrics);
    }
    
    private bool ShouldRunDeepEvaluation(string version)
    {
        // Run deep eval for major releases or if explicitly requested
        return version.Contains(".0.0") || Environment.GetEnvironmentVariable("DEEP_EVAL") == "true";
    }
    
    private async Task<List<EvaluationResult>> RunMicrosoftEvaluatorsAsync(GoldenDataset dataset)
    {
        // Use Microsoft's LLM-powered evaluators
        var evaluators = new[]
        {
            new MicrosoftRelevanceEvaluator(),
            new MicrosoftCompletenessEvaluator(),
            new MicrosoftGroundednessEvaluator()
        };
        
        var results = new List<EvaluationResult>();
        foreach (var example in dataset.Examples)
        {
            var output = await _chatClient.CompleteAsync(example.Input);
            foreach (var evaluator in evaluators)
            {
                var result = await evaluator.EvaluateAsync(
                    new[] { new ChatMessage(ChatRole.User, example.Input) },
                    new ChatResponse(output),
                    null,
                    null
                );
                results.Add(result);
            }
        }
        
        return results;
    }
}

// Usage
var pipeline = new ProductionEvaluationPipeline(
    chatClient,
    dataset,
    baseline,
    resultStore,
    monitoring
);

var gate = await pipeline.EvaluateBeforeDeployAsync("v2.3.0");

if (gate == DeploymentGate.Approved)
{
    await DeployToProductionAsync();
}
else
{
    throw new Exception("Deployment blocked due to quality issues");
}
```

## Cost Tracking

Both toolkits support cost monitoring:

```csharp
// ElBruno: Track tokens used by deterministic evaluators
var run = await elbrunoPipeline.RunAsync();
Console.WriteLine($"Tokens used: {run.TokensUsed}");
Console.WriteLine($"Estimated cost: ${run.EstimatedCost:F4}");

// Microsoft: Built-in token counting
var config = new ChatConfiguration { MaxTokens = 1000 };
// Token counting happens automatically during evaluation

// Store trends
SELECT 
  DATE(created_at) as day,
  AVG(tokens_used) as avg_tokens,
  SUM(cost_dollars) as total_cost
FROM evaluation_runs
GROUP BY DATE(created_at)
ORDER BY created_at DESC;
```

## Key Differences

| Aspect | ElBruno | Microsoft |
|--------|---------|-----------|
| **Speed** | Milliseconds (offline) | Seconds (LLM calls) |
| **Cost** | Negligible | $0.01-0.10 per eval |
| **Regression Detection** | Native (via baseline snapshots) | Manual comparison |
| **Synthetic Data** | Native (templates + LLM) | Not provided |
| **Persistence** | SQLite (local) | Azure Storage (cloud) |
| **Reporting** | JSON/CSV exports | HTML dashboards |
| **Use in CI/CD** | Perfect for gating | Better for dashboards |

```csharp
using ElBruno.AI.Evaluation.Reporting;
using ElBruno.AI.Evaluation.Evaluators;

var store = new SqliteResultStore("evaluations.db");

var evaluator = new RelevanceEvaluator(0.7);
var result = await evaluator.EvaluateAsync(
    input: "How do I reset my password?",
    output: "Visit account settings and click reset password.",
    expectedOutput: "Visit Settings > Account > Reset Password."
);

// Save to database
await store.SaveAsync(result);
```

Now every evaluation is recorded. You can query later:

```sql
-- All evaluations from today
SELECT score, passed, details FROM evaluations WHERE date(created_at) = date('now');

-- Average score by evaluator type
SELECT evaluator_type, AVG(score) as avg_score FROM evaluations GROUP BY evaluator_type;

-- Failed evaluations (find problem areas)
SELECT * FROM evaluations WHERE passed = 0 ORDER BY created_at DESC LIMIT 10;
```

## Baseline Snapshots

A **baseline snapshot** is a record of quality at a point in time. Use it to detect regressions:

```csharp
using ElBruno.AI.Evaluation.Metrics;

// Create a baseline from the current run
var baseline = new BaselineSnapshot
{
    Name = "Production v2.1.0",
    CreatedAt = DateTimeOffset.UtcNow,
    Metrics = new Dictionary<string, double>
    {
        ["relevance_avg"] = 0.82,
        ["factuality_avg"] = 0.87,
        ["safety_min"] = 0.95,
        ["hallucination_avg"] = 0.79
    }
};

// Later, compare new run against baseline
var pipeline = new EvaluationPipelineBuilder()
    .WithChatClient(chatClient)
    .WithDataset(dataset)
    .AddEvaluator(new RelevanceEvaluator())
    .AddEvaluator(new FactualityEvaluator())
    .AddEvaluator(new SafetyEvaluator())
    .WithBaseline(baseline)
    .Build();

var regression = await pipeline.RunWithBaselineAsync();

// Check for quality drops
if (regression.HasRegressions)
{
    foreach (var issue in regression.RegressionDetails)
    {
        Console.WriteLine($"⚠️ {issue.MetricName}: {issue.BaselineValue:F2} → {issue.CurrentValue:F2}");
    }
    
    throw new InvalidOperationException("Quality regression detected!");
}

Console.WriteLine("✅ All metrics passed baseline checks");
```

## Reporting and Export

Once evaluations are stored, export for analysis:

**Console Reporter:**

```csharp
var reporter = new ConsoleReporter();
var run = await pipeline.RunAsync();
reporter.Report(run);

// Output:
// Evaluation Run Summary
// =====================
// Total Examples: 42
// Passed: 38
// Failed: 4
// Pass Rate: 90.48%
//
// Relevance:    avg=0.82, min=0.65, max=0.99
// Factuality:   avg=0.87, min=0.52, max=1.00
// Safety:       avg=0.98, min=0.90, max=1.00
// Hallucination: avg=0.79, min=0.45, max=0.96
```

**JSON Export:**

```csharp
var exporter = new JsonExporter();
var json = await exporter.ExportAsync(run);
await File.WriteAllTextAsync("evaluation-results.json", json);

// Output: evaluation-results.json
{
  "timestamp": "2025-02-23T15:45:00Z",
  "dataset": "Support Bot",
  "examples_evaluated": 42,
  "results": [
    {
      "input": "How do I reset my password?",
      "output": "Visit account settings...",
      "overall_score": 0.89,
      "passed": true,
      "metrics": {
        "relevance": 0.85,
        "factuality": 0.92,
        "safety": 1.00
      }
    }
  ],
  "summary": {
    "pass_rate": 0.904,
    "avg_score": 0.86
  }
}
```

**CSV Export:**

```csharp
var csvExporter = new CsvExporter();
var csv = await csvExporter.ExportAsync(run);
await File.WriteAllTextAsync("evaluation-results.csv", csv);

// Use in Excel/Sheets for charts, pivot tables, etc.
// Example output:
// input,output,overall_score,passed,relevance,factuality,safety,hallucination
// "How do I...", "Visit account...", 0.89, true, 0.85, 0.92, 1.00, 0.82
```

## Cost and Token Tracking

Track how much you're spending:

```csharp
using ElBruno.AI.Evaluation.Metrics;

var run = new EvaluationRun
{
    Results = evaluationResults,
    TokensUsed = 12500,           // Total tokens consumed
    CostInDollars = 0.0156,       // $0.0156 for this run
    StartedAt = DateTime.UtcNow,
    CompletedAt = DateTime.UtcNow
};

// Store with results
await store.SaveAsync(run);

// Later, analyze spending
var query = @"
SELECT 
  DATE(created_at) as day,
  COUNT(*) as evaluations,
  SUM(tokens_used) as total_tokens,
  SUM(cost_dollars) as total_cost
FROM evaluation_runs
GROUP BY DATE(created_at)
ORDER BY created_at DESC
LIMIT 30;
";

// Execute and see trends
// day        | evaluations | total_tokens | total_cost
// 2025-02-23 | 12          | 150000       | $0.19
// 2025-02-22 | 15          | 187500       | $0.24
// 2025-02-21 | 8           | 100000       | $0.13
```

## Enterprise Pattern: Baseline and Regression Detection

Here's a production-grade pattern used by enterprises:

```csharp
public class AIEvaluationPipeline
{
    private readonly IChatClient _chatClient;
    private readonly GoldenDataset _dataset;
    private readonly BaselineSnapshot _baseline;
    private readonly SqliteResultStore _store;
    
    public async Task<EvaluationReport> EvaluateAndCompareAsync(
        string modelVersion,
        CancellationToken ct = default)
    {
        // 1. Run evaluation
        var evaluators = GetProductionEvaluators();
        var pipeline = new EvaluationPipelineBuilder()
            .WithChatClient(_chatClient)
            .WithDataset(_dataset)
            .ForEach(evaluators, e => pipeline.AddEvaluator(e))
            .WithBaseline(_baseline)
            .Build();
        
        var regressionReport = await pipeline.RunWithBaselineAsync(ct);
        
        // 2. Store results
        await _store.SaveAsync(new EvaluationRunRecord
        {
            ModelVersion = modelVersion,
            Results = regressionReport.Results,
            RegressionDetected = regressionReport.HasRegressions,
            CreatedAt = DateTimeOffset.UtcNow
        });
        
        // 3. Generate report
        return new EvaluationReport
        {
            ModelVersion = modelVersion,
            PassRate = regressionReport.Results.Count(r => r.Passed) / (double)regressionReport.Results.Count,
            RegressionDetected = regressionReport.HasRegressions,
            RegressionDetails = regressionReport.RegressionDetails,
            FailedExamples = regressionReport.Results
                .Where(r => !r.Passed)
                .Select(r => new FailureDetail { Input = r.Input, Reason = r.Details })
                .ToList()
        };
    }
    
    private List<IEvaluator> GetProductionEvaluators()
    {
        return new()
        {
            new RelevanceEvaluator(0.7),     // Must address the question
            new FactualityEvaluator(0.85),   // Strict on facts
            new HallucinationEvaluator(0.8), // Watch for making things up
            new SafetyEvaluator(0.95)        // Safety is non-negotiable
        };
    }
}

// Usage in CI/CD
var pipeline = new AIEvaluationPipeline(
    chatClient,
    dataset,
    baselineSnapshot,
    resultStore
);

var report = await pipeline.EvaluateAndCompareAsync("v2.1.0");

if (report.RegressionDetected)
{
    Console.WriteLine("❌ QUALITY REGRESSION DETECTED");
    foreach (var detail in report.RegressionDetails)
    {
        Console.WriteLine($"  {detail.MetricName}: {detail.BaselineValue:F2} → {detail.CurrentValue:F2}");
    }
    
    // Block deployment
    Environment.Exit(1);
}

Console.WriteLine($"✅ Quality check passed (pass rate: {report.PassRate:P})");
```

## Monitoring Dashboard

Store evaluation data in a time-series database for long-term monitoring:

```csharp
// After each evaluation run, write to monitoring system
var metrics = new Dictionary<string, double>
{
    ["ai.relevance.avg"] = results.Average(r => r.MetricScores["relevance"].Value),
    ["ai.factuality.avg"] = results.Average(r => r.MetricScores["factuality"].Value),
    ["ai.safety.min"] = results.Min(r => r.MetricScores["safety"].Value),
    ["ai.pass_rate"] = results.Count(r => r.Passed) / (double)results.Count,
    ["ai.tokens_used"] = run.TokensUsed,
    ["ai.cost_dollars"] = run.CostInDollars
};

// Send to your monitoring service (DataDog, Prometheus, CloudWatch, etc.)
await monitoringClient.RecordMetricsAsync(metrics);
```

In your dashboard, you'd see charts like:

```
Quality Over Time (Last 30 Days)
─────────────────────────────────
Score  │                    ┌─────────
0.90   │                   ╱
0.85   │              ┌────╱
0.80   │          ┌───╱
0.75   │      ┌───╱  ← Quality improved after v2.1.0
       │──────┴─────────────────────→
       Day 1                    Day 30

Pass Rate by Model Version
───────────────────────────
v2.0.0  ███████████████░░░ 88%
v2.0.1  ██████████████░░░░░ 84%  ← Regression!
v2.1.0  ████████████████░░ 92%   ← Fixed
```

## Deployment Gate

Use evaluation results to gate deployments:

```yaml
# .github/workflows/deploy.yml
name: Deploy with AI Quality Check

on:
  push:
    branches: [main]

jobs:
  evaluate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      
      - name: Run AI Evaluation
        run: dotnet run --project tests/ElBruno.AI.Evaluation.Tests/Evaluate.cs
        env:
          EVALUATION_BASELINE: production-v2.1.0
          LLM_ENDPOINT: ${{ secrets.LLM_ENDPOINT }}
      
      - name: Check Results
        run: |
          if [ -f "evaluation-regression.txt" ]; then
            echo "❌ Quality regression detected"
            exit 1
          fi
          echo "✅ Quality check passed"
  
  deploy:
    needs: evaluate
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Production
        run: ./deploy.sh
```

Your LLM can't be deployed if quality has regressed!

## Long-term Strategy

1. **Week 1:** Start with a small golden dataset (10 examples)
2. **Week 2:** Add your most common question types
3. **Week 3:** Establish baseline metrics for current model
4. **Week 4:** Set up regression detection in CI/CD
5. **Month 2:** Expand dataset to 50+ examples
6. **Month 3:** Add cost tracking and optimization
7. **Ongoing:** Evolve dataset as you learn from production data

## Try It Yourself

Create your first evaluation run with persistence:

```csharp
// Evaluate
var evaluator = new RelevanceEvaluator();
var result = await evaluator.EvaluateAsync(
    "What is AI?",
    "AI is artificial intelligence, a field of computer science."
);

// Persist
var store = new SqliteResultStore("evals.db");
await store.SaveAsync(result);

// Export
var exporter = new JsonExporter();
var json = await exporter.ExportAsync(
    new EvaluationRun { Results = new() { result } }
);
Console.WriteLine(json);
```

Run it daily, weekly, or with every deployment. Over time, you'll have a clear picture of your AI application's quality trend.

---

## Conclusion: The Complete Journey

You've now explored the full landscape of AI testing in .NET:

1. **Testing AI in .NET: The Landscape** — Understanding both toolkits
2. **Building Your Test Foundation** — Datasets and synthetic data (ElBruno)
3. **Evaluators: From Quick Checks to Deep Analysis** — Layered evaluation strategies
4. **AI Testing in Your CI Pipeline** — xUnit integration and automation
5. **Production AI Evaluation** (this post) — Complete pipeline using both toolkits

**Next steps:**

- Start with ElBruno for fast iteration and regression detection
- Graduate to Microsoft when you need nuanced quality judgment
- Use both in production for comprehensive coverage
- Monitor trends over time with SQLite + dashboards

The .NET ecosystem now has enterprise-grade AI evaluation. Build with confidence. Deploy with metrics. Monitor in production.

---

**Advanced topics:** See posts 6 & 7 for deep dives on synthetic data generation and evaluator selection by scenario.

---

## 👨‍💻 About the Author

**Bruno Capuano** is a Microsoft MVP and AI enthusiast who builds practical tools for .NET developers. This is Part 5 of a 7-part series on AI evaluation.

**🌟 Found this helpful?** Let's connect:

- 📘 [Read more on my blog](https://elbruno.com) — Deep technical articles on AI & .NET
- 🎥 [Watch video tutorials on YouTube](https://www.youtube.com/elbruno) — Demos and live coding
- 💼 [Connect on LinkedIn](https://www.linkedin.com/in/elbruno/) — Professional updates
- 🐦 [Follow on Twitter/X](https://www.x.com/elbruno/) — Quick tips and announcements
- 🎙️ [No Tiene Nombre Podcast](https://notienenombre.com) — Tech talks in Spanish
- 💻 [Explore more projects on GitHub](https://github.com/elbruno/) — Open-source AI tools

⭐ *If this series is helping you build better AI applications, give the [repo](https://github.com/elbruno/elbruno-ai-evaluation) a star and share it with your team!*
