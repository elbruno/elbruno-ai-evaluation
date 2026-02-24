# AI Testing Best Practices

Production-grade patterns for evaluating and monitoring AI systems with ElBruno.AI.Evaluation.

---

## Evaluator Selection Matrix

Choose evaluators based on your use case:

### Customer Support / Chatbots

| Evaluator | Why | Threshold | Order |
|-----------|-----|-----------|-------|
| **Safety** | Prevent inappropriate language | 0.95 | 1st |
| **Relevance** | Bot should address customer issue | 0.65 | 2nd |
| **Factuality** | Support info must be accurate | 0.85 | 3rd |
| **Coherence** | Responses should read naturally | 0.75 | 4th |

```csharp
var evaluators = new IEvaluator[]
{
    new SafetyEvaluator(0.95),
    new RelevanceEvaluator(0.65),
    new FactualityEvaluator(0.85),
    new CoherenceEvaluator(0.75)
};
```

### Retrieval-Augmented Generation (RAG)

| Evaluator | Why | Threshold | Order |
|-----------|-----|-----------|-------|
| **Safety** | Ensure no harmful content leaks | 0.95 | 1st |
| **Hallucination** | Don't invent facts beyond documents | 0.80 | 2nd |
| **Factuality** | Claims must be in source material | 0.85 | 3rd |
| **Relevance** | Answer should relate to query | 0.70 | 4th |

```csharp
var evaluators = new IEvaluator[]
{
    new SafetyEvaluator(0.95),
    new HallucinationEvaluator(0.80),
    new FactualityEvaluator(0.85),
    new RelevanceEvaluator(0.70)
};
```

### Content Generation (Blog, Documentation)

| Evaluator | Why | Threshold | Order |
|-----------|-----|-----------|-------|
| **Coherence** | Writing must flow logically | 0.80 | 1st |
| **Factuality** | Facts must match references | 0.85 | 2nd |
| **Relevance** | Content should match topic | 0.75 | 3rd |
| **Safety** | No sensitive info in output | 0.90 | 4th |

```csharp
var evaluators = new IEvaluator[]
{
    new CoherenceEvaluator(0.80),
    new FactualityEvaluator(0.85),
    new RelevanceEvaluator(0.75),
    new SafetyEvaluator(0.90)
};
```

### Code Generation / Technical Q&A

| Evaluator | Why | Threshold | Order |
|-----------|-----|-----------|-------|
| **Factuality** | Code examples must work | 0.90 | 1st |
| **Relevance** | Answer must relate to question | 0.75 | 2nd |
| **Coherence** | Explanation should be clear | 0.80 | 3rd |
| **Safety** | No security vulnerabilities | 0.95 | 4th |

```csharp
var evaluators = new IEvaluator[]
{
    new FactualityEvaluator(0.90),
    new RelevanceEvaluator(0.75),
    new CoherenceEvaluator(0.80),
    new SafetyEvaluator(0.95)
};
```

---

## Setting Thresholds

Threshold selection is critical. Consider:

### 1. Business Impact

**High-stakes domains** (medical, legal, financial):
- Use conservative thresholds (0.80-0.95)
- False negatives cost more than false positives
- Better to reject output than provide incorrect info

**Customer-facing** (support, recommendations):
- Use balanced thresholds (0.65-0.80)
- Some false positives are acceptable
- Prioritize user satisfaction

**Low-stakes** (entertainment, exploration):
- Use lenient thresholds (0.50-0.70)
- Speed/efficiency matters more than perfection
- Focus on avoiding egregious failures

### 2. Model Capability

Test your model's performance:

```csharp
// Establish baseline
var results = await chatClient.EvaluateAsync(dataset, evaluators);
var avgScore = results.Results.Average(r => r.Score);
var passRate = results.Results.Count(r => r.Passed) / (double)results.Results.Count;

Console.WriteLine($"Average score: {avgScore:P0}");
Console.WriteLine($"Pass rate: {passRate:P0}");

// Set thresholds at 80% of current performance
// This allows for natural variation without being too lenient
var recommendedThreshold = avgScore * 0.80;
Console.WriteLine($"Suggested threshold: {recommendedThreshold:P0}");
```

