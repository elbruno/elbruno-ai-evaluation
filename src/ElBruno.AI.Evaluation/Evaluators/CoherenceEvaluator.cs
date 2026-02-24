namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>Evaluates coherence and logical consistency of AI outputs.</summary>
public sealed class CoherenceEvaluator : IEvaluator
{
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        // TODO: Implement coherence evaluation
        throw new NotImplementedException();
    }
}
