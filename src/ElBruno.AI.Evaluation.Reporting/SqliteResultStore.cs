using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Reporting;

/// <summary>
/// Stores evaluation results in a SQLite database.
/// </summary>
public sealed class SqliteResultStore
{
    private readonly string _connectionString;

    public SqliteResultStore(string databasePath)
    {
        _connectionString = $"Data Source={databasePath}";
    }

    public Task SaveAsync(EvaluationResult result, CancellationToken ct = default)
    {
        // TODO: Implement SQLite persistence
        throw new NotImplementedException();
    }
}