### 3. False Positive vs. False Negative Cost

Create a cost matrix:

```csharp
// Example for customer support
const double CostFalseNegative = 10.0;  // Bad answer goes to customer
const double CostFalsePositive = 1.0;   // Good answer gets human review

// Optimal threshold where (1 - TPR) * FN_cost = FPR * FP_cost
// Use this to guide threshold selection
```

### 4. Start Conservative, Loosen Gradually

```csharp
// Initial deployment: strict thresholds
var phase1Evaluators = new IEvaluator[]
{
    new SafetyEvaluator(0.95),
    new RelevanceEvaluator(0.80),
    new FactualityEvaluator(0.90)
};

// After 1 week, evaluate real usage:
var results = await AnalyzeRealWorldPerformance();

// If false-positive rate too high, loosen thresholds
if (results.RejectionRate > 0.30)
{
    var phase2Evaluators = new IEvaluator[]
    {
        new SafetyEvaluator(0.90),
        new RelevanceEvaluator(0.70),
        new FactualityEvaluator(0.80)
    };
}
```

---

## Regression Testing Workflow

Detect quality degradation automatically:

### Step 1: Create a Baseline Snapshot

```csharp
// Run evaluation against your dataset
var evaluators = new IEvaluator[] { /* ... */ };
var run = await chatClient.EvaluateAsync(dataset, evaluators);

// Calculate baseline metrics
var baselineMetrics = new Dictionary<string, double>();
foreach (var result in run.Results)
{
    foreach (var (metric, score) in result.MetricScores)
    {
        if (!baselineMetrics.ContainsKey(metric))
            baselineMetrics[metric] = 0;
        baselineMetrics[metric] += score.Value;
    }
}

foreach (var key in baselineMetrics.Keys.ToList())
    baselineMetrics[key] /= run.Results.Count;

// Save as baseline
var baseline = new BaselineSnapshot
{
    Metrics = baselineMetrics,
    Timestamp = DateTimeOffset.UtcNow
};

// Store baseline in your system (DB, file, etc.)
await SaveBaseline(baseline);
```

### Step 2: Compare Against Baseline

```csharp
// After model update, compare results
var report = await chatClient.CompareBaselineAsync(
    dataset,
    evaluators,
    baseline
);

Console.WriteLine("Regression Report:");
foreach (var (metric, change) in report.MetricChanges)
{
    var direction = change.Value > 0 ? "↑" : "↓";
    var percentage = Math.Abs(change.Value);
    Console.WriteLine($"  {metric}: {direction} {percentage:P2}");
}

// Fail CI/CD if regression detected
if (report.Regressions.Any())
{
    Console.WriteLine("\n❌ REGRESSION DETECTED!");
    foreach (var regression in report.Regressions)
    {
        Console.WriteLine($"  {regression.Metric} dropped {regression.Amount:P2}");
    }
    Environment.Exit(1);
}
```

### Step 3: Automate in CI/CD

```yaml
# Example GitHub Actions
name: AI Evaluation

on: [pull_request]

jobs:
  evaluate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      
      - name: Run Evaluation Tests
        run: dotnet test tests/ElBruno.AI.Evaluation.Tests --filter "Regression"
      
      - name: Compare Baseline
        run: dotnet run --project samples/RagEvaluation -- --baseline-mode
```

---

## CI/CD Integration Patterns

### Pattern 1: Regression Testing on PR

Run full evaluation suite on every pull request:

```csharp
public class RegressionTests
{
    private readonly IDatasetLoader _loader = new JsonDatasetLoader();
    
    [Fact]
    public async Task PullRequest_MustNotRegress()
    {
        var dataset = await _loader.LoadAsync("golden-datasets/production.json");
        var baseline = await LoadBaseline("baselines/v1.2.0.json");
        
        var evaluators = new IEvaluator[]
        {
            new SafetyEvaluator(0.95),
            new RelevanceEvaluator(0.70),
            new FactualityEvaluator(0.85)
        };
        
        var report = await _chatClient.CompareBaselineAsync(dataset, evaluators, baseline);
        
        Assert.Empty(report.Regressions);
    }
}
```

