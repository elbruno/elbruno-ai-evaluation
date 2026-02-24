using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Evaluators;

/// <summary>
/// Evaluates cost-effectiveness of AI responses based on estimated token usage.
/// Not available in Microsoft.Extensions.AI.Evaluation — they track no cost metrics.
/// </summary>
public sealed class CostEvaluator : IEvaluator
{
    private readonly double _maxCostPerResponse;
    private readonly double _tokenCostRate;

    /// <summary>Creates a new <see cref="CostEvaluator"/>.</summary>
    /// <param name="maxCostPerResponse">Maximum acceptable cost per response in dollars. Default is 0.01.</param>
    /// <param name="tokenCostRate">Cost per 1K tokens in dollars. Default is 0.002.</param>
    public CostEvaluator(double maxCostPerResponse = 0.01, double tokenCostRate = 0.002)
    {
        _maxCostPerResponse = maxCostPerResponse;
        _tokenCostRate = tokenCostRate;
    }

    /// <inheritdoc />
    public Task<EvaluationResult> EvaluateAsync(
        string input, string output, string? expectedOutput = null, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(output))
            return Task.FromResult(MakeResult(1.0, 0, 0, "Empty response — no cost."));

        // Estimate tokens: words * 1.3
        int wordCount = output.Split([' ', '\t', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
        double estimatedTokens = wordCount * 1.3;
        double estimatedCost = (estimatedTokens / 1000.0) * _tokenCostRate;

        // Score: 1.0 if under budget, linear decay to 0.0 at 2x budget
        double score = estimatedCost <= _maxCostPerResponse
            ? 1.0
            : Math.Max(0.0, 1.0 - (estimatedCost - _maxCostPerResponse) / _maxCostPerResponse);

        string details = $"Estimated {estimatedTokens:F0} tokens, cost ${estimatedCost:F4} (budget: ${_maxCostPerResponse:F4})";

        return Task.FromResult(MakeResult(score, estimatedTokens, estimatedCost, details));
    }

    private static EvaluationResult MakeResult(double score, double tokens, double cost, string details) => new()
    {
        Score = Math.Clamp(score, 0.0, 1.0),
        Passed = score >= 0.5,
        Details = details,
        MetricScores = new()
        {
            ["cost"] = new MetricScore { Name = "Cost", Value = score, Threshold = 0.5 }
        }
    };
}
