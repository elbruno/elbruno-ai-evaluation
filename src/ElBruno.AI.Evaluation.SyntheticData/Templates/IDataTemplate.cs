namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Base interface for data templates.
/// Templates define the structure and content patterns for synthetic data generation.
/// </summary>
public interface IDataTemplate
{
    /// <summary>
    /// Gets the template type/category.
    /// </summary>
    string TemplateType { get; }

    /// <summary>
    /// Gets the example tags to apply to generated examples.
    /// </summary>
    IReadOnlyList<string> Tags { get; }

    /// <summary>
    /// Gets optional metadata to include in generated examples.
    /// </summary>
    IReadOnlyDictionary<string, string> Metadata { get; }
}
