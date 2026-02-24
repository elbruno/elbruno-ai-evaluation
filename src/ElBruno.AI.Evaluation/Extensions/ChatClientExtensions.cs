using Microsoft.Extensions.AI;

namespace ElBruno.AI.Evaluation.Extensions;

/// <summary>
/// Extension methods for IChatClient to simplify evaluation workflows.
/// </summary>
public static class ChatClientExtensions
{
    /// <summary>
    /// Evaluates a chat client response using the specified evaluator.
    /// </summary>
    public static Task<Evaluators.EvaluationResult> EvaluateAsync(
        this IChatClient chatClient,
        Evaluators.IEvaluator evaluator,
        string input,
        string? expectedOutput = null,
        CancellationToken ct = default)
    {
        // TODO: Implement chat client evaluation flow
        throw new NotImplementedException();
    }
}
