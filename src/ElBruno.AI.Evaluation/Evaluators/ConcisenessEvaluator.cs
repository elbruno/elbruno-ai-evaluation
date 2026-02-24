using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Penalizes overly verbose or too-short responses. Detects common padding phrases.
/// Microsoft has Fluency but NOT conciseness — this fills the gap.
/// </summary>
public sealed class ConcisenessEvaluator : IEvaluator
{
    private readonly int _minWords;
    private readonly int _maxWords;

    private static readonly string[] PaddingPhrases =
    [
        "in conclusion",
        "as i mentioned",
        "it's worth noting that",
        "it is worth noting that",
        "as a matter of fact",
        "needless to say",
        "at the end of the day",
        "it goes without saying",
        "in other words",
        "to summarize",
        "as previously stated",
        "for what it's worth"
    ];

    /// <summary>Creates a new <see cref="ConcisenessEvaluator"/>.</summary>
    /// <param name="minWords">Minimum ideal word count. Default is 20.</param>
    /// <param name="maxWords">Maximum ideal word count. Default is 200.</param>
    public ConcisenessEvaluator(int minWords = 20, int maxWords = 200)
    {
        _minWords = minWords;
        _maxWords = maxWords;
    }

    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(0.0, 0, 0, "Empty response."));

        int wordCount = output.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
        string lower = output.ToLowerInvariant();
        int paddingCount = PaddingPhrases.Count(p => lower.Contains(p, StringComparison.Ordinal));

        // Base score from word range
        double score;
        if (wordCount >= _minWords && wordCount <= _maxWords)
            score = 1.0;
        else if (wordCount < _minWords)
            score = _minWords == 0 ? 1.0 : Math.Max(0.0, (double)wordCount / _minWords);
        else
            score = Math.Max(0.0, 1.0 - (double)(wordCount - _maxWords) / _maxWords);

        // Penalize padding phrases (0.1 per phrase)
        score = Math.Max(0.0, score - paddingCount * 0.1);

        string details = $"Response has {wordCount} words (ideal: {_minWords}-{_maxWords}), {paddingCount} padding phrases detected";

        return Task.FromResult(MakeResult(score, wordCount, paddingCount, details));
    }

    private static EvaluationResult MakeResult(double score, int words, int padding, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= 0.5,
        Details = details,
        MetricScores = new()
        {
            ["conciseness"] = new MetricScore { Name = "Conciseness", Value = score, Threshold = 0.5 }
        }
    };
}
