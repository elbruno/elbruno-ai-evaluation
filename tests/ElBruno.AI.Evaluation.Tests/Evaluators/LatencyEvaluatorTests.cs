using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class LatencyEvaluatorTests
{
    [Fact]
    public async Task UnderThreshold_ScoresOne()
    {
        var evaluator = new LatencyEvaluator(maxAcceptableMs: 5000);
        var result = await evaluator.EvaluateAsync("[latency_ms:2000] q", "a");

        Assert.Equal(1.0, result.Score);
        Assert.True(result.Passed);
    }

    [Theory]
    [InlineData(6000, 5000, 0.8)]  // 1000 over threshold out of 5000 range => 1 - 1000/5000 = 0.8
    [InlineData(7500, 5000, 0.5)]  // 2500 over => 1 - 2500/5000 = 0.5
    [InlineData(9000, 5000, 0.2)]  // 4000 over => 1 - 4000/5000 = 0.2
    public async Task BetweenThresholdAndDouble_DecaysLinearly(double elapsedMs, double maxAcceptableMs, double expectedScore)
    {
        var evaluator = new LatencyEvaluator(maxAcceptableMs: maxAcceptableMs);
        var result = await evaluator.EvaluateAsync($"[latency_ms:{elapsedMs}] q", "a");

        Assert.Equal(expectedScore, result.Score, precision: 2);
    }

    [Fact]
    public async Task AtOrAboveDoubleThreshold_ScoresZero()
    {
        var evaluator = new LatencyEvaluator(maxAcceptableMs: 5000);

        var resultAt = await evaluator.EvaluateAsync("[latency_ms:10000] q", "a");
        var resultAbove = await evaluator.EvaluateAsync("[latency_ms:15000] q", "a");

        Assert.Equal(0.0, resultAt.Score);
        Assert.Equal(0.0, resultAbove.Score);
    }

    [Fact]
    public async Task DefaultThreshold_Is5000ms()
    {
        var evaluator = new LatencyEvaluator();

        // Under default 5000ms => score 1.0
        var result = await evaluator.EvaluateAsync("[latency_ms:3000] q", "a");
        Assert.Equal(1.0, result.Score);

        // At 2x default (10000ms) => score 0.0
        var result2 = await evaluator.EvaluateAsync("[latency_ms:10000] q", "a");
        Assert.Equal(0.0, result2.Score);
    }

    [Fact]
    public async Task Details_IncludesActualTimeAndThreshold()
    {
        var evaluator = new LatencyEvaluator(maxAcceptableMs: 3000);
        var result = await evaluator.EvaluateAsync("[latency_ms:4500] q", "a");

        Assert.Contains("4500", result.Details);
        Assert.Contains("3000", result.Details);
    }
}
