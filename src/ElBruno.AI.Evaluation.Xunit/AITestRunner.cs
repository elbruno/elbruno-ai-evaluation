using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using ElBruno.AI.Evaluation.Security;
using Microsoft.Extensions.AI;

namespace ElBruno.AI.Evaluation.Xunit;

/// <summary>
/// Fluent helper for running AI evaluations from within xUnit test methods.
/// </summary>
public sealed class AITestRunner
{
    private readonly IChatClient _chatClient;
    private readonly List<IEvaluator> _evaluators = [];
    private string? _datasetPath;
    private GoldenDataset? _dataset;

    /// <summary>Creates a new <see cref="AITestRunner"/> using the specified chat client.</summary>
    /// <param name="chatClient">The chat client to evaluate.</param>
    public AITestRunner(IChatClient chatClient)
    {
        _chatClient = chatClient ?? throw new ArgumentNullException(nameof(chatClient));
    }

    /// <summary>Sets the path to a JSON dataset file to load before running.</summary>
    /// <param name="datasetPath">Path to the golden dataset JSON file.</param>
    /// <returns>This runner for fluent chaining.</returns>
    public AITestRunner WithDataset(string datasetPath)
    {
        ArgumentNullException.ThrowIfNull(datasetPath);
        PathValidator.ValidateFilePath(datasetPath, nameof(datasetPath));
        _datasetPath = datasetPath;
        return this;
    }

    /// <summary>Sets a pre-loaded dataset to use for the evaluation run.</summary>
    /// <param name="dataset">The golden dataset.</param>
    /// <returns>This runner for fluent chaining.</returns>
    public AITestRunner WithDataset(GoldenDataset dataset)
    {
        _dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
        return this;
    }

    /// <summary>Adds an evaluator instance to the run.</summary>
    /// <param name="evaluator">The evaluator to add.</param>
    /// <returns>This runner for fluent chaining.</returns>
    public AITestRunner AddEvaluator(IEvaluator evaluator)
    {
        _evaluators.Add(evaluator ?? throw new ArgumentNullException(nameof(evaluator)));
        return this;
    }

    /// <summary>Adds an evaluator by type. The type must have a parameterless constructor.</summary>
    /// <typeparam name="T">The evaluator type implementing <see cref="IEvaluator"/>.</typeparam>
    /// <returns>This runner for fluent chaining.</returns>
    public AITestRunner AddEvaluator<T>() where T : IEvaluator, new()
    {
        _evaluators.Add(new T());
        return this;
    }

    /// <summary>Runs the evaluation and returns the completed <see cref="EvaluationRun"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The evaluation run with all results.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no dataset or evaluators are configured.</exception>
    public async Task<EvaluationRun> RunAsync(CancellationToken ct = default)
    {
        var dataset = await ResolveDatasetAsync(ct).ConfigureAwait(false);
        if (_evaluators.Count == 0)
            throw new InvalidOperationException("At least one evaluator is required. Call AddEvaluator().");

        return await _chatClient.EvaluateAsync(dataset, _evaluators, ct).ConfigureAwait(false);
    }

    private async Task<GoldenDataset> ResolveDatasetAsync(CancellationToken ct)
    {
        if (_dataset is not null)
            return _dataset;

        if (_datasetPath is not null)
        {
            var loader = new JsonDatasetLoader();
            return await loader.LoadAsync(_datasetPath, ct).ConfigureAwait(false);
        }

        throw new InvalidOperationException("No dataset configured. Call WithDataset().");
    }
}
