using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Measures response latency and flags slow responses.
/// Not available in Microsoft.Extensions.AI.Evaluation — covers operational metrics.
/// </summary>
public sealed class LatencyEvaluator : IEvaluator
{
    private readonly double _maxAcceptableMs;

    /// <summary>Creates a new <see cref="LatencyEvaluator"/>.</summary>
    /// <param name="maxAcceptableMs">Maximum acceptable response time in milliseconds. Default is 5000.</param>
    public LatencyEvaluator(double maxAcceptableMs = 5000) => _maxAcceptableMs = maxAcceptableMs;

    /// <inheritdoc />
    /// <remarks>
    /// If metadata key "latency_ms" is present in <paramref name="input"/>, it is parsed as pre-measured latency.
    /// Otherwise, the output length is used as a no-op fallback (use the overload with Func for real measurement).
    /// </remarks>
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Try to extract pre-measured latency from input metadata marker
        double elapsedMs = 0;
        const string marker = "[latency_ms:";
        int idx = input.IndexOf(marker, StringComparison.Ordinal);
        if (idx >= 0)
        {
            int end = input.IndexOf(']', idx);
            if (end > idx && double.TryParse(input.AsSpan(idx + marker.Length, end - idx - marker.Length), out double parsed))
                elapsedMs = parsed;
        }

        return Task.FromResult(BuildResult(elapsedMs));
    }

    /// <summary>
    /// Evaluates latency by executing <paramref name="action"/> and measuring elapsed time.
    /// </summary>
    public async Task<EvaluationResult> EvaluateAsync(Func<Task<string>> action, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var sw = System.Diagnostics.Stopwatch.StartNew();
        await action().ConfigureAwait(false);
        sw.Stop();
        return BuildResult(sw.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    /// Evaluates a pre-measured latency value in milliseconds.
    /// </summary>
    public EvaluationResult Evaluate(double elapsedMs) => BuildResult(elapsedMs);

    private EvaluationResult BuildResult(double elapsedMs)
    {
        // Score: 1.0 under threshold, linear decay to 0.0 at 2x threshold
        double score = elapsedMs <= _maxAcceptableMs
            ? 1.0
            : Math.Max(0.0, 1.0 - (elapsedMs - _maxAcceptableMs) / _maxAcceptableMs);

        string details = $"Response took {elapsedMs:F0}ms (threshold: {_maxAcceptableMs:F0}ms)";

        return new EvaluationResult
        {
            Score = Math.Clamp(score, 0.0, 1.0),
            Passed = score >= 0.5,
            Details = details,
            MetricScores = new()
            {
                ["latency"] = new MetricScore { Name = "Latency", Value = score, Threshold = 0.5 }
            }
        };
    }
}
