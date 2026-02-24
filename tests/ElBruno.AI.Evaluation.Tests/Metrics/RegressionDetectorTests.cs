using Xunit;
using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Tests.Metrics;

public class RegressionDetectorTests
{
    private static BaselineSnapshot MakeBaseline(Dictionary<string, double> scores) =>
        new() { DatasetName = "test", Scores = scores, AggregateScore = scores.Values.Average() };

    [Fact]
    public void HasRegression_NoDrops_ReturnsFalse()
    {
        var baseline = MakeBaseline(new() { ["accuracy"] = 0.8 });
        var current = new Dictionary<string, double> { ["accuracy"] = 0.8 };

        var detector = new RegressionDetector();
        Assert.False(detector.HasRegression(baseline, current));
    }

    [Fact]
    public void HasRegression_SignificantDrop_ReturnsTrue()
    {
        var baseline = MakeBaseline(new() { ["accuracy"] = 0.9 });
        var current = new Dictionary<string, double> { ["accuracy"] = 0.7 };

        var detector = new RegressionDetector();
        Assert.True(detector.HasRegression(baseline, current));
    }

    [Fact]
    public void Compare_ClassifiesCorrectly()
    {
        var baseline = MakeBaseline(new() { ["a"] = 0.8, ["b"] = 0.5, ["c"] = 0.9 });
        var current = new Dictionary<string, double> { ["a"] = 0.95, ["b"] = 0.51, ["c"] = 0.7 };

        var report = RegressionDetector.Compare(baseline, current, tolerance: 0.05);

        Assert.Single(report.Improved);    // a: 0.8 -> 0.95
        Assert.Single(report.Unchanged);   // b: 0.5 -> 0.51
        Assert.Single(report.Regressed);   // c: 0.9 -> 0.7
        Assert.True(report.HasRegressions);
        Assert.False(report.OverallPassed);
    }

    [Fact]
    public void Compare_MissingMetric_TreatedAsRegression()
    {
        var baseline = MakeBaseline(new() { ["a"] = 0.8, ["b"] = 0.7 });
        var current = new Dictionary<string, double> { ["a"] = 0.8 };

        var report = RegressionDetector.Compare(baseline, current);

        Assert.True(report.Regressed.ContainsKey("b"));
    }

    [Fact]
    public void Compare_CustomTolerance_Respected()
    {
        var baseline = MakeBaseline(new() { ["a"] = 0.8 });
        var current = new Dictionary<string, double> { ["a"] = 0.7 };

        var strict = RegressionDetector.Compare(baseline, current, tolerance: 0.05);
        var lenient = RegressionDetector.Compare(baseline, current, tolerance: 0.15);

        Assert.True(strict.HasRegressions);
        Assert.False(lenient.HasRegressions);
    }
}
