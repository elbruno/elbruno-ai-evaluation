using System.Text.Json;
using System.Text.Json.Serialization;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Security;

namespace ElBruno.AI.Evaluation.Reporting;

/// <summary>
/// Exports an <see cref="EvaluationRun"/> to a JSON file.
/// </summary>
public sealed class JsonExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Exports the evaluation run to a JSON file at the specified path.</summary>
    /// <param name="run">The evaluation run to export.</param>
    /// <param name="outputPath">Path of the output JSON file.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ExportAsync(EvaluationRun run, string outputPath, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);
        PathValidator.ValidateFilePath(outputPath, nameof(outputPath));

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, run, JsonOptions, ct).ConfigureAwait(false);
    }
}
