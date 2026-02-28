using System.Text.Json;
using ElBruno.AI.Evaluation.Security;

namespace ElBruno.AI.Evaluation.Metrics;

/// <summary>
/// A snapshot of baseline metric values for regression comparison.
/// </summary>
public sealed class BaselineSnapshot
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Name of the dataset this baseline was created from.</summary>
    public required string DatasetName { get; init; }

    /// <summary>When this baseline was created.</summary>
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Per-metric scores captured in this baseline.</summary>
    public Dictionary<string, double> Scores { get; init; } = [];

    /// <summary>Overall aggregate score across all metrics.</summary>
    public double AggregateScore { get; init; }

    /// <summary>Persists this baseline to a JSON file.</summary>
    public async Task SaveAsync(string filePath, CancellationToken cancellationToken = default)
    {
        PathValidator.ValidateFilePath(filePath, nameof(filePath));
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, this, JsonOptions, cancellationToken);
    }

    /// <summary>Loads a baseline from a JSON file.</summary>
    public static async Task<BaselineSnapshot> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        FileIntegrityValidator.ValidateJsonFile(filePath);
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<BaselineSnapshot>(stream, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Failed to deserialize baseline from '{filePath}'.");
    }

    /// <summary>Compares this baseline against another and returns a regression report.</summary>
    public RegressionReport Compare(BaselineSnapshot other, double tolerance = 0.05)
    {
        return RegressionDetector.Compare(baseline: this, current: other, tolerance: tolerance);
    }
}
