namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// Represents a single metric measurement.
/// </summary>
public sealed class MetricScore
{
    public required string Name { get; init; }
    public required double Value { get; init; }
    public double? Threshold { get; init; }
    public bool Passed => Threshold is null || Value >= Threshold;
}
