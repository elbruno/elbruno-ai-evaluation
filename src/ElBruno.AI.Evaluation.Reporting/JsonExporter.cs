using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Reporting;

/// <summary>
/// Exports evaluation results to JSON format.
/// </summary>
public sealed class JsonExporter
{
    public Task ExportAsync(IEnumerable<EvaluationResult> results, string outputPath, CancellationToken ct = default)
    {
        // TODO: Implement JSON export
        throw new NotImplementedException();
    }
}
