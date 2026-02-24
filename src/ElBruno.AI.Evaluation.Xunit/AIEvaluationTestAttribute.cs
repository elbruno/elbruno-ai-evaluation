using Xunit;

namespace ElBruno.AI.Evaluation.Xunit;

/// <summary>
/// Marks a test method as an AI evaluation test, discoverable in Test Explorer.
/// Extends <see cref="FactAttribute"/> so xUnit treats it as a runnable test.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AIEvaluationTestAttribute : FactAttribute
{
    /// <summary>Path to the golden dataset JSON file used by this test.</summary>
    public string? DatasetPath { get; set; }

    /// <summary>Evaluator types to apply during the test run.</summary>
    public Type[]? Evaluators { get; set; }

    /// <summary>Minimum passing threshold (0.0–1.0). Defaults to 0.7.</summary>
    public double Threshold { get; set; } = 0.7;
}
