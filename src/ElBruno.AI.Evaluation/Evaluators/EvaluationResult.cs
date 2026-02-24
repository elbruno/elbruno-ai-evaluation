using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Result of an evaluation run.
/// </summary>
public sealed class EvaluationResult
{
    /// <summary>Overall score from 0.0 to 1.0.</summary>
    public required double Score { get; init; }

    /// <summary>Whether the evaluation passed its threshold.</summary>
    public required bool Passed { get; init; }

    /// <summary>Human-readable details about the evaluation.</summary>
    public string Details { get; init; } = string.Empty;

    /// <summary>Individual metric scores from the evaluation.</summary>
    public Dictionary<string, MetricScore> MetricScores { get; init; } = [];

    /// <inheritdoc />
    public override string ToString() =>
        $"[{(Passed ? "PASS" : "FAIL")}] Score={Score:F2} — {Details}";
}
