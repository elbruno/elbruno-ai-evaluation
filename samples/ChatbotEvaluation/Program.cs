// ============================================================================
// Chatbot Evaluation Sample
// Demonstrates how to evaluate a chatbot using ElBruno.AI.Evaluation
// ============================================================================

using System.Text.Json;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using ElBruno.AI.Evaluation.Metrics;
using Microsoft.Extensions.AI;

Console.WriteLine("🤖 Chatbot Evaluation Sample");
Console.WriteLine("============================\n");

// ---------------------------------------------------------------------------
// Step 1: Create a mock IChatClient that echoes back the expected output.
// In production, this would be your real AI model endpoint.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 1: Setting up mock chat client...");
var chatClient = new EchoChatClient();
Console.WriteLine("   ✅ Mock chat client ready\n");

// ---------------------------------------------------------------------------
// Step 2: Load the chatbot golden dataset from the bundled datasets folder.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 2: Loading golden dataset...");
var loader = new JsonDatasetLoader();
var datasetPath = Path.Combine(
    AppContext.BaseDirectory, "..", "..", "..", "..", "..", 
    "src", "ElBruno.AI.Evaluation", "Datasets", "datasets", "chatbot-examples.json");
datasetPath = Path.GetFullPath(datasetPath);

GoldenDataset dataset;
if (File.Exists(datasetPath))
{
    dataset = await loader.LoadAsync(datasetPath);
    Console.WriteLine($"   ✅ Loaded '{dataset.Name}' with {dataset.Examples.Count} examples\n");
}
else
{
    // Fallback: create an inline dataset if the file path doesn't resolve
    Console.WriteLine($"   ⚠️ Dataset file not found at '{datasetPath}', using inline dataset");
    dataset = new GoldenDataset
    {
        Name = "chatbot-inline",
        Description = "Inline chatbot evaluation dataset",
        Examples =
        [
            new() { Input = "What is the capital of France?", ExpectedOutput = "The capital of France is Paris." },
            new() { Input = "How do I reset my password?", ExpectedOutput = "Go to Settings > Security > Reset Password and follow the prompts." },
            new() { Input = "What is machine learning?", ExpectedOutput = "Machine learning is a subset of artificial intelligence where systems learn patterns from data to make predictions or decisions without being explicitly programmed." },
        ]
    };
    Console.WriteLine($"   ✅ Created inline dataset with {dataset.Examples.Count} examples\n");
}

var summary = dataset.GetSummary();
Console.WriteLine($"   Dataset summary: {summary.TotalExamples} examples, {summary.UniqueTags.Count} unique tags\n");

// ---------------------------------------------------------------------------
// Step 3: Configure evaluators — Hallucination, Relevance, and Safety.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 3: Configuring evaluators...");
IEvaluator[] evaluators =
[
    new HallucinationEvaluator(),
    new RelevanceEvaluator(),
    new SafetyEvaluator()
];
Console.WriteLine("   ✅ HallucinationEvaluator (threshold=0.7)");
Console.WriteLine("   ✅ RelevanceEvaluator (threshold=0.6)");
Console.WriteLine("   ✅ SafetyEvaluator (threshold=0.9)\n");

// ---------------------------------------------------------------------------
// Step 4: Run the evaluation pipeline against the dataset.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 4: Running evaluation...");
var run = await chatClient.EvaluateAsync(dataset, evaluators);
Console.WriteLine($"   ✅ Evaluation complete — {run.Results.Count} examples evaluated\n");

// ---------------------------------------------------------------------------
// Step 5: Print results to console (ConsoleReporter-style output).
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 5: Results");
Console.WriteLine("─────────────────────────────────────────────────────");
Console.WriteLine($"  Run ID:          {run.RunId}");
Console.WriteLine($"  Dataset:         {run.DatasetName}");
Console.WriteLine($"  Aggregate Score: {run.AggregateScore:F4}");
Console.WriteLine($"  Pass Rate:       {run.PassRate:P1}");
Console.WriteLine($"  All Passed:      {run.AllPassed}");
Console.WriteLine("─────────────────────────────────────────────────────\n");

Console.WriteLine("  Per-example results:");
for (int i = 0; i < run.Results.Count; i++)
{
    var r = run.Results[i];
    var input = dataset.Examples[i].Input;
    var truncatedInput = input.Length > 50 ? input[..50] + "..." : input;
    Console.WriteLine($"  [{i + 1}] {(r.Passed ? "✅ PASS" : "❌ FAIL")} Score={r.Score:F4} | {truncatedInput}");

    foreach (var (name, metric) in r.MetricScores)
        Console.WriteLine($"       └─ {metric}");
}
Console.WriteLine();

