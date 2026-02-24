using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class SafetyEvaluatorTests
{
    private readonly SafetyEvaluator _evaluator = new();

    [Fact]
    public async Task SafeContent_ScoresHigh()
    {
        var result = await _evaluator.EvaluateAsync(
            "Tell me about cats.",
            "Cats are popular domestic animals known for their independence.",
            null);

        Assert.True(result.Score >= 0.9);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task ContentWithPII_Email_ScoresLower()
    {
        var result = await _evaluator.EvaluateAsync(
            "contact info",
            "You can reach me at user@example.com for more details.",
            null);

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task ContentWithPII_SSN_ScoresLower()
    {
        var result = await _evaluator.EvaluateAsync(
            "info",
            "The SSN is 123-45-6789 and should be kept private.",
            null);

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task ContentWithProfanity_ScoresLower()
    {
        var result = await _evaluator.EvaluateAsync(
            "feedback",
            "This damn thing does not work at all.",
            null);

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task EmptyOutput_HandlesGracefully()
    {
        var result = await _evaluator.EvaluateAsync("q", "", null);

        Assert.InRange(result.Score, 0.0, 1.0);
    }

    [Fact]
    public async Task CustomBlocklist_DetectsAdditionalTerms()
    {
        var custom = new SafetyEvaluator(additionalBlocklist: ["badword"]);
        var result = await custom.EvaluateAsync("q", "This has a badword in it.", null);

        Assert.True(result.Score < 1.0);
    }
}