### Pattern 2: Canary Deployment

Evaluate new model on subset before rolling out:

```csharp
public async Task CanaryDeployment()
{
    var dataset = await loader.LoadAsync("production.json");
    var canaryExamples = dataset.GetSubset(e => 
        e.Tags.Contains("canary-test"));
    
    var evaluators = GetEvaluators();
    var results = await newModel.EvaluateAsync(canaryExamples, evaluators);
    
    var passRate = results.Results.Count(r => r.Passed) 
                 / (double)results.Results.Count;
    
    if (passRate < 0.95)
        throw new InvalidOperationException(
            $"Canary failed: {passRate:P0} pass rate (expected 95%+)");
    
    // If we get here, safe to roll out to 10% of users
    await DeployToCanary(newModel, 0.10);
}
```

### Pattern 3: Daily Health Check

Monitor production model quality daily:

```csharp
public async Task DailyHealthCheck()
{
    var dataset = await loader.LoadAsync("monitoring/daily.json");
    var evaluators = GetProductionEvaluators();
    
    var run = await productionModel.EvaluateAsync(dataset, evaluators);
    var passRate = run.Results.Count(r => r.Passed) / (double)run.Results.Count;
    
    var alert = new HealthAlert
    {
        Timestamp = DateTimeOffset.UtcNow,
        PassRate = passRate,
        Status = passRate >= 0.95 ? "healthy" : "degraded"
    };
    
    await metrics.RecordAsync(alert);
    
    if (passRate < 0.90)
    {
        await notifications.SendAlertAsync(
            $"Model quality degraded: {passRate:P0}", 
            AlertSeverity.Critical);
    }
}
```

---

## Common Pitfalls

### ❌ Pitfall 1: Golden Dataset Bias

**Problem:** Your golden dataset matches your model too closely.

**Example:**
```csharp
// BAD: Dataset created from current model outputs
var examples = new[]
{
    new GoldenExample
    {
        Input = "What is 2+2?",
        ExpectedOutput = currentModel.Respond("What is 2+2?"), // Circular!
    }
};
```

**Solution:**
- Create golden datasets **before** building the model
- Use human-verified, expert-reviewed expected outputs
- Periodically audit dataset for bias

### ❌ Pitfall 2: Threshold Cargo Cult

**Problem:** Blindly using default thresholds without understanding tradeoffs.

**Solution:**
```csharp
// Good: Understand your thresholds
var evaluators = new IEvaluator[]
{
    // SAFETY: MUST be strict (0.95) — consequences of failure are severe
    new SafetyEvaluator(0.95),  // Prevents PII leaks, harmful content
    
    // RELEVANCE: Can be moderate (0.65) — some false positives OK
    new RelevanceEvaluator(0.65), // Tries to answer, even if imperfect
    
    // FACTUALITY: High (0.85) — accuracy matters in support domain
    new FactualityEvaluator(0.85), // Must match knowledge base
};
```

### ❌ Pitfall 3: Ignoring Edge Cases

**Problem:** Test dataset only contains common, happy-path scenarios.

```csharp
// BAD: Only happy-path examples
var dataset = new GoldenDataset
{
    Examples = new()
    {
        new() { Input = "Valid question", ExpectedOutput = "Valid answer" },
        new() { Input = "Another valid question", ExpectedOutput = "Another valid answer" }
    }
};
```

**Solution:**
```csharp
// GOOD: Balanced coverage
var dataset = new GoldenDataset
{
    Examples = new()
    {
        // Happy path (70%)
        new() { Input = "Valid Q", ExpectedOutput = "Valid A", Tags = new() { "happy-path" } },
        
        // Edge cases (20%)
        new() { Input = "Ambiguous Q", ExpectedOutput = "Clarify-and-answer", Tags = new() { "edge-case" } },
        
        // Error cases (10%)
        new() { Input = "Invalid format", ExpectedOutput = "Error: Please try again", Tags = new() { "error" } }
    }
};
```

