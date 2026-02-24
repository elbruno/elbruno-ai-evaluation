using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Detects hallucinations by comparing output against expected output and context.
/// Uses keyword overlap to identify claims not grounded in the reference material.
/// </summary>
public sealed class HallucinationEvaluator : IEvaluator
{
    private readonly double _threshold;

    /// <summary>Creates a new <see cref="HallucinationEvaluator"/> with the specified threshold.</summary>
    /// <param name="threshold">Minimum score (0-1) to pass. Default is 0.7.</param>
    public HallucinationEvaluator(double threshold = 0.7) => _threshold = threshold;

    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(1.0, "Output is empty — no hallucination possible."));

        // Build the grounding corpus from expected output and input
        var groundingText = string.Join(" ", input ?? "", expectedOutput ?? "");
        if (string.IsNullOrWhiteSpace(groundingText))
            return Task.FromResult(MakeResult(1.0, "No grounding context provided — skipping hallucination check."));

        var groundingTokens = Tokenize(groundingText);
        var outputTokens = Tokenize(output);

        if (outputTokens.Count == 0)
            return Task.FromResult(MakeResult(1.0, "Output has no meaningful tokens."));

        int grounded = outputTokens.Count(t => groundingTokens.Contains(t));
        double score = (double)grounded / outputTokens.Count;

        int ungrounded = outputTokens.Count - grounded;
        string details = $"Keyword overlap: {grounded}/{outputTokens.Count} tokens grounded ({score:P0}). {ungrounded} potentially hallucinated token(s).";

        return Task.FromResult(MakeResult(score, details));
    }

    private EvaluationResult MakeResult(double score, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= _threshold,
        Details = details,
        MetricScores = new()
        {
            ["hallucination"] = new MetricScore { Name = "Hallucination", Value = score, Threshold = _threshold }
        }
    };

    private static HashSet<string> Tokenize(string text) =>
        new(text.Split([' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\''],
                StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLowerInvariant())
            .Where(t => t.Length > 2),
            StringComparer.OrdinalIgnoreCase);
}
