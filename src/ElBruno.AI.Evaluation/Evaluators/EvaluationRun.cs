using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Represents a complete evaluation run with results, timing, and cost information.
/// </summary>
public sealed class EvaluationRun
{
    /// <summary>Unique identifier for this run.</summary>
    public Guid RunId { get; init; } = Guid.NewGuid();

    /// <summary>When the evaluation run started.</summary>
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>When the evaluation completed.</summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Name of the dataset that was evaluated.</summary>
    public required string DatasetName { get; init; }

    /// <summary>Individual results for each example in the dataset.</summary>
    public List<EvaluationResult> Results { get; init; } = [];

    /// <summary>Total tokens consumed during the run, if tracked.</summary>
    public int? TotalTokens { get; set; }

    /// <summary>Estimated cost of the run, if tracked.</summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>Wall-clock duration of the run.</summary>
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;

    /// <summary>Average score across all examples.</summary>
    public double AggregateScore => Results.Count > 0 ? Results.Average(r => r.Score) : 0.0;

    /// <summary>Fraction of results that passed (0.0 to 1.0).</summary>
    public double PassRate => Results.Count > 0 ? (double)Results.Count(r => r.Passed) / Results.Count : 0.0;

    /// <summary>Whether all individual evaluations passed.</summary>
    public bool AllPassed => Results.All(r => r.Passed);

    /// <summary>Creates a <see cref="BaselineSnapshot"/> from this run's results.</summary>
    public BaselineSnapshot ToBaseline()
    {
        var scores = new Dictionary<string, double>();
        foreach (var result in Results)
        {
            foreach (var (name, metric) in result.MetricScores)
            {
                scores[name] = metric.Value;
            }
        }

        return new BaselineSnapshot
        {
            DatasetName = DatasetName,
            CreatedAt = CompletedAt ?? DateTimeOffset.UtcNow,
            Scores = scores,
            AggregateScore = AggregateScore
        };
    }
}
