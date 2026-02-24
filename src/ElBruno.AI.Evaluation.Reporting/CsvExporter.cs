using System.Globalization;
using System.Text;
using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Reporting;

/// <summary>
/// Exports evaluation results to CSV format with one row per example.
/// </summary>
public sealed class CsvExporter
{
    /// <summary>Exports the evaluation run results to a CSV file at the specified path.</summary>
    /// <param name="run">The evaluation run to export.</param>
    /// <param name="outputPath">Path of the output CSV file.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task ExportAsync(EvaluationRun run, string outputPath, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(run);
        ArgumentException.ThrowIfNullOrEmpty(outputPath);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Collect all distinct metric names across results
        var metricNames = run.Results
            .SelectMany(r => r.MetricScores.Keys)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        var sb = new StringBuilder();

        // Header
        sb.Append("Index,Score,Passed,Details");
        foreach (var name in metricNames)
            sb.Append($",{name}");
        sb.AppendLine();

        // Rows
        for (int i = 0; i < run.Results.Count; i++)
        {
            var r = run.Results[i];
            sb.Append(CultureInfo.InvariantCulture, $"{i + 1},{r.Score:F4},{r.Passed},{EscapeCsv(r.Details)}");
            foreach (var name in metricNames)
            {
                var value = r.MetricScores.TryGetValue(name, out var m) ? m.Value.ToString("F4", CultureInfo.InvariantCulture) : "";
                sb.Append($",{value}");
            }
            sb.AppendLine();
        }

        await File.WriteAllTextAsync(outputPath, sb.ToString(), ct).ConfigureAwait(false);
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
