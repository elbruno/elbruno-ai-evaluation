using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using ElBruno.AI.Evaluation.Metrics;
using Microsoft.Extensions.AI;

namespace ElBruno.AI.Evaluation;

/// <summary>
/// Fluent builder for constructing an <see cref="EvaluationPipeline"/>.
/// </summary>
public sealed class EvaluationPipelineBuilder
{
    private IChatClient? _chatClient;
    private GoldenDataset? _dataset;
    private readonly List<IEvaluator> _evaluators = [];
    private BaselineSnapshot? _baseline;

    /// <summary>Sets the chat client to evaluate.</summary>
    public EvaluationPipelineBuilder WithChatClient(IChatClient chatClient)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
        return this;
    }

    /// <summary>Sets the golden dataset to evaluate against.</summary>
    public EvaluationPipelineBuilder WithDataset(GoldenDataset dataset)
    {
        _dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
        return this;
    }

    /// <summary>Adds an evaluator to the pipeline.</summary>
    public EvaluationPipelineBuilder AddEvaluator(IEvaluator evaluator)
    {
        _evaluators.Add(evaluator ?? throw new ArgumentNullException(nameof(evaluator)));
        return this;
    }

    /// <summary>Sets an optional baseline snapshot for regression comparison.</summary>
    public EvaluationPipelineBuilder WithBaseline(BaselineSnapshot baseline)
    {
        _baseline = baseline ?? throw new ArgumentNullException(nameof(baseline));
        return this;
    }

    /// <summary>Builds the configured <see cref="EvaluationPipeline"/>.</summary>
    /// <exception cref="InvalidOperationException">Thrown when required components are missing.</exception>
    public EvaluationPipeline Build()
    {
        if (_chatClient is null) throw new InvalidOperationException("ChatClient is required. Call WithChatClient().");
        if (_dataset is null) throw new InvalidOperationException("Dataset is required. Call WithDataset().");
        if (_evaluators.Count == 0) throw new InvalidOperationException("At least one evaluator is required. Call AddEvaluator().");

        return new EvaluationPipeline(_chatClient, _dataset, _evaluators, _baseline);
    }
}

/// <summary>
/// An evaluation pipeline that runs a chat client against a dataset with configured evaluators.
/// </summary>
public sealed class EvaluationPipeline
{
    private readonly IChatClient _chatClient;
    private readonly GoldenDataset _dataset;
    private readonly IReadOnlyList<IEvaluator> _evaluators;
    private readonly BaselineSnapshot? _baseline;

    internal EvaluationPipeline(
        IChatClient chatClient,
        GoldenDataset dataset,
        IReadOnlyList<IEvaluator> evaluators,
        BaselineSnapshot? baseline)
    {
        _chatClient = chatClient;
        _dataset = dataset;
        _evaluators = evaluators;
        _baseline = baseline;
    }

    /// <summary>
    /// Runs the evaluation pipeline. If a baseline is configured, returns an <see cref="EvaluationRun"/>
    /// that can be compared via <see cref="RunWithBaselineAsync"/>.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The <see cref="EvaluationRun"/> with all results.</returns>
    public Task<EvaluationRun> RunAsync(CancellationToken ct = default)
        => _chatClient.EvaluateAsync(_dataset, _evaluators, ct);

    /// <summary>
    /// Runs the evaluation pipeline and compares results against the configured baseline.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="RegressionReport"/> with comparison details.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no baseline is configured.</exception>
    public Task<RegressionReport> RunWithBaselineAsync(CancellationToken ct = default)
    {
        if (_baseline is null)
            throw new InvalidOperationException("No baseline configured. Call WithBaseline() on the builder.");
        return _chatClient.CompareBaselineAsync(_dataset, _evaluators, _baseline, ct);
    }
}
