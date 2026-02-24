using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for generating adversarial and edge-case examples.
/// Includes nulls, empty strings, contradictions, typos, and malformed inputs.
/// </summary>
public sealed class AdversarialTemplate : IDataTemplate
{
    private readonly List<string> _tags = [];
    private readonly Dictionary<string, string> _metadata = [];
    private bool _nullInjection = true;
    private bool _truncation = true;
    private bool _typoInjection = true;
    private bool _contradictions = true;
    private bool _longInputs;
    private int _maxLength = 2000;

    /// <summary>
    /// Creates a new adversarial template with base examples to perturb.
    /// </summary>
    public AdversarialTemplate(IReadOnlyList<GoldenExample> baseExamples)
    {
        ArgumentNullException.ThrowIfNull(baseExamples);
        BaseExamples = baseExamples;
    }

    /// <summary>Gets the base examples used for perturbation.</summary>
    public IReadOnlyList<GoldenExample> BaseExamples { get; }

    /// <summary>Enables null/empty input injection. Default: true.</summary>
    public AdversarialTemplate WithNullInjection(bool enabled = true) { _nullInjection = enabled; return this; }

    /// <summary>Enables input truncation (partial/incomplete queries). Default: true.</summary>
    public AdversarialTemplate WithTruncation(bool enabled = true) { _truncation = enabled; return this; }

    /// <summary>Enables typo/character-level perturbations. Default: true.</summary>
    public AdversarialTemplate WithTypoInjection(bool enabled = true) { _typoInjection = enabled; return this; }

    /// <summary>Enables contradiction injection. Default: true.</summary>
    public AdversarialTemplate WithContradictions(bool enabled = true) { _contradictions = enabled; return this; }

    /// <summary>Enables extremely long input generation. Default: false.</summary>
    public AdversarialTemplate WithLongInputs(bool enabled = false, int maxLength = 2000)
    {
        _longInputs = enabled;
        _maxLength = maxLength;
        return this;
    }

    /// <summary>Adds tags to all generated adversarial examples.</summary>
    public AdversarialTemplate AddTags(params string[] tags)
    {
        _tags.AddRange(tags);
        return this;
    }

    /// <summary>Gets the list of enabled perturbation types.</summary>
    internal IReadOnlyList<string> GetEnabledPerturbations()
    {
        var perturbations = new List<string>();
        if (_nullInjection) perturbations.Add("null_injection");
        if (_truncation) perturbations.Add("truncation");
        if (_typoInjection) perturbations.Add("typo_injection");
        if (_contradictions) perturbations.Add("contradiction");
        if (_longInputs) perturbations.Add("long_input");
        if (perturbations.Count == 0) perturbations.Add("typo_injection");
        return perturbations;
    }

    /// <inheritdoc />
    public string TemplateType => "Adversarial";

    /// <inheritdoc />
    public IReadOnlyList<string> Tags => _tags;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Metadata => _metadata;
}