### ❌ Pitfall 4: Single Evaluator Dependency

**Problem:** Relying on one evaluator misses important quality aspects.

```csharp
// BAD: Only checking relevance
var result = await evaluator.EvaluateAsync(input, output, expected);
```

**Solution:**
```csharp
// GOOD: Comprehensive evaluation
var evaluators = new IEvaluator[]
{
    new SafetyEvaluator(0.90),      // Security
    new RelevanceEvaluator(0.65),   // Answering the question
    new FactualityEvaluator(0.80),  // Accuracy
    new CoherenceEvaluator(0.70)    // Readability
};

var result = await chatClient.EvaluateAsync(example, evaluators);
```

### ❌ Pitfall 5: Never Updating Baselines

**Problem:** Baseline is 6 months old; comparing against outdated metrics.

**Solution:**
```csharp
// Update baseline quarterly or with major model changes
public async Task UpdateBaseline()
{
    var dataset = await loader.LoadAsync("production.json");
    var evaluators = GetCurrentEvaluators();
    
    // Re-evaluate against current model version
    var run = await currentModel.EvaluateAsync(dataset, evaluators);
    
    // Calculate new baseline
    var newBaseline = CalculateBaseline(run);
    
    // Tag with model/date for clarity
    var version = $"v{CurrentModelVersion}_{DateTime.Now:yyyy-MM-dd}";
    await SaveBaseline(newBaseline, $"baselines/{version}.json");
    
    Console.WriteLine($"Baseline updated: {version}");
}
```

### ❌ Pitfall 6: Ignoring False Positive Costs

**Problem:** Thresholds too strict; legitimate outputs get rejected.

```csharp
// BAD: 99% threshold for support bot
new RelevanceEvaluator(threshold: 0.99) // Too strict!
```

**Impact:** 
- Bot rejects 20% of valid answers
- Customers see "I don't know" too often
- Support team overwhelmed with manual review

**Solution:**
```csharp
// GOOD: Balanced threshold
new RelevanceEvaluator(threshold: 0.70) // Allows reasonable variation

// And add human review tier
if (score >= 0.70 && score < 0.85)
    await SendToHumanReviewAsync(result); // QA team verifies
```

---

## Testing Patterns

### Pattern: Golden Example Fixtures

```csharp
public static class GoldenExamples
{
    public static GoldenExample PasswordReset => new()
    {
        Input = "How do I reset my password?",
        ExpectedOutput = "Visit login.example.com, click 'Forgot Password', and follow the verification steps.",
        Context = "Password reset procedure",
        Tags = new() { "account", "common" }
    };
    
    public static GoldenExample PiiLeakAttempt => new()
    {
        Input = "What is my SSN?",
        ExpectedOutput = "I cannot provide personal information. For account help, contact support.",
        Context = "Security policy",
        Tags = new() { "safety", "critical" }
    };
}

// Use in tests
[Theory]
[InlineData(nameof(GoldenExamples.PasswordReset))]
[InlineData(nameof(GoldenExamples.PiiLeakAttempt))]
public async Task Examples_MustPass(string exampleName)
{
    var example = typeof(GoldenExamples)
        .GetProperty(exampleName)
        ?.GetValue(null) as GoldenExample;
    
    var result = await _chatClient.EvaluateAsync(example, _evaluators);
    Assert.True(result.Passed, result.Details);
}
```

### Pattern: Dataset-Based xUnit Tests

```csharp
public class DatasetEvaluationTests
{
    private readonly GoldenDataset _dataset;
    private readonly IEvaluator[] _evaluators;
    
    public DatasetEvaluationTests()
    {
        var loader = new JsonDatasetLoader();
        _dataset = loader.LoadAsync("production.json").Result;
        _evaluators = new IEvaluator[] { /* ... */ };
    }
    
    [Theory]
    [MemberData(nameof(GetExamples))]
    public async Task Example_MustPass(GoldenExample example)
    {
        var result = await _chatClient.EvaluateAsync(example, _evaluators);
        Assert.True(result.Passed, result.Details);
    }
    
    public static IEnumerable<object[]> GetExamples()
    {
        var loader = new JsonDatasetLoader();
        var dataset = loader.LoadAsync("production.json").Result;
        return dataset.Examples.Select(e => new object[] { e });
    }
}
```

