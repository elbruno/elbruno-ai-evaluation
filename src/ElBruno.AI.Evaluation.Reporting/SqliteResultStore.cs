using System.Text;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Security;
using Microsoft.Data.Sqlite;

namespace ElBruno.AI.Evaluation.Reporting;

/// <summary>
/// Stores and retrieves evaluation runs in a SQLite database.
/// </summary>
public sealed class SqliteResultStore : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    private SqliteResultStore(SqliteConnection connection)
    {
        _connection = connection;
    }

    /// <summary>Creates or opens a SQLite database at the specified path and ensures the schema exists.</summary>
    /// <param name="dbPath">Path to the SQLite database file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An initialized <see cref="SqliteResultStore"/>.</returns>
    public static async Task<SqliteResultStore> CreateAsync(string dbPath, CancellationToken ct = default)
    {
        FileIntegrityValidator.ValidateDatabaseFile(dbPath);
        var connection = new SqliteConnection($"Data Source={dbPath}");
        await connection.OpenAsync(ct).ConfigureAwait(false);

        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Runs (
                RunId TEXT PRIMARY KEY,
                DatasetName TEXT NOT NULL,
                StartedAt TEXT NOT NULL,
                CompletedAt TEXT,
                AggregateScore REAL NOT NULL,
                PassRate REAL NOT NULL,
                TotalTokens INTEGER,
                EstimatedCost REAL
            );
            CREATE TABLE IF NOT EXISTS Results (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RunId TEXT NOT NULL,
                Input TEXT NOT NULL,
                Output TEXT NOT NULL,
                ExpectedOutput TEXT,
                Score REAL NOT NULL,
                Passed INTEGER NOT NULL,
                Details TEXT,
                FOREIGN KEY (RunId) REFERENCES Runs(RunId)
            );
            """;
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

        return new SqliteResultStore(connection);
    }

    /// <summary>Saves an evaluation run and its results to the database.</summary>
    /// <param name="run">The evaluation run to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task SaveRunAsync(EvaluationRun run, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(run);

        using var transaction = await _connection.BeginTransactionAsync(ct).ConfigureAwait(false);

        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = """
                INSERT INTO Runs (RunId, DatasetName, StartedAt, CompletedAt, AggregateScore, PassRate, TotalTokens, EstimatedCost)
                VALUES ($runId, $datasetName, $startedAt, $completedAt, $aggregateScore, $passRate, $totalTokens, $estimatedCost)
                """;
            cmd.Parameters.AddWithValue("$runId", run.RunId.ToString());
            cmd.Parameters.AddWithValue("$datasetName", run.DatasetName);
            cmd.Parameters.AddWithValue("$startedAt", run.StartedAt.ToString("o"));
            cmd.Parameters.AddWithValue("$completedAt", run.CompletedAt?.ToString("o") ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$aggregateScore", run.AggregateScore);
            cmd.Parameters.AddWithValue("$passRate", run.PassRate);
            cmd.Parameters.AddWithValue("$totalTokens", run.TotalTokens ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("$estimatedCost", run.EstimatedCost.HasValue ? (double)run.EstimatedCost.Value : DBNull.Value);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        foreach (var result in run.Results)
        {
            using var cmd = _connection.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Results (RunId, Input, Output, ExpectedOutput, Score, Passed, Details)
                VALUES ($runId, $input, $output, $expectedOutput, $score, $passed, $details)
                """;
            cmd.Parameters.AddWithValue("$runId", run.RunId.ToString());
            cmd.Parameters.AddWithValue("$input", result.Details); // Details contains evaluator info
            cmd.Parameters.AddWithValue("$output", string.Empty);
            cmd.Parameters.AddWithValue("$expectedOutput", (object?)null ?? DBNull.Value);
            cmd.Parameters.AddWithValue("$score", result.Score);
            cmd.Parameters.AddWithValue("$passed", result.Passed ? 1 : 0);
            cmd.Parameters.AddWithValue("$details", result.Details);
            await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>Retrieves evaluation runs, optionally filtered by dataset name.</summary>
    /// <param name="datasetName">Optional dataset name filter.</param>
    /// <param name="limit">Maximum number of runs to return.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of evaluation runs ordered by most recent first.</returns>
    public async Task<IReadOnlyList<EvaluationRun>> GetRunsAsync(string? datasetName = null, int limit = 50, CancellationToken ct = default)
    {
        using var cmd = _connection.CreateCommand();
        var sb = new StringBuilder("SELECT RunId, DatasetName, StartedAt, CompletedAt, TotalTokens, EstimatedCost FROM Runs");
        if (datasetName is not null)
        {
            sb.Append(" WHERE DatasetName = $datasetName");
            cmd.Parameters.AddWithValue("$datasetName", datasetName);
        }
        sb.Append(" ORDER BY StartedAt DESC LIMIT $limit");
        cmd.Parameters.AddWithValue("$limit", limit);
        cmd.CommandText = sb.ToString();

        var runs = new List<EvaluationRun>();
        using var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var run = new EvaluationRun
            {
                RunId = Guid.Parse(reader.GetString(0)),
                DatasetName = reader.GetString(1),
                StartedAt = DateTimeOffset.Parse(reader.GetString(2)),
                CompletedAt = reader.IsDBNull(3) ? null : DateTimeOffset.Parse(reader.GetString(3)),
                TotalTokens = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                EstimatedCost = reader.IsDBNull(5) ? null : (decimal)reader.GetDouble(5)
            };
            runs.Add(run);
        }

        return runs;
    }

    /// <summary>Gets the most recent run for the given dataset, suitable as a baseline.</summary>
    /// <param name="datasetName">The dataset name to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The latest evaluation run, or null if none exist.</returns>
    public async Task<EvaluationRun?> GetLatestBaselineAsync(string datasetName, CancellationToken ct = default)
    {
        var runs = await GetRunsAsync(datasetName, limit: 1, ct).ConfigureAwait(false);
        return runs.Count > 0 ? runs[0] : null;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}
