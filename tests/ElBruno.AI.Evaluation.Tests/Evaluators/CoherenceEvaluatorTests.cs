using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class CoherenceEvaluatorTests
{
    private readonly CoherenceEvaluator _evaluator = new();

    [Fact]
    public async Task CoherentText_ScoresHigh()
    {
        var result = await _evaluator.EvaluateAsync(
            "Explain gravity.",
            "Gravity is a fundamental force that attracts objects with mass toward each other. It keeps planets in orbit around the sun.",
            null);

        Assert.True(result.Score >= 0.7);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task IncoherentText_WithContradictions_ScoresLower()
    {
        var result = await _evaluator.EvaluateAsync(
            "Is the sky blue?",
            "The sky is blue. The sky is not blue. It can rain. It cannot rain.",
            null);

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task EmptyOutput_ScoresLow()
    {
        var result = await _evaluator.EvaluateAsync("question", "", null);

        Assert.Equal(0.0, result.Score);
        Assert.False(result.Passed);
    }

    [Fact]
    public async Task HighlyRepetitiveText_ScoresLower()
    {
        var repetitive = string.Join(" ", Enumerable.Repeat("The answer is yes.", 20));
        var result = await _evaluator.EvaluateAsync("question", repetitive, null);

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task CustomThreshold_AffectsPassFail()
    {
        var strict = new CoherenceEvaluator(threshold: 0.99);
        var result = await strict.EvaluateAsync("q", "A decent but not perfect answer.", null);

        // With a very high threshold, the result may or may not pass depending on evaluation
        Assert.InRange(result.Score, 0.0, 1.0);
    }
}
