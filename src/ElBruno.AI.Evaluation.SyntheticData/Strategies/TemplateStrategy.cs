using ElBruno.AI.Evaluation.SyntheticData.Templates;

namespace ElBruno.AI.Evaluation.SyntheticData.Strategies;

/// <summary>
/// Configuration strategy for deterministic template-based generation.
/// </summary>
public sealed class TemplateStrategy
{
    /// <summary>
    /// Gets or sets the data template to use for generation.
    /// </summary>
    public IDataTemplate? Template { get; set; }

    /// <summary>
    /// Gets or sets the optional random seed for reproducible generation.
    /// Null = non-deterministic.
    /// </summary>
    public int? RandomSeed { get; set; }

    /// <summary>
    /// Gets or sets whether to shuffle generated examples. Default: false.
    /// </summary>
    public bool Shuffle { get; set; }

    /// <summary>
    /// Gets or sets how to handle missing/null expected outputs.
    /// Options: "skip", "use_input_as_expected", "empty_string".
    /// Default: "empty_string".
    /// </summary>
    public string? NullHandling { get; set; } = "empty_string";
}
