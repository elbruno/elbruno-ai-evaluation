using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class ConcisenessEvaluatorTests
{
    [Fact]
    public async Task WithinIdealRange_ScoresOne()
    {
        var evaluator = new ConcisenessEvaluator();
        // 50 words — within default 20-200 range
        var response = string.Join(" ", Enumerable.Repeat("word", 50));
        var result = await evaluator.EvaluateAsync("q", response);

        Assert.Equal(1.0, result.Score);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task ShorterThanMin_ScoreDecays()
    {
        var evaluator = new ConcisenessEvaluator();
        // 5 words — well under default min of 20
        var result = await evaluator.EvaluateAsync("q", "one two three four five");

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task LongerThanMax_ScoreDecays()
    {
        var evaluator = new ConcisenessEvaluator();
        // 400 words — well over default max of 200
        var response = string.Join(" ", Enumerable.Repeat("word", 400));
        var result = await evaluator.EvaluateAsync("q", response);

        Assert.True(result.Score < 1.0);
    }

    [Fact]
    public async Task PaddingPhrases_ReduceScore()
    {
        var evaluator = new ConcisenessEvaluator();
        // Response within word range but with padding phrases
        var padded = "In conclusion, it's worth noting that needless to say " +
                     "the answer is 42. " +
                     string.Join(" ", Enumerable.Repeat("context", 30));
        var clean = "The answer is 42. " + string.Join(" ", Enumerable.Repeat("context", 30));

        var paddedResult = await evaluator.EvaluateAsync("q", padded);
        var cleanResult = await evaluator.EvaluateAsync("q", clean);

        Assert.True(paddedResult.Score < cleanResult.Score);
    }

    [Fact]
    public async Task CustomWordRange_Works()
    {
        var evaluator = new ConcisenessEvaluator(minWords: 10, maxWords: 50);

        // 30 words — within custom range
        var inRange = string.Join(" ", Enumerable.Repeat("word", 30));
        var result = await evaluator.EvaluateAsync("q", inRange);
        Assert.Equal(1.0, result.Score);

        // 100 words — over custom max
        var overMax = string.Join(" ", Enumerable.Repeat("word", 100));
        var result2 = await evaluator.EvaluateAsync("q", overMax);
        Assert.True(result2.Score < 1.0);
    }
}
