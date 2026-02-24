using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class ConsistencyEvaluatorTests
{
    [Fact]
    public async Task ConsistentResponse_ScoresOne()
    {
        var evaluator = new ConsistencyEvaluator();
        var result = await evaluator.EvaluateAsync(
            "Tell me about Paris.",
            "Paris is the capital of France. Paris is a beautiful city.");

        Assert.Equal(1.0, result.Score);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task OneContradiction_ScoresHalf()
    {
        var evaluator = new ConsistencyEvaluator();
        var result = await evaluator.EvaluateAsync(
            "Tell me about the sky.",
            "The sky is blue. The sky is not blue.");

        Assert.Equal(0.5, result.Score, precision: 2);
    }

    [Fact]
    public async Task TwoOrMoreContradictions_ScoresZero()
    {
        var evaluator = new ConsistencyEvaluator();
        var result = await evaluator.EvaluateAsync(
            "Tell me about weather.",
            "The sky is blue. The sky is not blue. Rain is wet. Rain is not wet.");

        Assert.Equal(0.0, result.Score);
    }

    [Fact]
    public async Task DetectsIsNotPattern()
    {
        var evaluator = new ConsistencyEvaluator();
        var result = await evaluator.EvaluateAsync(
            "Describe water.",
            "Water is essential. Water is not essential.");

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task Details_DescribeContradictions()
    {
        var evaluator = new ConsistencyEvaluator();
        var result = await evaluator.EvaluateAsync(
            "Tell me about cats.",
            "Cats are friendly. Cats are not friendly.");

        Assert.False(string.IsNullOrWhiteSpace(result.Details));
        Assert.Contains("contradiction", result.Details, StringComparison.OrdinalIgnoreCase);
    }
}
