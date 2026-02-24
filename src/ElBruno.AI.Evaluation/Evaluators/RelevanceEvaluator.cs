using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Measures if the output is relevant to the input query using cosine similarity
/// of key term frequency vectors.
/// </summary>
public sealed class RelevanceEvaluator : IEvaluator
{
    private readonly double _threshold;

    /// <summary>Creates a new <see cref="RelevanceEvaluator"/> with the specified threshold.</summary>
    /// <param name="threshold">Minimum score (0-1) to pass. Default is 0.6.</param>
    public RelevanceEvaluator(double threshold = 0.6) => _threshold = threshold;

    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(0.0, "Input or output is empty — cannot assess relevance."));

        var inputFreq = GetTermFrequency(input);
        var outputFreq = GetTermFrequency(output);

        double score = CosineSimilarity(inputFreq, outputFreq);
        string details = $"Cosine similarity between input and output terms: {score:F3}. " +
                         $"Input terms: {inputFreq.Count}, Output terms: {outputFreq.Count}.";

        return Task.FromResult(MakeResult(score, details));
    }

    private EvaluationResult MakeResult(double score, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= _threshold,
        Details = details,
        MetricScores = new()
        {
            ["relevance"] = new MetricScore { Name = "Relevance", Value = score, Threshold = _threshold }
        }
    };

    private static Dictionary<string, int> GetTermFrequency(string text)
    {
        var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var token in text.Split([' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '"', '\''],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            var lower = token.ToLowerInvariant();
            if (lower.Length <= 2) continue;
            freq[lower] = freq.GetValueOrDefault(lower) + 1;
        }
        return freq;
    }

    private static double CosineSimilarity(Dictionary<string, int> a, Dictionary<string, int> b)
    {
        var allKeys = new HashSet<string>(a.Keys, StringComparer.OrdinalIgnoreCase);
        allKeys.UnionWith(b.Keys);

        double dot = 0, magA = 0, magB = 0;
        foreach (var key in allKeys)
        {
            double va = a.GetValueOrDefault(key);
            double vb = b.GetValueOrDefault(key);
            dot += va * vb;
            magA += va * va;
            magB += vb * vb;
        }

        double denom = Math.Sqrt(magA) * Math.Sqrt(magB);
        return denom == 0 ? 0.0 : dot / denom;
    }
}
