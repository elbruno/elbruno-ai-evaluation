// ============================================================================
// RAG Pipeline Evaluation Sample
// Demonstrates how to evaluate a RAG system using ElBruno.AI.Evaluation
// ============================================================================

using System.Text.Json;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using ElBruno.AI.Evaluation.Metrics;
using Microsoft.Extensions.AI;

Console.WriteLine("📚 RAG Pipeline Evaluation Sample");
Console.WriteLine("=================================\n");

// ---------------------------------------------------------------------------
// Step 1: Create a mock RAG pipeline chat client.
// This simulates a retrieval-augmented generation system that uses context
// from retrieved documents to generate answers.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 1: Setting up mock RAG pipeline...");
var ragClient = new MockRagChatClient();
Console.WriteLine("   ✅ Mock RAG pipeline ready\n");

// ---------------------------------------------------------------------------
// Step 2: Load the RAG golden dataset.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 2: Loading RAG golden dataset...");
var loader = new JsonDatasetLoader();
var datasetPath = Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..", "..",
    "src", "ElBruno.AI.Evaluation", "Datasets", "datasets", "rag-examples.json");
datasetPath = Path.GetFullPath(datasetPath);

GoldenDataset dataset;
if (File.Exists(datasetPath))
{
    dataset = await loader.LoadAsync(datasetPath);
    Console.WriteLine($"   ✅ Loaded '{dataset.Name}' with {dataset.Examples.Count} examples\n");
}
else
{
    // Fallback: create an inline RAG dataset
    Console.WriteLine($"   ⚠️ Dataset file not found at '{datasetPath}', using inline dataset");
    dataset = new GoldenDataset
    {
        Name = "rag-inline",
        Description = "Inline RAG evaluation dataset",
        Examples =
        [
            new()
            {
                Input = "What is the return policy?",
                ExpectedOutput = "Items can be returned within 30 days of purchase with a valid receipt for a full refund.",
                Context = "Return Policy: All items purchased from our store may be returned within 30 days of the original purchase date. A valid receipt is required for a full refund."
            },
            new()
            {
                Input = "How much does the Pro plan cost?",
                ExpectedOutput = "The Pro plan costs $49 per month or $470 per year with annual billing.",
                Context = "Pricing: Basic plan $19/month. Pro plan $49/month or $470/year (annual billing). Enterprise plan: contact sales."
            },
            new()
            {
                Input = "What programming languages does the SDK support?",
                ExpectedOutput = "The SDK supports C#, Python, JavaScript, and Java.",
                Context = "SDK Documentation: Our SDK is available for C#, Python, JavaScript, and Java."
            },
        ]
    };
    Console.WriteLine($"   ✅ Created inline dataset with {dataset.Examples.Count} examples\n");
}

var summary = dataset.GetSummary();
Console.WriteLine($"   Dataset: {summary.TotalExamples} examples, {summary.ExamplesWithContext} with context\n");

// ---------------------------------------------------------------------------
// Step 3: Configure evaluators for RAG — Factuality, Hallucination, Coherence.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 3: Configuring RAG evaluators...");
IEvaluator[] evaluators =
[
    new FactualityEvaluator(),
    new HallucinationEvaluator(),
    new CoherenceEvaluator()
];
Console.WriteLine("   ✅ FactualityEvaluator (threshold=0.8)");
Console.WriteLine("   ✅ HallucinationEvaluator (threshold=0.7)");
Console.WriteLine("   ✅ CoherenceEvaluator (threshold=0.7)\n");

// ---------------------------------------------------------------------------
// Step 4: Run initial evaluation to establish a baseline.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 4: Running initial evaluation (baseline run)...");
var baselineRun = await ragClient.EvaluateAsync(dataset, evaluators);
Console.WriteLine($"   ✅ Baseline run complete — {baselineRun.Results.Count} examples evaluated");
Console.WriteLine($"   Aggregate Score: {baselineRun.AggregateScore:F4}");
Console.WriteLine($"   Pass Rate: {baselineRun.PassRate:P1}\n");

// Save baseline snapshot
var baseline = baselineRun.ToBaseline();
Console.WriteLine($"   Baseline snapshot: {baseline.Scores.Count} metrics captured");
foreach (var (metric, score) in baseline.Scores)
    Console.WriteLine($"     └─ {metric}: {score:F4}");
Console.WriteLine();

// ---------------------------------------------------------------------------
// Step 5: Simulate a "new version" run and detect regressions.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 5: Running regression detection...");
Console.WriteLine("   Simulating evaluation of updated RAG pipeline...");

// Use the same deterministic client — scores should be unchanged
var regressionReport = await ragClient.CompareBaselineAsync(dataset, evaluators, baseline);

Console.WriteLine($"\n   Regression Report (tolerance={regressionReport.Tolerance:P0}):");
Console.WriteLine("   ─────────────────────────────────────────────");

