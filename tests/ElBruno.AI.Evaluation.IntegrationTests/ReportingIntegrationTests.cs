using Xunit;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.IntegrationTests;

public class ReportingIntegrationTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public ReportingIntegrationTests() => Directory.CreateDirectory(_tempDir);
    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public async Task EvaluationRun_ToBaseline_SaveAndReload()
    {
        var client = new MockChatClient("The capital of France is Paris.");
        var dataset = new GoldenDataset
        {
            Name = "report-test",
            Examples = [new() { Input = "Capital of France?", ExpectedOutput = "Paris" }]
        };

        var run = await client.EvaluateAsync(dataset, [new RelevanceEvaluator()]);
        var baseline = run.ToBaseline();

        var path = Path.Combine(_tempDir, "baseline.json");
        await baseline.SaveAsync(path);
        var loaded = await BaselineSnapshot.LoadAsync(path);

        Assert.Equal("report-test", loaded.DatasetName);
    }

    [Fact]
    public async Task DatasetLoader_JsonRoundTrip_ThenEvaluate()
    {
        var loader = new JsonDatasetLoader();
        var dataset = new GoldenDataset
        {
            Name = "round-trip",
            Examples =
            [
                new() { Input = "Hello", ExpectedOutput = "Hi there", Tags = ["greeting"] }
            ]
        };

        var path = Path.Combine(_tempDir, "dataset.json");
        await loader.SaveAsync(dataset, path);
        var loaded = await loader.LoadAsync(path);

        var client = new MockChatClient("Hi there, how are you?");
        var run = await client.EvaluateAsync(loaded, [new RelevanceEvaluator(), new SafetyEvaluator()]);

        Assert.Single(run.Results);
        Assert.True(run.AggregateScore > 0.0);
    }

    [Fact]
    public async Task BaselineComparison_EndToEnd()
    {
        var client = new MockChatClient("Paris is the capital of France.");
        var dataset = new GoldenDataset
        {
            Name = "e2e-test",
            Examples = [new() { Input = "Capital of France?", ExpectedOutput = "Paris" }]
        };

        // Create baseline from first run
        var run1 = await client.EvaluateAsync(dataset, [new RelevanceEvaluator()]);
        var baseline = run1.ToBaseline();

        var baselinePath = Path.Combine(_tempDir, "baseline.json");
        await baseline.SaveAsync(baselinePath);
        var loadedBaseline = await BaselineSnapshot.LoadAsync(baselinePath);

        // Run again and compare
        var report = await client.CompareBaselineAsync(
            dataset, [new RelevanceEvaluator()], loadedBaseline);

        Assert.NotNull(report);
        Assert.False(report.HasRegressions);
    }

    [Fact]
    public async Task MultipleEvaluators_FullWorkflow()
    {
        var client = new MockChatClient("Water is a chemical compound with formula H2O.");
        var dataset = new GoldenDataset
        {
            Name = "multi-eval",
            Examples =
            [
                new() { Input = "What is water?", ExpectedOutput = "Water is H2O." },
                new() { Input = "Describe water.", ExpectedOutput = "Water is a compound of hydrogen and oxygen." }
            ]
        };

        var evaluators = new IEvaluator[]
        {
            new RelevanceEvaluator(),
            new CoherenceEvaluator(),
            new SafetyEvaluator(),
            new FactualityEvaluator()
        };

        var run = await client.EvaluateAsync(dataset, evaluators);

        Assert.Equal(2, run.Results.Count);
        Assert.All(run.Results, r => Assert.InRange(r.Score, 0.0, 1.0));
        Assert.True(run.PassRate >= 0.0);
    }
}
