using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Xunit;

/// <summary>
/// Assertion helpers for AI evaluation results in xUnit tests.
/// </summary>
public static class AIAssert
{
    public static void PassesThreshold(EvaluationResult result, double threshold = 0.7)
    {
        if (result.Score < threshold)
        {
            throw new global::Xunit.Sdk.XunitException(
                $"Evaluation score {result.Score:F3} is below threshold {threshold:F3}. {result.Details}");
        }
    }

    public static void AllMetricsPass(EvaluationResult result)
    {
        var failed = result.MetricScores
            .Where(kv => !kv.Value.Passed)
            .Select(kv => $"{kv.Key}: {kv.Value.Value:F3} < {kv.Value.Threshold:F3}")
            .ToList();

        if (failed.Count > 0)
        {
            throw new global::Xunit.Sdk.XunitException(
                $"Failed metrics: {string.Join(", ", failed)}");
        }
    }
}
