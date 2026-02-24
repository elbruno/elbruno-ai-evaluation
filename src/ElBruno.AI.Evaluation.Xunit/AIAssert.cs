using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Xunit;

/// <summary>
/// Fluent assertion helpers for AI evaluation results in xUnit tests.
/// Each method throws <see cref="global::Xunit.Sdk.XunitException"/> on failure.
/// </summary>
public static class AIAssert
{
    /// <summary>Asserts that the overall LLM output score meets the given threshold.</summary>
    /// <param name="result">The evaluation result to check.</param>
    /// <param name="threshold">Minimum passing score (0.0–1.0).</param>
    public static void LLMOutputSatisfies(EvaluationResult result, double threshold = 0.7)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Score < threshold)
            throw new global::Xunit.Sdk.XunitException(
                $"LLM output score {result.Score:F3} is below threshold {threshold:F3}. {result.Details}");
    }

    /// <summary>Asserts the current run does not regress beyond tolerance from the baseline.</summary>
    /// <param name="currentRun">The current evaluation run.</param>
    /// <param name="baseline">The baseline snapshot to compare against.</param>
    /// <param name="tolerance">Allowed regression tolerance (0.0–1.0).</param>
    public static void MeetsBaseline(EvaluationRun currentRun, BaselineSnapshot baseline, double tolerance = 0.05)
    {
        ArgumentNullException.ThrowIfNull(currentRun);
        ArgumentNullException.ThrowIfNull(baseline);
        var report = RegressionDetector.Compare(baseline, currentRun.ToBaseline(), tolerance);
        if (report.HasRegressions)
        {
            var regressed = string.Join(", ", report.Regressed.Select(
                kv => $"{kv.Key}: {kv.Value.Baseline:F3} → {kv.Value.Current:F3}"));
            throw new global::Xunit.Sdk.XunitException(
                $"Regression detected (tolerance={tolerance:F3}): {regressed}");
        }
    }

    /// <summary>Asserts that the result has no hallucination failures (Hallucination metric passes).</summary>
    /// <param name="result">The evaluation result to check.</param>
    public static void NoHallucinations(EvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        AssertMetricPasses(result, "Hallucination", "Hallucination detected");
    }

    /// <summary>Asserts that the result passes the Safety evaluator metric.</summary>
    /// <param name="result">The evaluation result to check.</param>
    public static void IsSafe(EvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        AssertMetricPasses(result, "Safety", "Safety check failed");
    }

    /// <summary>Asserts that the result meets the relevance threshold.</summary>
    /// <param name="result">The evaluation result to check.</param>
    /// <param name="threshold">Minimum relevance score (0.0–1.0).</param>
    public static void IsRelevant(EvaluationResult result, double threshold = 0.6)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.MetricScores.TryGetValue("Relevance", out var metric))
        {
            if (metric.Value < threshold)
                throw new global::Xunit.Sdk.XunitException(
                    $"Relevance score {metric.Value:F3} is below threshold {threshold:F3}.");
        }
        else
        {
            throw new global::Xunit.Sdk.XunitException(
                "Relevance metric not found in evaluation result. Did you include RelevanceEvaluator?");
        }
    }

    /// <summary>Asserts that the result passes the Coherence evaluator metric.</summary>
    /// <param name="result">The evaluation result to check.</param>
    public static void IsCoherent(EvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        AssertMetricPasses(result, "Coherence", "Coherence check failed");
    }

    /// <summary>Asserts that all evaluator metrics in the result pass their respective thresholds.</summary>
    /// <param name="result">The evaluation result to check.</param>
    public static void PassesAllEvaluators(EvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var failed = result.MetricScores
            .Where(kv => !kv.Value.Passed)
            .Select(kv => $"{kv.Key}: {kv.Value.Value:F3} (threshold={kv.Value.Threshold:F3})")
            .ToList();

        if (failed.Count > 0)
            throw new global::Xunit.Sdk.XunitException(
                $"Failed evaluators: {string.Join(", ", failed)}");
    }

    private static void AssertMetricPasses(EvaluationResult result, string metricName, string failurePrefix)
    {
        if (result.MetricScores.TryGetValue(metricName, out var metric))
        {
            if (!metric.Passed)
                throw new global::Xunit.Sdk.XunitException(
                    $"{failurePrefix}: score {metric.Value:F3} below threshold {metric.Threshold:F3}.");
        }
        else
        {
            throw new global::Xunit.Sdk.XunitException(
                $"{metricName} metric not found in evaluation result. Did you include the {metricName}Evaluator?");
        }
    }
}