// ---------------------------------------------------------------------------
// Step 6: Save results to SQLite (demonstration).
// Note: SqliteResultStore is a stub — we serialize the run as JSON to SQLite.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 6: Saving results to SQLite...");
var dbPath = Path.Combine(AppContext.BaseDirectory, "evaluation-results.db");
try
{
    // Use Microsoft.Data.Sqlite directly since SqliteResultStore is a stub
    using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={dbPath}");
    await connection.OpenAsync();

    using var createCmd = connection.CreateCommand();
    createCmd.CommandText = """
        CREATE TABLE IF NOT EXISTS evaluation_runs (
            id TEXT PRIMARY KEY,
            dataset_name TEXT NOT NULL,
            aggregate_score REAL NOT NULL,
            pass_rate REAL NOT NULL,
            all_passed INTEGER NOT NULL,
            results_json TEXT NOT NULL,
            created_at TEXT NOT NULL
        )
        """;
    await createCmd.ExecuteNonQueryAsync();

    using var insertCmd = connection.CreateCommand();
    insertCmd.CommandText = """
        INSERT INTO evaluation_runs (id, dataset_name, aggregate_score, pass_rate, all_passed, results_json, created_at)
        VALUES ($id, $dataset, $score, $passRate, $allPassed, $json, $createdAt)
        """;
    insertCmd.Parameters.AddWithValue("$id", run.RunId.ToString());
    insertCmd.Parameters.AddWithValue("$dataset", run.DatasetName);
    insertCmd.Parameters.AddWithValue("$score", run.AggregateScore);
    insertCmd.Parameters.AddWithValue("$passRate", run.PassRate);
    insertCmd.Parameters.AddWithValue("$allPassed", run.AllPassed ? 1 : 0);
    insertCmd.Parameters.AddWithValue("$json", JsonSerializer.Serialize(run.Results.Select(r => new { r.Score, r.Passed, r.Details })));
    insertCmd.Parameters.AddWithValue("$createdAt", DateTimeOffset.UtcNow.ToString("o"));
    await insertCmd.ExecuteNonQueryAsync();

    Console.WriteLine($"   ✅ Saved to {dbPath}\n");
}
catch (Exception ex)
{
    Console.WriteLine($"   ⚠️ SQLite save failed: {ex.Message}\n");
}

// ---------------------------------------------------------------------------
// Step 7: Baseline comparison — create a baseline and compare against it.
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 7: Baseline comparison...");

// Create a baseline from the current run
var baseline = run.ToBaseline();
Console.WriteLine($"   Baseline created with {baseline.Scores.Count} metrics, aggregate={baseline.AggregateScore:F4}");

// Simulate a "second run" and compare — since our mock is deterministic,
// results will be unchanged (within tolerance).
var regressionReport = await chatClient.CompareBaselineAsync(dataset, evaluators, baseline);

Console.WriteLine($"   Regression detection (tolerance={regressionReport.Tolerance:P0}):");
Console.WriteLine($"     Improved:  {regressionReport.Improved.Count} metrics");
Console.WriteLine($"     Regressed: {regressionReport.Regressed.Count} metrics");
Console.WriteLine($"     Unchanged: {regressionReport.Unchanged.Count} metrics");
Console.WriteLine($"     Overall:   {(regressionReport.OverallPassed ? "✅ PASSED" : "❌ REGRESSION DETECTED")}");
Console.WriteLine();

Console.WriteLine("🎉 Chatbot evaluation sample complete!");

// ============================================================================
// Mock IChatClient — echoes the input back with a realistic response pattern.
// In a real scenario, replace this with an actual AI model client.
// ============================================================================
sealed class EchoChatClient : IChatClient
{
    // Simple lookup to return expected-style answers for known inputs
    private static readonly Dictionary<string, string> Responses = new(StringComparer.OrdinalIgnoreCase)
    {
        ["What is the capital of France?"] = "The capital of France is Paris.",
        ["How do I reset my password?"] = "Go to Settings > Security > Reset Password and follow the prompts.",
        ["What are your business hours?"] = "Our business hours are Monday through Friday, 9 AM to 5 PM EST.",
        ["Can you help me track my order?"] = "Please provide your order number and I can look up the tracking status for you.",
        ["What is machine learning?"] = "Machine learning is a subset of artificial intelligence where systems learn patterns from data to make predictions or decisions without being explicitly programmed.",
        ["Tell me a joke"] = "Why do programmers prefer dark mode? Because light attracts bugs!",
        ["What payment methods do you accept?"] = "We accept Visa, Mastercard, American Express, PayPal, and Apple Pay.",
    };

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        var lastMessage = messages.LastOrDefault()?.Text ?? string.Empty;

        // Return a known response or echo the input
        var responseText = Responses.TryGetValue(lastMessage, out var known)
            ? known
            : $"You asked: {lastMessage}";

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotSupportedException("Streaming not supported in this sample.");

    public void Dispose() { }
    public object? GetService(Type serviceType, object? serviceKey = null) => null;
}
