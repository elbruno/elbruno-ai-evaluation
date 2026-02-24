using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Verifies factual accuracy by checking if key claims in the output are supported
/// by the expected output. Score = supported claims / total claims.
/// </summary>
public sealed class FactualityEvaluator : IEvaluator
{
    private readonly double _threshold;

    /// <summary>Creates a new <see cref="FactualityEvaluator"/> with the specified threshold.</summary>
    /// <param name="threshold">Minimum score (0-1) to pass. Default is 0.8.</param>
    public FactualityEvaluator(double threshold = 0.8) => _threshold = threshold;

    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(expectedOutput))
            return Task.FromResult(MakeResult(1.0, "No expected output provided — factuality check skipped."));

        if (string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(0.0, "Output is empty — no claims to verify."));

        // Extract claims as sentences from output
        var claims = ExtractClaims(output);
        if (claims.Count == 0)
            return Task.FromResult(MakeResult(1.0, "No claims extracted from output."));

        var referenceTokens = Tokenize(expectedOutput);
        int supported = 0;
        var unsupportedClaims = new List<string>();

        foreach (var claim in claims)
        {
            var claimTokens = Tokenize(claim);
            if (claimTokens.Count == 0) { supported++; continue; }

            double overlap = (double)claimTokens.Count(t => referenceTokens.Contains(t)) / claimTokens.Count;
            if (overlap >= 0.5)
                supported++;
            else
                unsupportedClaims.Add(claim.Length > 60 ? claim[..57] + "..." : claim);
        }

        double score = (double)supported / claims.Count;
        string details = $"{supported}/{claims.Count} claims supported ({score:P0}).";
        if (unsupportedClaims.Count > 0)
            details += $" Unsupported: [{string.Join("; ", unsupportedClaims.Take(3))}]";

        return Task.FromResult(MakeResult(score, details));
    }

    private EvaluationResult MakeResult(double score, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= _threshold,
        Details = details,
        MetricScores = new()
        {
            ["factuality"] = new MetricScore { Name = "Factuality", Value = score, Threshold = _threshold }
        }
    };

    private static List<string> ExtractClaims(string text) =>
        text.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 3)
            .ToList();

    private static HashSet<string> Tokenize(string text) =>
        new(text.Split([' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\''],
                StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.ToLowerInvariant())
            .Where(t => t.Length > 2),
            StringComparer.OrdinalIgnoreCase);
}
