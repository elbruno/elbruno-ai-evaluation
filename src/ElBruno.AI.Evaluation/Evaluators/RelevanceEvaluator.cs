namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>Evaluates relevance of AI outputs to the input query.</summary>
public sealed class RelevanceEvaluator : IEvaluator
{
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        // TODO: Implement relevance evaluation
        throw new NotImplementedException();
    }
}
