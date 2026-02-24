// =============================================================================
// Evaluation Journey Sample
// Complete pipeline: synthetic data → evaluators → regression → JSON export.
// =============================================================================

using System.Text.Json;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Metrics;
using ElBruno.AI.Evaluation.Reporting;
using ElBruno.AI.Evaluation.SyntheticData.Generators;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using Microsoft.Extensions.AI;

Console.WriteLine("=== Complete Evaluation Journey ===\n");

// --- Step 1: Generate synthetic Q&A dataset (10 examples) ---
var template = new QaTemplate(
    questionTemplates: ["What is {0}?", "Define {0}.", "Explain {0}.", "Describe {0}.", "How does {0} work?"],
    answerTemplates: [
        "{0} is an important concept in software engineering.",
        "{0} refers to a methodology for building reliable systems.",
        "{0} involves systematic approaches to quality assurance.",
        "{0} is a practice used in modern development workflows.",
        "{0} enables teams to deliver higher quality software."
    ]
).WithCategory("Software Engineering").AddTags("qa", "eval-journey");

var generator = new DeterministicGenerator(template, randomSeed: 123);
var examples = await generator.GenerateAsync(10);
var dataset = new GoldenDataset { Name = "eval-journey", Tags = ["demo"] };
foreach (var ex in examples) dataset.AddExample(ex);
Console.WriteLine($"📝 Generated {dataset.Examples.Count} synthetic examples\n");

// --- Step 2: Mock chat client returning simple responses ---
using var client = new MockChatClient(dataset.Examples
    .Select(e => (e.Input, $"Regarding your question: {e.ExpectedOutput} This is well-established knowledge."))
    .ToList());

// --- Step 3: Run ElBruno deterministic evaluators ---
IEvaluator[] evaluators = [
    new RelevanceEvaluator(threshold: 0.5),
    new CoherenceEvaluator(threshold: 0.6),
    new SafetyEvaluator(),
    new ConcisenessEvaluator(minWords: 5, maxWords: 300),
    new CompletenessEvaluator()
];

Console.WriteLine("🔍 Running 5 evaluators on each example...\n");
var run = new EvaluationRun { StartedAt = DateTimeOffset.UtcNow, DatasetName = dataset.Name };

foreach (var example in dataset.Examples)
{
    var response = await client.GetResponseAsync(example.Input);
    var output = response.Text ?? "";
    var scores = new Dictionary<string, MetricScore>();
    var allPassed = true;

    foreach (var eval in evaluators)
    {
        var result = await eval.EvaluateAsync(example.Input, output, example.ExpectedOutput);
        allPassed &= result.Passed;
        foreach (var kv in result.MetricScores) scores[kv.Key] = kv.Value;
    }

    run.Results.Add(new EvaluationResult
    {
        Score = scores.Values.Average(s => s.Value),
        Passed = allPassed,
        Details = $"Evaluated: {example.Input[..Math.Min(40, example.Input.Length)]}...",
        MetricScores = scores
    });
}
run.CompletedAt = DateTimeOffset.UtcNow;

// --- Step 4: Per-evaluator results ---
Console.WriteLine("📊 Results Summary:");
Console.WriteLine($"  Pass rate: {run.PassRate:P0} ({run.Results.Count(r => r.Passed)}/{run.Results.Count})");
Console.WriteLine($"  Aggregate: {run.AggregateScore:F3}");
Console.WriteLine($"  Duration:  {run.Duration?.TotalMilliseconds:F0}ms\n");

var metricNames = run.Results.SelectMany(r => r.MetricScores.Keys).Distinct();
foreach (var metric in metricNames)
{
    var scores = run.Results.Where(r => r.MetricScores.ContainsKey(metric))
                           .Select(r => r.MetricScores[metric].Value).ToList();
    Console.WriteLine($"  {metric,-20} avg={scores.Average():F3}  min={scores.Min():F3}  max={scores.Max():F3}");
}

// --- Step 5: Regression detection ---
Console.WriteLine("\n🔄 Regression Detection:");
var baseline = run.ToBaseline();
Console.WriteLine($"  Baseline created with {baseline.Scores.Count} metrics");

// Simulate second run with slightly different scores
var secondRun = run.ToBaseline();
secondRun = new BaselineSnapshot
{
    DatasetName = baseline.DatasetName,
    Scores = baseline.Scores.ToDictionary(kv => kv.Key, kv => kv.Value - 0.02),
    AggregateScore = baseline.AggregateScore - 0.02
};
var report = RegressionDetector.Compare(baseline, secondRun, tolerance: 0.05);
Console.WriteLine($"  Regressions: {report.HasRegressions} | Regressed: {report.Regressed.Count} | Unchanged: {report.Unchanged.Count}");

// --- Step 6: Export results to JSON ---
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "journey-results.json");
var exporter = new JsonExporter();
await exporter.ExportAsync(run, outputPath);
Console.WriteLine($"\n💾 Results exported to: {outputPath}");
File.Delete(outputPath);

// --- Step 7: Journey summary ---
Console.WriteLine("\n🎉 Evaluation Journey Complete!");
Console.WriteLine($"   Data → {dataset.Examples.Count} examples generated");
Console.WriteLine($"   Eval → {evaluators.Length} evaluators × {dataset.Examples.Count} examples = {evaluators.Length * dataset.Examples.Count} checks");
Console.WriteLine($"   Regression → baseline created and compared");
Console.WriteLine($"   Export → JSON report generated");

// =============================================================================
// Mock IChatClient that returns predetermined responses
// =============================================================================
sealed class MockChatClient(List<(string Input, string Output)> pairs) : IChatClient
{
    public void Dispose() { }
    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken ct = default)
    {
        var last = messages.LastOrDefault()?.Text ?? "";
        var match = pairs.FirstOrDefault(p => p.Input == last);
        var text = match.Output ?? "I don't have information about that topic.";
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken ct = default)
        => throw new NotSupportedException();
}
