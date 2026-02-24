namespace ElBruno.AI.Evaluation.Datasets;

/// <summary>
/// Represents a single example in a golden dataset.
/// </summary>
public sealed class GoldenExample
{
    public required string Input { get; init; }
    public required string ExpectedOutput { get; init; }
    public string? Context { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
}

/// <summary>
/// A versioned collection of golden examples used as ground truth for evaluations.
/// </summary>
public sealed class GoldenDataset
{
    public required string Name { get; init; }
    public string Version { get; init; } = "1.0.0";
    public IReadOnlyList<GoldenExample> Examples { get; init; } = [];
}
