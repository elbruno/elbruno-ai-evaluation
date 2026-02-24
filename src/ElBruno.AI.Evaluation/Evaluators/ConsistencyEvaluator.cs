using System.Text.RegularExpressions;
using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Checks if a response contradicts itself by detecting contradictory statement pairs
/// and numerical inconsistencies. A refinement beyond Microsoft's Coherence evaluator,
/// which measures logical flow but not self-contradiction.
/// </summary>
public sealed partial class ConsistencyEvaluator : IEvaluator
{
    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(1.0, 0, "Empty response — no contradictions possible."));

        var sentences = output.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => s.Length > 3).ToList();

        int contradictions = 0;
        contradictions += DetectNegationContradictions(sentences);
        contradictions += DetectNumericalInconsistencies(sentences);

        // Score: 1.0 = none, 0.5 = 1 contradiction, 0.0 = 2+
        double score = contradictions switch
        {
            0 => 1.0,
            1 => 0.5,
            _ => 0.0
        };

        string details = contradictions == 0
            ? "No contradictions detected."
            : $"{contradictions} contradiction(s) detected in {sentences.Count} sentences.";

        return Task.FromResult(MakeResult(score, contradictions, details));
    }

    private static int DetectNegationContradictions(List<string> sentences)
    {
        int count = 0;
        for (int i = 0; i < sentences.Count; i++)
        {
            for (int j = i + 1; j < sentences.Count; j++)
            {
                if (AreContradictory(sentences[i], sentences[j]))
                    count++;
            }
        }
        return count;
    }

    private static bool AreContradictory(string a, string b)
    {
        var wordsA = Tokenize(a);
        var wordsB = Tokenize(b);

        // Pattern: "X is Y" vs "X is not Y"
        for (int i = 0; i < wordsA.Length - 2; i++)
        {
            if (!string.Equals(wordsA[i + 1], "is", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(wordsA[i + 1], "are", StringComparison.OrdinalIgnoreCase))
                continue;

            string subject = wordsA[i];
            string verb = wordsA[i + 1];
            string predicate = wordsA[i + 2];

            for (int k = 0; k < wordsB.Length - 3; k++)
            {
                if (string.Equals(wordsB[k], subject, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(wordsB[k + 1], verb, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(wordsB[k + 2], "not", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(wordsB[k + 3], predicate, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
        }
        return false;
    }

    private static int DetectNumericalInconsistencies(List<string> sentences)
    {
        // Find "entity ... number" patterns and check for conflicts
        var entityNumbers = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var numRegex = NumberPattern();

        foreach (var sentence in sentences)
        {
            var matches = numRegex.Matches(sentence);
            if (matches.Count == 0) continue;

            var words = Tokenize(sentence);
            foreach (Match m in matches)
            {
                int pos = sentence.IndexOf(m.Value, StringComparison.Ordinal);
                // Use preceding word as entity context
                string preceding = words.LastOrDefault(w =>
                    sentence.IndexOf(w, StringComparison.OrdinalIgnoreCase) < pos &&
                    !double.TryParse(w, out _)) ?? "";
                if (preceding.Length <= 2) continue;

                if (!entityNumbers.TryGetValue(preceding, out var nums))
                    entityNumbers[preceding] = nums = [];
                nums.Add(m.Value);
            }
        }

        return entityNumbers.Values.Count(v => v.Count > 1);
    }

    private static string[] Tokenize(string text) =>
        text.Split([' ', ',', ';', ':', '(', ')', '"', '\''], StringSplitOptions.RemoveEmptyEntries);

    [GeneratedRegex(@"\b\d+(?:\.\d+)?\b")]
    private static partial Regex NumberPattern();

    private static EvaluationResult MakeResult(double score, int contradictions, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= 0.5,
        Details = details,
        MetricScores = new()
        {
            ["consistency"] = new MetricScore { Name = "Consistency", Value = score, Threshold = 0.5 }
        }
    };
}
