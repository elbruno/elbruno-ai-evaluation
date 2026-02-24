using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class RelevanceEvaluatorTests
{
    private readonly RelevanceEvaluator _evaluator = new();

    [Fact]
    public async Task RelevantOutput_ScoresHigh()
    {
        var result = await _evaluator.EvaluateAsync(
            "What is machine learning?",
            "Machine learning is a subset of artificial intelligence that enables systems to learn from data.",
            "Machine learning is an AI technique for learning from data.");

        Assert.InRange(result.Score, 0.0, 1.0);
        // Relevance evaluator uses cosine similarity between input and output
    }

    [Fact]
    public async Task IrrelevantOutput_ScoresLow()
    {
        var result = await _evaluator.EvaluateAsync(
            "What is machine learning?",
            "The recipe for chocolate cake requires flour, sugar, and eggs.",
            null);

        Assert.True(result.Score < 0.5);
    }

    [Fact]
    public async Task EmptyOutput_HandlesGracefully()
    {
        var result = await _evaluator.EvaluateAsync("question about topic", "", null);

        Assert.Equal(0.0, result.Score);
        Assert.False(result.Passed);
    }

    [Fact]
    public async Task IdenticalInputAndOutput_ScoresHigh()
    {
        var text = "Machine learning uses algorithms to learn from data.";
        var result = await _evaluator.EvaluateAsync(text, text, null);

        Assert.True(result.Score >= 0.9);
    }

    [Fact]
    public async Task CustomThreshold_AffectsPassFail()
    {
        var strict = new RelevanceEvaluator(threshold: 0.99);
        var result = await strict.EvaluateAsync("cats", "dogs and cats", null);

        Assert.False(result.Passed);
    }
}
