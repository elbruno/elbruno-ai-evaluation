namespace ElBruno.AI.Evaluation.SyntheticData.Strategies;

/// <summary>
/// Configuration strategy for LLM-powered synthetic data generation.
/// </summary>
public sealed class LlmStrategy
{
    /// <summary>
    /// Gets or sets the system prompt that guides the LLM.
    /// </summary>
    public string? SystemPrompt { get; set; }

    /// <summary>
    /// Gets or sets the generation template specifying output structure.
    /// </summary>
    public GenerationTemplate? GenerationTemplate { get; set; }

    /// <summary>
    /// Gets or sets the temperature for generation (0.0-2.0). Default: 0.7.
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// Gets or sets the maximum tokens per response. Default: 500.
    /// </summary>
    public int MaxTokens { get; set; } = 500;

    /// <summary>
    /// Gets or sets the degree of parallelism. Default: 1.
    /// </summary>
    public int ParallelismDegree { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether to retry failed generations. Default: true.
    /// </summary>
    public bool RetryOnFailure { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum retry count. Default: 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
