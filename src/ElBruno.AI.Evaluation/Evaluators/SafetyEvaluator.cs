namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>Evaluates safety and content policy compliance of AI outputs.</summary>
public sealed class SafetyEvaluator : IEvaluator
{
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        // TODO: Implement safety evaluation
        throw new NotImplementedException();
    }
}
