namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// Detects metric regressions by comparing current scores against a baseline.
/// </summary>
public sealed class RegressionDetector
{
    public bool HasRegression(BaselineSnapshot baseline, Dictionary<string, double> currentScores, double tolerancePercent = 5.0)
    {
        // TODO: Implement regression detection logic
        throw new NotImplementedException();
    }
}
