namespace ElBruno.AI.Evaluation.Datasets;

/// <summary>
/// Represents a single test case in a golden dataset.
/// </summary>
public sealed class GoldenExample
{
    /// <summary>The input prompt or query.</summary>
    public required string Input { get; init; }

    /// <summary>The expected/reference output.</summary>
    public required string ExpectedOutput { get; init; }

    /// <summary>Optional context provided to the model (e.g. RAG documents).</summary>
    public string? Context { get; init; }

    /// <summary>Tags for categorizing and filtering examples.</summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>Arbitrary key-value metadata for the example.</summary>
    public Dictionary<string, string> Metadata { get; init; } = [];
}

/// <summary>
/// A versioned collection of golden examples used as ground truth for evaluations.
/// </summary>
public sealed class GoldenDataset
{
    /// <summary>Name of the dataset.</summary>
    public required string Name { get; init; }

    /// <summary>Semantic version string.</summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>Human-readable description of the dataset.</summary>
    public string? Description { get; init; }

    /// <summary>When the dataset was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Tags for categorizing the dataset.</summary>
    public List<string> Tags { get; init; } = [];

    /// <summary>The golden examples in this dataset.</summary>
    public List<GoldenExample> Examples { get; init; } = [];

    /// <summary>Adds an example to the dataset.</summary>
    public void AddExample(GoldenExample example)
    {
        ArgumentNullException.ThrowIfNull(example);
        Examples.Add(example);
    }

    /// <summary>Returns examples matching the specified tag.</summary>
    public IReadOnlyList<GoldenExample> GetByTag(string tag) =>
        Examples.Where(e => e.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)).ToList();

    /// <summary>Returns a new dataset containing only examples matching the predicate.</summary>
    public GoldenDataset GetSubset(Func<GoldenExample, bool> predicate) =>
        new()
        {
            Name = Name,
            Version = Version,
            Description = Description,
            CreatedAt = CreatedAt,
            Tags = Tags,
            Examples = Examples.Where(predicate).ToList()
        };

    /// <summary>Returns summary statistics for this dataset.</summary>
    public DatasetSummary GetSummary() =>
        new()
        {
            TotalExamples = Examples.Count,
            UniqueTags = Examples.SelectMany(e => e.Tags).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            ExamplesWithContext = Examples.Count(e => e.Context is not null)
        };
}

/// <summary>
/// Summary statistics for a <see cref="GoldenDataset"/>.
/// </summary>
public sealed class DatasetSummary
{
    /// <summary>Total number of examples.</summary>
    public int TotalExamples { get; init; }

    /// <summary>Distinct tags across all examples.</summary>
    public IReadOnlyList<string> UniqueTags { get; init; } = [];

    /// <summary>Number of examples that include context.</summary>
    public int ExamplesWithContext { get; init; }
}