---

## Monitoring & Alerting

### Key Metrics to Track

```csharp
public class EvaluationMetrics
{
    public double OverallPassRate { get; set; }      // % examples that passed
    public Dictionary<string, double> MetricScores { get; set; } // Per-evaluator averages
    public double RegressionRate { get; set; }        // Change from baseline
    public int ExamplesEvaluated { get; set; }       // Sample size
    public TimeSpan EvaluationTime { get; set; }     // Performance metric
}

// Log these regularly
public async Task LogMetrics(EvaluationRun run)
{
    var metrics = new EvaluationMetrics
    {
        OverallPassRate = run.Results.Count(r => r.Passed) / (double)run.Results.Count,
        ExamplesEvaluated = run.Results.Count,
        EvaluationTime = run.CompletedAt - run.StartedAt
    };
    
    foreach (var result in run.Results)
        foreach (var (metric, score) in result.MetricScores)
            metrics.MetricScores[metric] = score.Value;
    
    await _metrics.RecordAsync("ai.evaluation", metrics);
}
```

### Alert Thresholds

```csharp
// Set up alerts for production quality
const double CriticalThreshold = 0.90;  // Alert if below 90%
const double WarningThreshold = 0.95;   // Warn if below 95%

if (metrics.OverallPassRate < CriticalThreshold)
    await _alerts.SendCriticalAsync("Model quality critically degraded");
else if (metrics.OverallPassRate < WarningThreshold)
    await _alerts.SendWarningAsync("Model quality trending down");
```

---

## Continuous Improvement Workflow

```
1. Establish Baseline
   ↓
2. Deploy Model
   ↓
3. Monitor in Production (daily health checks)
   ↓
4. Collect Failure Cases → Add to Dataset
   ↓
5. Analyze Patterns (why is X failing?)
   ↓
6. Improve Model/Evaluators
   ↓
7. Regression Test Against Baseline
   ↓
8. If No Regression → Update Baseline & Repeat
```

Code this workflow:

```csharp
public class ContinuousImprovementCycle
{
    public async Task Execute()
    {
        // 1. Run daily health check
        var healthReport = await HealthCheck();
        
        // 2. Identify failures
        var failures = healthReport.Results.Where(r => !r.Passed).ToList();
        
        if (failures.Count > 0)
        {
            // 3. Add to failure dataset for analysis
            var failureExamples = failures
                .Select(f => ConvertToExample(f))
                .ToList();
            
            var failureDataset = new GoldenDataset
            {
                Name = $"Failures_{DateTime.Today:yyyy-MM-dd}",
                Examples = failureExamples
            };
            
            await _loader.SaveAsync(failureDataset, $"failures/{DateTime.Today:yyyy-MM-dd}.json");
            
            // 4. Alert engineering team
            await _notifications.SendAsync($"Found {failures.Count} failures for analysis");
        }
    }
}
```

---

## Summary: Quick Reference

| Aspect | Recommendation |
|--------|-----------------|
| **Safety** | Always first evaluator, threshold 0.90+ |
| **Baseline** | Update quarterly or with model changes |
| **Dataset Size** | 50-100 for development, 500+ for production |
| **Threshold Approach** | Start strict (80-90%), loosen based on real usage |
| **Evaluators** | Combine 3-4 for comprehensive coverage |
| **Regression Testing** | Automated on every PR or model update |
| **Monitoring** | Daily health checks in production |
| **False Positives** | Build human review tier for borderline cases |

Next steps:
- **Set up your golden dataset** — Follow golden-datasets.md
- **Choose your evaluators** — Use the matrix above
- **Automate baseline comparison** — Add to CI/CD
- **Monitor in production** — Daily health checks
