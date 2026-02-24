using Xunit;

namespace ElBruno.AI.Evaluation.Tests;

public class SmokeTests
{
    [Fact]
    public void EvaluationResult_CanBeCreated()
    {
        var result = new Evaluators.EvaluationResult
        {
            Score = 0.95,
            Passed = true,
            Details = "Test passed"
        };

        Assert.True(result.Passed);
        Assert.Equal(0.95, result.Score);
    }
}
