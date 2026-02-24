using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Checks structural coherence of AI outputs: sentence completion, logical flow,
/// and absence of contradictions.
/// </summary>
public sealed class CoherenceEvaluator : IEvaluator
{
    private readonly double _threshold;

    private static readonly string[] ContradictionPairs =
    [
        "is|is not", "was|was not", "can|cannot", "will|will not",
        "true|false", "yes|no", "always|never", "all|none",
        "increase|decrease", "before|after"
    ];

    /// <summary>Creates a new <see cref="CoherenceEvaluator"/> with the specified threshold.</summary>
    /// <param name="threshold">Minimum score (0-1) to pass. Default is 0.7.</param>
    public CoherenceEvaluator(double threshold = 0.7) => _threshold = threshold;

    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(0.0, "Output is empty — no coherence to evaluate."));

        var sentences = output.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

        var issues = new List<string>();
        double score = 1.0;

        // Check 1: Sentence completeness (each sentence should have >= 3 words)
        int incomplete = sentences.Count(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 3);
        if (incomplete > 0)
        {
            double penalty = 0.15 * incomplete;
            score -= penalty;
            issues.Add($"{incomplete} incomplete sentence(s)");
        }

        // Check 2: Contradiction detection
        var lowerOutput = output.ToLowerInvariant();
        int contradictions = 0;
        foreach (var pair in ContradictionPairs)
        {
            var parts = pair.Split('|');
            if (lowerOutput.Contains(parts[0]) && lowerOutput.Contains(parts[1]))
                contradictions++;
        }
        if (contradictions > 0)
        {
            score -= 0.1 * contradictions;
            issues.Add($"{contradictions} potential contradiction(s)");
        }

        // Check 3: Excessive repetition
        if (sentences.Count >= 2)
        {
            var distinct = sentences.Select(s => s.ToLowerInvariant().Trim()).Distinct().Count();
            double repetitionRatio = 1.0 - ((double)distinct / sentences.Count);
            if (repetitionRatio > 0.3)
            {
                score -= 0.2;
                issues.Add($"high repetition ({repetitionRatio:P0})");
            }
        }

        score = Math.Clamp(score, 0.0, 1.0);
        string details = issues.Count == 0
            ? $"Output is coherent ({sentences.Count} sentence(s), no issues detected)."
            : $"Issues: {string.Join(", ", issues)}. Final score: {score:F2}.";

        return Task.FromResult(MakeResult(score, details));
    }

    private EvaluationResult MakeResult(double score, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= _threshold,
        Details = details,
        MetricScores = new()
        {
            ["coherence"] = new MetricScore { Name = "Coherence", Value = score, Threshold = _threshold }
        }
    };
}
