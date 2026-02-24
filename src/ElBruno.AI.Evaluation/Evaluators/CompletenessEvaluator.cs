using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Checks if all parts of a multi-part question are answered. Uses heuristic detection
/// of question markers. Microsoft has Completeness but it's LLM-based; ours is offline.
/// </summary>
public sealed class CompletenessEvaluator : IEvaluator
{
    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(input))
            return Task.FromResult(MakeResult(1.0, 0, 0, "No input — nothing to check."));

        var topics = ExtractTopics(input);
        if (topics.Count == 0)
            return Task.FromResult(MakeResult(1.0, 0, 0, "No question markers detected in input."));

        if (string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(0.0, 0, topics.Count, $"Empty response but {topics.Count} topic(s) detected."));

        string outputLower = output.ToLowerInvariant();
        int addressed = 0;
        foreach (var topic in topics)
        {
            // Check if key terms from the topic appear in output
            var keywords = topic.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 3)
                .Select(w => w.ToLowerInvariant())
                .ToList();

            if (keywords.Count == 0 || keywords.Any(k => outputLower.Contains(k, StringComparison.Ordinal)))
                addressed++;
        }

        double score = (double)addressed / topics.Count;
        string details = $"{addressed}/{topics.Count} topics addressed in response.";

        return Task.FromResult(MakeResult(score, addressed, topics.Count, details));
    }

    private static List<string> ExtractTopics(string input)
    {
        var topics = new List<string>();

        // Detect question sentences (ending with ?)
        var sentences = input.Split(['.', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        int qIdx = 0;
        int searchFrom = 0;
        foreach (var sentence in sentences)
        {
            // Check if the original input has '?' after this sentence
            int pos = input.IndexOf(sentence, searchFrom, StringComparison.Ordinal);
            if (pos >= 0)
            {
                int afterPos = pos + sentence.Length;
                if (afterPos < input.Length && input[afterPos] == '?')
                {
                    topics.Add(sentence.Trim());
                    qIdx++;
                }
                searchFrom = pos + 1;
            }
        }

        // Detect numbered list items (1. xxx, 2. xxx)
        foreach (var line in input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = line.TrimStart();
            if (trimmed.Length > 2 && char.IsDigit(trimmed[0]) && (trimmed[1] == '.' || (char.IsDigit(trimmed[1]) && trimmed.Length > 2 && trimmed[2] == '.')))
            {
                var content = trimmed[(trimmed.IndexOf('.') + 1)..].Trim();
                if (content.Length > 0 && !topics.Any(t => t.Contains(content, StringComparison.OrdinalIgnoreCase)))
                    topics.Add(content);
            }
        }

        // Detect "and" conjunctions splitting questions (only if no topics found yet)
        if (topics.Count == 0 && input.Contains('?'))
        {
            var parts = input.Split(" and ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length > 1)
                topics.AddRange(parts.Select(p => p.TrimEnd('?').Trim()).Where(p => p.Length > 3));
        }

        return topics;
    }

    private static EvaluationResult MakeResult(double score, int addressed, int total, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= 0.5,
        Details = details,
        MetricScores = new()
        {
            ["completeness"] = new MetricScore { Name = "Completeness", Value = score, Threshold = 0.5 }
        }
    };
}
