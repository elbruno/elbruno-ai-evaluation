namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// A snapshot of baseline metric values for regression comparison.
/// </summary>
public sealed class BaselineSnapshot
{
    public required string Name { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public Dictionary<string, double> Scores { get; init; } = [];
}
