namespace ElBruno.AI.Evaluation.Datasets;

/// <summary>
/// Tracks a specific version of a dataset.
/// </summary>
public sealed class DatasetVersion
{
    /// <summary>Semantic version string.</summary>
    public required string Version { get; init; }

    /// <summary>When this version was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Description of changes in this version.</summary>
    public string? ChangeDescription { get; init; }

    /// <summary>
    /// Computes the diff between two datasets, identifying added, removed, and modified examples.
    /// </summary>
    public static DatasetDiff Diff(GoldenDataset baseline, GoldenDataset updated)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(updated);

        var baselineInputs = baseline.Examples.ToDictionary(e => e.Input);
        var updatedInputs = updated.Examples.ToDictionary(e => e.Input);

        return new DatasetDiff
        {
            Added = updated.Examples.Where(e => !baselineInputs.ContainsKey(e.Input)).ToList(),
            Removed = baseline.Examples.Where(e => !updatedInputs.ContainsKey(e.Input)).ToList(),
            Modified = updated.Examples
                .Where(e => baselineInputs.TryGetValue(e.Input, out var b) && b.ExpectedOutput != e.ExpectedOutput)
                .ToList()
        };
    }
}

/// <summary>
/// Represents differences between two dataset versions.
/// </summary>
public sealed class DatasetDiff
{
    /// <summary>Examples added in the updated dataset.</summary>
    public IReadOnlyList<GoldenExample> Added { get; init; } = [];

    /// <summary>Examples removed from the baseline dataset.</summary>
    public IReadOnlyList<GoldenExample> Removed { get; init; } = [];

    /// <summary>Examples with the same input but different expected output.</summary>
    public IReadOnlyList<GoldenExample> Modified { get; init; } = [];
}
