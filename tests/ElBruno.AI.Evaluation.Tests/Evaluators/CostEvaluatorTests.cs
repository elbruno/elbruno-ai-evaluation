using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class CostEvaluatorTests
{
    [Fact]
    public async Task UnderBudget_ScoresOne()
    {
        var evaluator = new CostEvaluator(maxCostPerResponse: 0.01);
        var result = await evaluator.EvaluateAsync("short question", "short answer");

        Assert.Equal(1.0, result.Score);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task OverBudget_ScoreDecays()
    {
        // Very tight budget should cause decay for a longer response
        var evaluator = new CostEvaluator(maxCostPerResponse: 0.0001);
        var longResponse = string.Join(" ", Enumerable.Repeat("word", 500));
        var result = await evaluator.EvaluateAsync("q", longResponse);

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task TokenEstimation_UsesWordCountTimes1Point3()
    {
        // 100 words * 1.3 = 130 tokens expected
        var evaluator = new CostEvaluator(maxCostPerResponse: 1.0);
        var words100 = string.Join(" ", Enumerable.Repeat("hello", 100));
        var result = await evaluator.EvaluateAsync("q", words100);

        Assert.Contains("130", result.Details); // 100 * 1.3 = 130 tokens
    }

    [Fact]
    public async Task Details_IncludeTokenCountAndCostEstimate()
    {
        var evaluator = new CostEvaluator(maxCostPerResponse: 0.01);
        var result = await evaluator.EvaluateAsync("question here", "answer here with words");

        Assert.Contains("token", result.Details, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("cost", result.Details, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CustomTokenCostRate_Works()
    {
        var cheap = new CostEvaluator(maxCostPerResponse: 0.001, tokenCostRate: 0.001);
        var expensive = new CostEvaluator(maxCostPerResponse: 0.001, tokenCostRate: 1.0);

        var response = string.Join(" ", Enumerable.Repeat("word", 200));
        var cheapResult = await cheap.EvaluateAsync("q", response);
        var expensiveResult = await expensive.EvaluateAsync("q", response);

        Assert.True(cheapResult.Score > expensiveResult.Score);
    }
}
