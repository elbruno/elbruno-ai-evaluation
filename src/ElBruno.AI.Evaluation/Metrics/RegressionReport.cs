namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// Report produced by regression detection comparing current scores against a baseline.
/// </summary>
public sealed class RegressionReport
{
    /// <summary>Metrics that improved beyond the tolerance.</summary>
    public required Dictionary<string, (double Baseline, double Current)> Improved { get; init; }

    /// <summary>Metrics that regressed beyond the tolerance.</summary>
    public required Dictionary<string, (double Baseline, double Current)> Regressed { get; init; }

    /// <summary>Metrics that stayed within the tolerance range.</summary>
    public required Dictionary<string, (double Baseline, double Current)> Unchanged { get; init; }

    /// <summary>The tolerance used for comparison.</summary>
    public double Tolerance { get; init; }

    /// <summary>True if no regressions were detected.</summary>
    public bool OverallPassed => Regressed.Count == 0;

    /// <summary>Whether any metric regressed beyond tolerance.</summary>
    public bool HasRegressions => Regressed.Count > 0;
}