if (regressionReport.Improved.Count > 0)
{
    Console.WriteLine("   📈 Improved:");
    foreach (var (metric, values) in regressionReport.Improved)
        Console.WriteLine($"      {metric}: {values.Baseline:F4} → {values.Current:F4} (+{values.Current - values.Baseline:F4})");
}

if (regressionReport.Regressed.Count > 0)
{
    Console.WriteLine("   📉 Regressed:");
    foreach (var (metric, values) in regressionReport.Regressed)
        Console.WriteLine($"      {metric}: {values.Baseline:F4} → {values.Current:F4} ({values.Current - values.Baseline:F4})");
}

if (regressionReport.Unchanged.Count > 0)
{
    Console.WriteLine("   ➡️  Unchanged:");
    foreach (var (metric, values) in regressionReport.Unchanged)
        Console.WriteLine($"      {metric}: {values.Baseline:F4} → {values.Current:F4}");
}

Console.WriteLine($"\n   Overall: {(regressionReport.OverallPassed ? "✅ NO REGRESSIONS" : "❌ REGRESSIONS DETECTED")}");
Console.WriteLine();

// ---------------------------------------------------------------------------
// Step 6: Print detailed per-example results.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 6: Detailed per-example results");
Console.WriteLine("─────────────────────────────────────────────────────");
for (int i = 0; i < baselineRun.Results.Count; i++)
{
    var r = baselineRun.Results[i];
    var example = dataset.Examples[i];
    var truncatedInput = example.Input.Length > 50 ? example.Input[..50] + "..." : example.Input;
    Console.WriteLine($"  [{i + 1}] {(r.Passed ? "✅ PASS" : "❌ FAIL")} Score={r.Score:F4} | {truncatedInput}");
    Console.WriteLine($"       Context: {(example.Context != null ? "provided" : "none")}");

    foreach (var (name, metric) in r.MetricScores)
        Console.WriteLine($"       └─ {metric}");
    Console.WriteLine();
}

// ---------------------------------------------------------------------------
// Step 7: Export results to JSON.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 7: Exporting results to JSON...");
var exportPath = Path.Combine(AppContext.BaseDirectory, "rag-evaluation-results.json");
try
{
    var exportData = new
    {
        RunId = baselineRun.RunId,
        Dataset = baselineRun.DatasetName,
        AggregateScore = baselineRun.AggregateScore,
        PassRate = baselineRun.PassRate,
        AllPassed = baselineRun.AllPassed,
        Results = baselineRun.Results.Select((r, idx) => new
        {
            Example = idx + 1,
            Input = dataset.Examples[idx].Input,
            r.Score,
            r.Passed,
            r.Details,
            Metrics = r.MetricScores.ToDictionary(m => m.Key, m => new { m.Value.Value, m.Value.Passed })
        }),
        Baseline = new
        {
            baseline.DatasetName,
            baseline.AggregateScore,
            baseline.Scores
        },
        Regression = new
        {
            regressionReport.OverallPassed,
            regressionReport.Tolerance,
            ImprovedCount = regressionReport.Improved.Count,
            RegressedCount = regressionReport.Regressed.Count,
            UnchangedCount = regressionReport.Unchanged.Count
        }
    };

    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    var json = JsonSerializer.Serialize(exportData, jsonOptions);
    await File.WriteAllTextAsync(exportPath, json);
    Console.WriteLine($"   ✅ Results exported to {exportPath}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   ⚠️ JSON export failed: {ex.Message}\n");
}

Console.WriteLine("🎉 RAG pipeline evaluation sample complete!");

// ============================================================================
// Mock RAG Chat Client — simulates a RAG pipeline that retrieves context
// and generates answers. Returns expected-style answers for known inputs.
// ============================================================================
sealed class MockRagChatClient : IChatClient
{
    // Simulated RAG responses — in production these come from retrieval + generation
    private static readonly Dictionary<string, string> RagResponses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["What is the return policy?"] = "Items can be returned within 30 days of purchase with a valid receipt for a full refund.",
        ["How much does the Pro plan cost?"] = "The Pro plan costs $49 per month or $470 per year with annual billing.",
        ["What programming languages does the SDK support?"] = "The SDK supports C#, Python, JavaScript, and Java.",
        ["What are the system requirements?"] = "The application requires Windows 10 or later, 8 GB RAM minimum, and 2 GB of free disk space.",
        ["How do I configure SSO?"] = "Navigate to Admin > Security > SSO Configuration, enter your Identity Provider metadata URL, and click Save.",
        ["What SLA do you offer?"] = "We offer 99.9% uptime SLA for Pro plans and 99.99% for Enterprise plans.",
        ["How do I export my data?"] = "Go to Settings > Data Management > Export and select your preferred format (CSV, JSON, or Parquet).",
    };

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lastMessage = messages.LastOrDefault()?.Text ?? string.Empty;

        var responseText = RagResponses.TryGetValue(lastMessage, out var known)
            ? known
            : $"Based on the retrieved documents, here is the answer to your question: {lastMessage}";

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Streaming not supported in this sample.");

    public void Dispose() { }
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
}
