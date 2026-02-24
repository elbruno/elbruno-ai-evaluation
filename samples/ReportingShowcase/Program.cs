using System.Text.Json;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Reporting;

// Create a sample golden dataset for testing
var dataset = new GoldenDataset
{
    Name = "customer-support",
    Examples = new()
    {
        new()
        {
            Input = "How do I reset my password?",
            ExpectedOutput = "Visit Settings > Security > Reset Password"
        },
        new()
        {
            Input = "What is the refund policy?",
            ExpectedOutput = "We offer 30-day refunds for unused products with original packaging"
        },
        new()
        {
            Input = "How do I contact support?",
            ExpectedOutput = "Email support@company.com or call 1-800-SUPPORT"
        },
        new()
        {
            Input = "Can I change my subscription plan?",
            ExpectedOutput = "Yes, go to Account > Billing > Change Plan to modify your subscription"
        }
    }
};

// Configure evaluators
var evaluators = new IEvaluator[]
{
    new RelevanceEvaluator(threshold: 0.6),
    new HallucinationEvaluator(threshold: 0.7)
};

// Create an evaluation run
var run = new EvaluationRun
{
    RunId = Guid.NewGuid(),
    DatasetName = dataset.Name,
    StartedAt = DateTimeOffset.UtcNow,
};

// Simulate evaluation results (in practice, you'd call evaluators on real AI outputs)
var sampleOutputs = new[]
{
    "Go to Settings menu and find the Security section to reset your password",
    "We have a 30-day refund policy for new products only",
    "Contact our support team via email at support@company.com",
    "You can change your plan in Account settings under Billing options"
};

// Run evaluations
foreach (var (example, output) in dataset.Examples.Zip(sampleOutputs))
{
    var scores = new Dictionary<string, double>();
    var metricScores = new Dictionary<string, ElBruno.AI.Evaluation.Metrics.MetricScore>();

    foreach (var evaluator in evaluators)
    {
        var result = await evaluator.EvaluateAsync(example.Input, output, example.ExpectedOutput);
        foreach (var (name, metric) in result.MetricScores)
        {
            metricScores[name] = metric;
        }
    }

    var overallScore = metricScores.Values.Average(m => m.Value);
    var passed = metricScores.Values.All(m => m.Passed);

    run.Results.Add(new EvaluationResult
    {
        Score = overallScore,
        Passed = passed,
        Details = $"Input: {example.Input}",
        MetricScores = metricScores
    });
}

run.CompletedAt = DateTimeOffset.UtcNow;

// 1. Store results in SQLite
Console.WriteLine("=== Storing Results in SQLite ===");
var dbPath = Path.Combine(AppContext.BaseDirectory, "evaluation.db");
await using (var store = await SqliteResultStore.CreateAsync(dbPath))
{
    await store.SaveRunAsync(run);
    Console.WriteLine($"✅ Saved run {run.RunId} to {dbPath}");
    
    var savedRuns = await store.GetRunsAsync(dataset.Name, limit: 1);
    Console.WriteLine($"✅ Retrieved {savedRuns.Count} run(s) from database");
}

// 2. Export to JSON
Console.WriteLine("\n=== Exporting to JSON ===");
var jsonPath = Path.Combine(AppContext.BaseDirectory, "results.json");
var jsonExporter = new JsonExporter();
await jsonExporter.ExportAsync(run, jsonPath);
Console.WriteLine($"✅ Exported to {jsonPath}");

// Show JSON preview
var jsonContent = await File.ReadAllTextAsync(jsonPath);
var jsonPreview = jsonContent.Length > 300 ? jsonContent.Substring(0, 300) + "..." : jsonContent;
Console.WriteLine($"   Preview:\n{jsonPreview}");

// 3. Export to CSV
Console.WriteLine("\n=== Exporting to CSV ===");
var csvPath = Path.Combine(AppContext.BaseDirectory, "results.csv");
var csvExporter = new CsvExporter();
await csvExporter.ExportAsync(run, csvPath);
Console.WriteLine($"✅ Exported to {csvPath}");

// Show CSV preview
var csvLines = (await File.ReadAllLinesAsync(csvPath)).Take(3).ToList();
Console.WriteLine($"   Preview:");
foreach (var line in csvLines)
    Console.WriteLine($"   {line}");

// 4. Print console summary
Console.WriteLine("\n=== Console Report Summary ===");
var reporter = new ConsoleReporter { Verbose = true };
reporter.Report(run);

Console.WriteLine("\n✅ Reporting demonstration complete!");
