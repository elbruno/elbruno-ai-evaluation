using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class HallucinationEvaluatorTests
{
    private readonly HallucinationEvaluator _evaluator = new();

    [Fact]
    public async Task GroundedOutput_ScoresHigh()
    {
        var result = await _evaluator.EvaluateAsync(
            "What is the capital of France?",
            "The capital of France is Paris.",
            "Paris is the capital of France.");

        Assert.True(result.Score > 0.5);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task HallucinatedOutput_ScoresLow()
    {
        var result = await _evaluator.EvaluateAsync(
            "What is the capital of France?",
            "The capital of France is Tokyo, located in Japan near Mount Fuji.",
            "Paris is the capital of France.");

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task EmptyOutput_HandlesGracefully()
    {
        var result = await _evaluator.EvaluateAsync("question", "", "expected answer");

        Assert.InRange(result.Score, 0.0, 1.0);
    }

    [Fact]
    public async Task EmptyExpectedOutput_HandlesGracefully()
    {
        var result = await _evaluator.EvaluateAsync("question", "some output", null);

        // Should still produce a result without throwing
        Assert.InRange(result.Score, 0.0, 1.0);
    }

    [Fact]
    public async Task CustomThreshold_AffectsPassFail()
    {
        var strict = new HallucinationEvaluator(threshold: 0.99);
        var result = await strict.EvaluateAsync(
            "q", "The answer is approximately correct", "The answer is correct");

        Assert.False(result.Passed);
    }
}
