using Xunit;
using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Tests.Metrics;

public class BaselineSnapshotTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    public BaselineSnapshotTests() => Directory.CreateDirectory(_tempDir);
    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesData()
    {
        var path = Path.Combine(_tempDir, "baseline.json");
        var snapshot = new BaselineSnapshot
        {
            DatasetName = "test-ds",
            Scores = new() { ["accuracy"] = 0.85, ["relevance"] = 0.92 },
            AggregateScore = 0.885
        };

        await snapshot.SaveAsync(path);
        var loaded = await BaselineSnapshot.LoadAsync(path);

        Assert.Equal("test-ds", loaded.DatasetName);
        Assert.Equal(0.85, loaded.Scores["accuracy"]);
        Assert.Equal(0.92, loaded.Scores["relevance"]);
        Assert.Equal(0.885, loaded.AggregateScore);
    }

    [Fact]
    public async Task LoadAsync_MissingFile_Throws()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => BaselineSnapshot.LoadAsync(Path.Combine(_tempDir, "missing.json")));
    }

    [Fact]
    public void Compare_ReturnsRegressionReport()
    {
        var baseline = new BaselineSnapshot
        {
            DatasetName = "ds",
            Scores = new() { ["a"] = 0.5, ["b"] = 0.9 }
        };
        var current = new BaselineSnapshot
        {
            DatasetName = "ds",
            Scores = new() { ["a"] = 0.8, ["b"] = 0.7 }
        };

        var report = baseline.Compare(current);

        Assert.True(report.HasRegressions);
        Assert.Contains("b", report.Regressed.Keys);
        Assert.Contains("a", report.Improved.Keys);
    }

    [Fact]
    public void Compare_NoRegressions_OverallPassed()
    {
        var baseline = new BaselineSnapshot { DatasetName = "ds", Scores = new() { ["a"] = 0.8 } };
        var current = new BaselineSnapshot { DatasetName = "ds", Scores = new() { ["a"] = 0.85 } };

        var report = baseline.Compare(current);

        Assert.True(report.OverallPassed);
        Assert.False(report.HasRegressions);
    }
}
