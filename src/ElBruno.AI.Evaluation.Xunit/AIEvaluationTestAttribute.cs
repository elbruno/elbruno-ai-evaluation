namespace ElBruno.AI.Evaluation.Xunit;

/// <summary>
/// Marks a test method as an AI evaluation test.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AIEvaluationTestAttribute : Attribute
{
    public double MinScore { get; set; } = 0.7;
    public string? EvaluatorType { get; set; }
}
