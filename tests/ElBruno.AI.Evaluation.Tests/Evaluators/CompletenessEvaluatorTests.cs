using Xunit;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Tests.Evaluators;

public class CompletenessEvaluatorTests
{
    [Fact]
    public async Task AllQuestionsAddressed_ScoresOne()
    {
        var evaluator = new CompletenessEvaluator();
        var result = await evaluator.EvaluateAsync(
            "What is Python? What is Java?",
            "Python is a programming language. Java is also a programming language.");

        Assert.Equal(1.0, result.Score);
        Assert.True(result.Passed);
    }

    [Fact]
    public async Task PartialCoverage_ReflectsRatio()
    {
        var evaluator = new CompletenessEvaluator();
        var result = await evaluator.EvaluateAsync(
            "What is Python? What is Java? What is Rust?",
            "Python is a programming language. Java is a programming language.");

        // 2 of 3 addressed => ~0.67
        Assert.InRange(result.Score, 0.5, 0.8);
    }

    [Fact]
    public async Task DetectsQuestionsByQuestionMark()
    {
        var evaluator = new CompletenessEvaluator();
        var result = await evaluator.EvaluateAsync(
            "What is AI? How does ML work? Why use neural networks?",
            "AI is artificial intelligence.");

        // Only 1 of 3 questions addressed
        Assert.True(result.Score < 0.5);
    }

    [Fact]
    public async Task SingleQuestion_HandledCorrectly()
    {
        var evaluator = new CompletenessEvaluator();
        var result = await evaluator.EvaluateAsync(
            "What is the capital of France?",
            "The capital of France is Paris.");

        Assert.Equal(1.0, result.Score);
    }

    [Fact]
    public async Task Details_ListAddressedAndMissedTopics()
    {
        var evaluator = new CompletenessEvaluator();
        var result = await evaluator.EvaluateAsync(
            "What is Python? What is Rust?",
            "Python is a programming language.");

        Assert.False(string.IsNullOrWhiteSpace(result.Details));
        // Details should mention what was or wasn't addressed
        Assert.True(result.Details.Length > 10);
    }
}
