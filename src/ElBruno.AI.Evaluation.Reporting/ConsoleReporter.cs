using ElBruno.AI.Evaluation.Evaluators;

namespace ElBruno.AI.Evaluation.Reporting;

/// <summary>
/// Pretty-prints evaluation results to the console with color and summary statistics.
/// </summary>
public sealed class ConsoleReporter
{
    /// <summary>When true, prints details for every individual example.</summary>
    public bool Verbose { get; set; }

    /// <summary>Reports a single evaluation result to the console.</summary>
    /// <param name="result">The evaluation result to display.</param>
    public void Report(EvaluationResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var icon = result.Passed ? "✅" : "❌";
        Console.WriteLine($"{icon} Score: {result.Score:F2} — {result.Details}");

        if (Verbose)
        {
            foreach (var (name, metric) in result.MetricScores)
            {
                var metricIcon = metric.Passed ? "  ✅" : "  ❌";
                Console.WriteLine($"{metricIcon} {name}: {metric.Value:F3} (threshold={metric.Threshold:F3})");
            }
        }
    }

    /// <summary>Reports an entire evaluation run with summary and optional per-example detail.</summary>
    /// <param name="run">The evaluation run to display.</param>
    public void Report(EvaluationRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        int passed = run.Results.Count(r => r.Passed);
        int total = run.Results.Count;
        double pct = total > 0 ? (double)passed / total * 100 : 0;
        var summaryIcon = run.AllPassed ? "✅" : "❌";
        Console.WriteLine($"{summaryIcon} {passed}/{total} passed ({pct:F0}%) | Avg score: {run.AggregateScore:F2}");

        // Per-evaluator breakdown
        var metricGroups = run.Results
            .SelectMany(r => r.MetricScores)
            .GroupBy(kv => kv.Key)
            .ToList();

        foreach (var group in metricGroups)
        {
            var avgScore = group.Average(kv => kv.Value.Value);
            var passCount = group.Count(kv => kv.Value.Passed);
            Console.WriteLine($"  {group.Key}: avg={avgScore:F3}, passed={passCount}/{group.Count()}");
        }

        // Failures highlighted
        var failures = run.Results.Where(r => !r.Passed).ToList();
        if (failures.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine($"--- Failures ({failures.Count}) ---");
            foreach (var fail in failures)
                Console.WriteLine($"  ❌ Score={fail.Score:F3}: {fail.Details}");
        }

        // Verbose: each example
        if (Verbose)
        {
            Console.WriteLine();
            Console.WriteLine("--- All Results ---");
            for (int i = 0; i < run.Results.Count; i++)
            {
                var r = run.Results[i];
                var icon = r.Passed ? "✅" : "❌";
                Console.WriteLine($"  [{i + 1}] {icon} Score={r.Score:F3}: {r.Details}");
            }
        }
    }
}
