namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// Detects metric regressions by comparing current scores against a baseline.
/// </summary>
public sealed class RegressionDetector
{
    /// <summary>Configurable tolerance for score drops. Defaults to 0.05 (5%).</summary>
    public double Tolerance { get; init; } = 0.05;

    /// <summary>Returns true if any current score dropped below the baseline by more than the tolerance.</summary>
    public bool HasRegression(BaselineSnapshot baseline, Dictionary<string, double> currentScores)
    {
        return Compare(baseline, currentScores, Tolerance).OverallPassed is false;
    }

    /// <summary>Compares current scores against a baseline snapshot and produces a regression report.</summary>
    public static RegressionReport Compare(BaselineSnapshot baseline, BaselineSnapshot current, double tolerance = 0.05)
    {
        return Compare(baseline, current.Scores, tolerance);
    }

    /// <summary>Compares current scores against a baseline and produces a regression report.</summary>
    public static RegressionReport Compare(BaselineSnapshot baseline, Dictionary<string, double> currentScores, double tolerance = 0.05)
    {
        var improved = new Dictionary<string, (double Baseline, double Current)>();
        var regressed = new Dictionary<string, (double Baseline, double Current)>();
        var unchanged = new Dictionary<string, (double Baseline, double Current)>();

        foreach (var (metric, baselineValue) in baseline.Scores)
        {
            if (!currentScores.TryGetValue(metric, out var currentValue))
            {
                // Missing metric treated as regression to zero
                regressed[metric] = (baselineValue, 0.0);
                continue;
            }

            var delta = currentValue - baselineValue;
            if (delta < -tolerance)
                regressed[metric] = (baselineValue, currentValue);
            else if (delta > tolerance)
                improved[metric] = (baselineValue, currentValue);
            else
                unchanged[metric] = (baselineValue, currentValue);
        }

        return new RegressionReport
        {
            Improved = improved,
            Regressed = regressed,
            Unchanged = unchanged,
            Tolerance = tolerance
        };
    }
}
