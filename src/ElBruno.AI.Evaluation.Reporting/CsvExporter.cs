using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Reporting;

/// <summary>
/// Exports evaluation results to CSV format.
/// </summary>
public sealed class CsvExporter
{
    public Task ExportAsync(IEnumerable<EvaluationResult> results, string outputPath, CancellationToken ct = default)
    {
        // TODO: Implement CSV export
        throw new NotImplementedException();
    }
}
