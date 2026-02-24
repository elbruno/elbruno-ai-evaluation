using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.SyntheticData.Generators;
using ElBruno.AI.Evaluation.SyntheticData.Strategies;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using Microsoft.Extensions.AI;

namespace ElBruno.AI.Evaluation.SyntheticData;

/// <summary>
/// Fluent builder for generating synthetic golden datasets.
/// </summary>
public sealed class SyntheticDatasetBuilder
{
    private readonly string _datasetName;
    private string _version = "1.0.0";
    private string? _description;
    private readonly List<string> _tags = [];
    private ISyntheticDataGenerator? _generator;
    private int _count;

    /// <summary>
    /// Creates a new SyntheticDatasetBuilder with the specified dataset name.
    /// </summary>
    public SyntheticDatasetBuilder(string datasetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(datasetName);
        _datasetName = datasetName;
    }

    /// <summary>Sets the semantic version of the dataset.</summary>
    public SyntheticDatasetBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    /// <summary>Sets the dataset description.</summary>
    public SyntheticDatasetBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>Adds tags to categorize the dataset.</summary>
    public SyntheticDatasetBuilder WithTags(params string[] tags)
    {
        _tags.AddRange(tags);
        return this;
    }

    /// <summary>Uses deterministic template-based generation.</summary>
    public SyntheticDatasetBuilder UseDeterministicGenerator(
        Action<TemplateStrategy> configureStrategy)
    {
        var strategy = new TemplateStrategy();
        configureStrategy(strategy);

        if (strategy.Template is null)
            throw new InvalidOperationException("Template must be configured for deterministic generation.");

        _generator = new DeterministicGenerator(strategy.Template, strategy.RandomSeed);
        return this;
    }

    /// <summary>Uses LLM-powered generation via IChatClient.</summary>
    public SyntheticDatasetBuilder UseLlmGenerator(
        IChatClient chatClient,
        Action<LlmStrategy> configureStrategy)
    {
        var strategy = new LlmStrategy();
        configureStrategy(strategy);

        var generator = new LlmGenerator(
            chatClient,
            strategy.SystemPrompt ?? "Generate synthetic Q&A examples as a JSON array.",
            strategy.GenerationTemplate ?? GenerationTemplate.SimpleQA);

        generator.WithTemperature(strategy.Temperature)
                 .WithMaxTokens(strategy.MaxTokens)
                 .WithParallelism(strategy.ParallelismDegree);

        _generator = generator;
        return this;
    }

    /// <summary>Uses composite generation (deterministic + LLM).</summary>
    public SyntheticDatasetBuilder UseCompositeGenerator(
        Action<CompositeGeneratorConfig> configureComposite)
    {
        var config = new CompositeGeneratorConfig();
        configureComposite(config);
        _generator = config.Build();
        return this;
    }

    /// <summary>Generates Q&amp;A pairs from a template.</summary>
    public SyntheticDatasetBuilder GenerateQaPairs(
        int count,
        Action<QaTemplate>? configure = null)
    {
        _count = count;
        return this;
    }

    /// <summary>Generates RAG context+answer examples.</summary>
    public SyntheticDatasetBuilder GenerateRagExamples(
        int count,
        Action<RagTemplate>? configure = null)
    {
        _count = count;
        return this;
    }

    /// <summary>Generates adversarial/edge-case examples.</summary>
    public SyntheticDatasetBuilder GenerateAdversarialExamples(
        int count,
        Action<AdversarialTemplate>? configure = null)
    {
        _count = count;
        return this;
    }

    /// <summary>Generates domain-specific examples.</summary>
    public SyntheticDatasetBuilder GenerateDomainExamples(
        string domain,
        int count,
        Action<DomainTemplate>? configure = null)
    {
        _count = count;
        return this;
    }

    /// <summary>Builds and returns the synthetic GoldenDataset.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no generator is configured.</exception>
    public async Task<GoldenDataset> BuildAsync(CancellationToken ct = default)
    {
        if (_generator is null)
            throw new InvalidOperationException("No generator configured. Call UseDeterministicGenerator, UseLlmGenerator, or UseCompositeGenerator first.");

        if (_count <= 0)
            throw new InvalidOperationException("No examples requested. Call GenerateQaPairs, GenerateRagExamples, GenerateAdversarialExamples, or GenerateDomainExamples first.");

        var examples = await _generator.GenerateAsync(_count, ct).ConfigureAwait(false);

        return new GoldenDataset
        {
            Name = _datasetName,
            Version = _version,
            Description = _description,
            Tags = [.. _tags],
            Examples = [.. examples]
        };
    }
}

/// <summary>
/// Configuration for composite generation (hybrid deterministic + LLM).
/// </summary>
public sealed class CompositeGeneratorConfig
{
    private readonly List<(ISyntheticDataGenerator Generator, double Weight)> _generators = [];

    /// <summary>
    /// Adds a deterministic generator with a weight (proportion of total examples).
    /// </summary>
    public void AddDeterministicGenerator(
        IDataTemplate template,
        double weight,
        int? randomSeed = null)
    {
        _generators.Add((new DeterministicGenerator(template, randomSeed), weight));
    }

    /// <summary>
    /// Adds an LLM generator with a weight.
    /// </summary>
    public void AddLlmGenerator(
        IChatClient chatClient,
        string systemPrompt,
        GenerationTemplate generationTemplate,
        double weight)
    {
        _generators.Add((new LlmGenerator(chatClient, systemPrompt, generationTemplate), weight));
    }

    /// <summary>
    /// Gets the configured composite generator.
    /// </summary>
    public CompositeGenerator Build()
    {
        if (_generators.Count == 0)
            throw new InvalidOperationException("At least one generator must be added.");

        return new CompositeGenerator([.. _generators]);
    }
}
