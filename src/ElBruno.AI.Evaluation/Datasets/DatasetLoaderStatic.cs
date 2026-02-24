using System.Text.Json;
using System.Text.Json.Serialization;

namespace ElBruno.AI.Evaluation.Datasets;

/// <summary>
/// Static utility methods for loading and saving golden datasets from files.
/// Supports JSON and CSV formats with no external dependencies.
/// </summary>
public static class DatasetLoaderStatic
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Loads a <see cref="GoldenDataset"/> from a JSON file.</summary>
    public static async Task<GoldenDataset> LoadFromJsonAsync(string path, CancellationToken ct = default)
    {
        await using var stream = File.OpenRead(path);
        return await JsonSerializer.DeserializeAsync<GoldenDataset>(stream, JsonOptions, ct)
            ?? throw new InvalidOperationException($"Failed to deserialize dataset from '{path}'.");
    }

    /// <summary>Loads a <see cref="GoldenDataset"/> from a CSV file.</summary>
    /// <param name="path">Path to the CSV file.</param>
    /// <param name="inputColumn">Name of the input column. Default is "input".</param>
    /// <param name="outputColumn">Name of the expected output column. Default is "expected_output".</param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task<GoldenDataset> LoadFromCsvAsync(
        string path, string inputColumn = "input", string outputColumn = "expected_output", CancellationToken ct = default)
    {
        var lines = await File.ReadAllLinesAsync(path, ct);
        if (lines.Length == 0)
            throw new InvalidOperationException("CSV file is empty.");

        var headers = ParseCsvLine(lines[0]);
        int inputIdx = headers.IndexOf(inputColumn);
        int outputIdx = headers.IndexOf(outputColumn);

        if (inputIdx < 0) throw new InvalidOperationException($"CSV must contain '{inputColumn}' column.");
        if (outputIdx < 0) throw new InvalidOperationException($"CSV must contain '{outputColumn}' column.");

        var examples = new List<GoldenExample>();
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            var cols = ParseCsvLine(lines[i]);
            examples.Add(new GoldenExample
            {
                Input = cols.ElementAtOrDefault(inputIdx) ?? string.Empty,
                ExpectedOutput = cols.ElementAtOrDefault(outputIdx) ?? string.Empty
            });
        }

        return new GoldenDataset
        {
            Name = Path.GetFileNameWithoutExtension(path),
            Examples = examples
        };
    }

    /// <summary>Saves a <see cref="GoldenDataset"/> to a JSON file.</summary>
    public static async Task SaveToJsonAsync(GoldenDataset dataset, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, dataset, JsonOptions, ct);
    }

    /// <summary>Saves a <see cref="GoldenDataset"/> to a CSV file.</summary>
    public static async Task SaveToCsvAsync(GoldenDataset dataset, string path, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var lines = new List<string> { "input,expected_output,context,tags" };
        foreach (var ex in dataset.Examples)
        {
            string input = EscapeCsv(ex.Input);
            string output = EscapeCsv(ex.ExpectedOutput);
            string context = EscapeCsv(ex.Context ?? "");
            string tags = EscapeCsv(string.Join(";", ex.Tags));
            lines.Add($"{input},{output},{context},{tags}");
        }

        await File.WriteAllLinesAsync(path, lines, ct);
    }

    private static string EscapeCsv(string value) =>
        value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        int start = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
                inQuotes = !inQuotes;
            else if (line[i] == ',' && !inQuotes)
            {
                fields.Add(UnescapeCsv(line[start..i]));
                start = i + 1;
            }
        }
        fields.Add(UnescapeCsv(line[start..]));
        return fields;
    }

    private static string UnescapeCsv(string field)
    {
        field = field.Trim();
        if (field.Length >= 2 && field[0] == '"' && field[^1] == '"')
            field = field[1..^1].Replace("\"\"", "\"");
        return field;
    }
}
