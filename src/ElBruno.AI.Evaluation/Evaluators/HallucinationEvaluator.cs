namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>Detects hallucinated content in AI outputs.</summary>
public sealed class HallucinationEvaluator : IEvaluator
{
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        // TODO: Implement hallucination detection
        throw new NotImplementedException();
    }
}
