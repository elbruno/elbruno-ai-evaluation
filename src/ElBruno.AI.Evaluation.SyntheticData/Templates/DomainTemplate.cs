namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for domain-specific synthetic data generation.
/// Handles domain vocabularies, terminology, and realistic constraints.
/// </summary>
public sealed class DomainTemplate : IDataTemplate
{
    private readonly List<string> _tags = [];
    private readonly List<string> _constraints = [];
    private Dictionary<string, string> _metadata = [];
    private List<string> _vocabulary = [];
    private string? _complianceFramework;

    /// <summary>
    /// Creates a template for a specific domain (e.g., "healthcare", "finance", "legal").
    /// </summary>
    public DomainTemplate(string domain)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        Domain = domain;
    }

    /// <summary>
    /// Sets domain-specific vocabulary/terms to use in generation.
    /// </summary>
    public DomainTemplate WithVocabulary(IReadOnlyList<string> terms)
    {
        _vocabulary = [.. terms];
        return this;
    }

    /// <summary>
    /// Sets domain-specific constraints.
    /// </summary>
    public DomainTemplate WithConstraints(params string[] constraints)
    {
        _constraints.AddRange(constraints);
        return this;
    }

    /// <summary>
    /// Sets the regulatory/compliance framework (e.g., "HIPAA" for healthcare).
    /// </summary>
    public DomainTemplate WithComplianceFramework(string framework)
    {
        _complianceFramework = framework;
        _metadata["compliance"] = framework;
        return this;
    }

    /// <summary>
    /// Adds tags reflecting the domain.
    /// </summary>
    public DomainTemplate AddTags(params string[] tags)
    {
        _tags.AddRange(tags);
        return this;
    }

    /// <summary>
    /// Adds domain-specific metadata.
    /// </summary>
    public DomainTemplate WithMetadata(Dictionary<string, string> metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>Gets the domain name.</summary>
    public string Domain { get; }

    /// <summary>Gets the vocabulary terms for this domain.</summary>
    public IReadOnlyList<string> Vocabulary => _vocabulary;

    /// <inheritdoc />
    public string TemplateType => $"Domain:{Domain}";

    /// <inheritdoc />
    public IReadOnlyList<string> Tags => _tags;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Metadata => _metadata;
}
