namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// Represents a single metric measurement with a normalized score between 0 and 1.
/// </summary>
public sealed class MetricScore
{
    /// <summary>Name of the metric (e.g., "Relevance", "Coherence").</summary>
    public required string Name { get; init; }

    /// <summary>Normalized score value between 0.0 and 1.0.</summary>
    public required double Value { get; init; }

    /// <summary>Optional pass/fail threshold. When set, <see cref="Passed"/> is computed against it.</summary>
    public double? Threshold { get; init; }

    /// <summary>Whether the score meets or exceeds the threshold. True when no threshold is set.</summary>
    public bool Passed => Threshold is null || Value >= Threshold;

    /// <summary>Weight used when aggregating multiple scores. Defaults to 1.0.</summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>Timestamp when this score was recorded.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public override string ToString() =>
        $"{Name}={Value:F2}{(Threshold.HasValue ? $" (threshold={Threshold.Value:F2}, {(Passed ? "PASS" : "FAIL")})" : "")}";
}
