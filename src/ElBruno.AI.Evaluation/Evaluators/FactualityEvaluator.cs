namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>Evaluates factual accuracy of AI outputs.</summary>
public sealed class FactualityEvaluator : IEvaluator
{
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        // TODO: Implement factuality evaluation
        throw new NotImplementedException();
    }
}
