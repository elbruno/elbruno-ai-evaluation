using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class FactualityEvaluatorTests
{
    private readonly FactualityEvaluator _evaluator = new();

    [Fact]
    public async Task FactualOutput_MatchingExpected_ScoresHigh()
    {
        var result = await _evaluator.EvaluateAsync(
            "What is water?",
            "Water is a chemical compound with formula H2O.",
            "Water is a chemical compound with formula H2O.");

        Assert.True(result.Score >= 0.8);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task NonFactualOutput_DifferentFromExpected_ScoresLower()
    {
        var result = await _evaluator.EvaluateAsync(
            "What is water?",
            "Water is a rare mineral found only on Mars.",
            "Water is a chemical compound with formula H2O found abundantly on Earth.");

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task EmptyOutput_ReturnsZeroScore()
    {
        var result = await _evaluator.EvaluateAsync("q", "", "expected");

        Assert.Equal(0.0, result.Score);
        Assert.False(result.Passed);
    }

    [Fact]
    public async Task NullExpectedOutput_DoesNotThrow()
    {
        var result = await _evaluator.EvaluateAsync("q", "some output", null);
        Assert.InRange(result.Score, 0.0, 1.0);
    }

    [Fact]
    public async Task CustomThreshold_AffectsPassFail()
    {
        var lenient = new FactualityEvaluator(threshold: 0.1);
        var result = await lenient.EvaluateAsync("q", "some words here", "some words there");

        Assert.True(result.Passed);
    }
}
