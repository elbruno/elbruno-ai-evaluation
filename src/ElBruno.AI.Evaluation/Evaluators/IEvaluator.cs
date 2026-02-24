namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Core interface for all AI evaluators.
/// </summary>
public interface IEvaluator
{
    /// <summary>
    /// Evaluates an AI-generated output against optional expected output.
    /// </summary>
    Task<EvaluationResult> EvaluateAsync(
        string input,
        string output,
        string? expectedOutput = null,
        CancellationToken ct = default);
}
