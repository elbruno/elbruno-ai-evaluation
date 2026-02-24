namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// Combines multiple <see cref="MetricScore"/> instances into aggregate values.
/// </summary>
public static class AggregateScorer
{
    /// <summary>Computes a weighted average of the provided metric scores.</summary>
    public static double ComputeWeightedAverage(IReadOnlyList<MetricScore> scores)
    {
        if (scores.Count == 0) return 0.0;

        var totalWeight = 0.0;
        var weightedSum = 0.0;
        foreach (var s in scores)
        {
            weightedSum += s.Value * s.Weight;
            totalWeight += s.Weight;
        }

        return totalWeight == 0 ? 0.0 : weightedSum / totalWeight;
    }

    /// <summary>Returns the minimum score value across all provided metrics.</summary>
    public static double ComputeMinimum(IReadOnlyList<MetricScore> scores)
    {
        if (scores.Count == 0) return 0.0;

        var min = double.MaxValue;
        foreach (var s in scores)
        {
            if (s.Value < min) min = s.Value;
        }
        return min;
    }

    /// <summary>Returns the fraction of scores that passed their threshold (0.0 to 1.0).</summary>
    public static double ComputePassRate(IReadOnlyList<MetricScore> scores)
    {
        if (scores.Count == 0) return 0.0;
        var passed = 0;
        foreach (var s in scores)
        {
            if (s.Passed) passed++;
        }
        return (double)passed / scores.Count;
    }
}
