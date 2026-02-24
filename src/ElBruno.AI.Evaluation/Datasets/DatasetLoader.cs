using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElBruno.AI.Evaluation.Datasets;

/// <summary>
/// Loads and saves golden datasets from various sources.
/// </summary>
public interface IDatasetLoader
{
    /// <summary>Loads a dataset from a JSON file.</summary>
    Task<GoldenDataset> LoadAsync(string filePath, CancellationToken ct = default);

    /// <summary>Saves a dataset to a JSON file.</summary>
    Task SaveAsync(GoldenDataset dataset, string filePath, CancellationToken ct = default);

    /// <summary>Loads a dataset from a CSV file with Input, ExpectedOutput, Context, Tags columns.</summary>
    Task<GoldenDataset> LoadFromCsvAsync(string filePath, CancellationToken ct = default);
}

/// <summary>
/// JSON-based implementation of <see cref="IDatasetLoader"/>.
/// </summary>
public sealed class JsonDatasetLoader : IDatasetLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public async Task<GoldenDataset> LoadAsync(string filePath, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<GoldenDataset>(stream, JsonOptions, ct)
            ?? throw new InvalidOperationException($"Failed to deserialize dataset from '{filePath}'.");
    }

    /// <inheritdoc />
    public async Task SaveAsync(GoldenDataset dataset, string filePath, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, dataset, JsonOptions, ct);
    }

    /// <inheritdoc />
    public async Task<GoldenDataset> LoadFromCsvAsync(string filePath, CancellationToken ct = default)
    {
        var lines = await File.ReadAllLinesAsync(filePath, ct);
        if (lines.Length == 0)
            throw new InvalidOperationException("CSV file is empty.");

        var headers = lines[0].Split(',');
        int inputIdx = Array.IndexOf(headers, "Input");
        int expectedIdx = Array.IndexOf(headers, "ExpectedOutput");
        int contextIdx = Array.IndexOf(headers, "Context");
        int tagsIdx = Array.IndexOf(headers, "Tags");

        if (inputIdx < 0 || expectedIdx < 0)
            throw new InvalidOperationException("CSV must contain 'Input' and 'ExpectedOutput' columns.");

        var examples = new List<GoldenExample>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = lines[i].Split(',');

            examples.Add(new GoldenExample
            {
                Input = cols.ElementAtOrDefault(inputIdx) ?? string.Empty,
                ExpectedOutput = cols.ElementAtOrDefault(expectedIdx) ?? string.Empty,
                Context = contextIdx >= 0 ? cols.ElementAtOrDefault(contextIdx) : null,
                Tags = tagsIdx >= 0 && cols.Length > tagsIdx
                    ? cols[tagsIdx].Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : []
            });
        }

        return new GoldenDataset
        {
            Name = Path.GetFileNameWithoutExtension(filePath),
            Examples = examples
        };
    }
}
