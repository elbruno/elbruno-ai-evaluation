using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Metrics;
using Microsoft.Extensions.AI;

namespace ElBruno.AI.Evaluation.Extensions;

/// <summary>
/// Extension methods for <see cref="IChatClient"/> to simplify evaluation workflows.
/// </summary>
public static class ChatClientExtensions
{
    /// <summary>
    /// Evaluates a single input against the chat client using the specified evaluators.
    /// </summary>
    /// <param name="chatClient">The chat client to evaluate.</param>
    /// <param name="example">The golden example containing input and expected output.</param>
    /// <param name="evaluators">The evaluators to run against the response.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An aggregated <see cref="EvaluationResult"/>.</returns>
    public static async Task<EvaluationResult> EvaluateAsync(
        this IChatClient chatClient,
        GoldenExample example,
        IEnumerable<IEvaluator> evaluators,
        CancellationToken ct = default)
    {
        var response = await chatClient.GetResponseAsync(example.Input, cancellationToken: ct).ConfigureAwait(false);
        var output = response.Text ?? string.Empty;

        var allMetrics = new Dictionary<string, MetricScore>();
        double totalScore = 0;
        int count = 0;
        bool allPassed = true;
        var details = new List<string>();

        foreach (var evaluator in evaluators)
        {
            var result = await evaluator.EvaluateAsync(example.Input, output, example.ExpectedOutput, ct).ConfigureAwait(false);
            totalScore += result.Score;
            count++;
            allPassed &= result.Passed;
            if (!string.IsNullOrEmpty(result.Details))
                details.Add(result.Details);

            foreach (var kvp in result.MetricScores)
                allMetrics[kvp.Key] = kvp.Value;
        }

        return new EvaluationResult
        {
            Score = count > 0 ? totalScore / count : 0.0,
            Passed = allPassed,
            Details = string.Join("; ", details),
            MetricScores = allMetrics
        };
    }

    /// <summary>
    /// Evaluates a chat client against an entire golden dataset using the specified evaluators.
    /// </summary>
    /// <param name="chatClient">The chat client to evaluate.</param>
    /// <param name="dataset">The golden dataset containing examples.</param>
    /// <param name="evaluators">The evaluators to run against each response.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An <see cref="EvaluationRun"/> containing all results.</returns>
    public static async Task<EvaluationRun> EvaluateAsync(
        this IChatClient chatClient,
        GoldenDataset dataset,
        IEnumerable<IEvaluator> evaluators,
        CancellationToken ct = default)
    {
        var evaluatorList = evaluators.ToList();
        var results = new List<EvaluationResult>();

        foreach (var example in dataset.Examples)
        {
            var result = await chatClient.EvaluateAsync(example, evaluatorList, ct).ConfigureAwait(false);
            results.Add(result);
        }

        return new EvaluationRun
        {
            DatasetName = dataset.Name,
            StartedAt = DateTimeOffset.UtcNow,
            Results = results
        };
    }

    /// <summary>
    /// Evaluates a chat client against a dataset and compares results to a baseline snapshot.
    /// </summary>
    /// <param name="chatClient">The chat client to evaluate.</param>
    /// <param name="dataset">The golden dataset containing examples.</param>
    /// <param name="evaluators">The evaluators to run against each response.</param>
    /// <param name="baseline">The baseline snapshot to compare against.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="RegressionReport"/> with comparison details.</returns>
    public static async Task<RegressionReport> CompareBaselineAsync(
        this IChatClient chatClient,
        GoldenDataset dataset,
        IEnumerable<IEvaluator> evaluators,
        BaselineSnapshot baseline,
        CancellationToken ct = default)
    {
        var run = await chatClient.EvaluateAsync(dataset, evaluators, ct).ConfigureAwait(false);

        // Aggregate current scores by metric name
        var currentScores = new Dictionary<string, double>();
        foreach (var result in run.Results)
        {
            foreach (var kvp in result.MetricScores)
            {
                if (!currentScores.ContainsKey(kvp.Key))
                    currentScores[kvp.Key] = 0;
                currentScores[kvp.Key] += kvp.Value.Value;
            }
        }

        if (run.Results.Count > 0)
        {
            foreach (var key in currentScores.Keys.ToList())
                currentScores[key] /= run.Results.Count;
        }

        return RegressionDetector.Compare(baseline, currentScores);
    }
}
