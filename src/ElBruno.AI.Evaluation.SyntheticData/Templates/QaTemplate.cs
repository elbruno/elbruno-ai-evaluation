namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for generating Q&amp;A (question-answer) pairs.
/// Suitable for FAQ systems, chatbots, and knowledge-based QA.
/// </summary>
public sealed class QaTemplate : IDataTemplate
{
    private readonly IReadOnlyList<string> _questionTemplates;
    private readonly IReadOnlyList<string> _answerTemplates;
    private readonly List<string> _tags = [];
    private Dictionary<string, string> _metadata = [];
    private string? _category;

    /// <summary>
    /// Creates a new Q&amp;A template with question prompts and answer patterns.
    /// </summary>
    public QaTemplate(
        IReadOnlyList<string> questionTemplates,
        IReadOnlyList<string> answerTemplates)
    {
        ArgumentNullException.ThrowIfNull(questionTemplates);
        ArgumentNullException.ThrowIfNull(answerTemplates);
        _questionTemplates = questionTemplates;
        _answerTemplates = answerTemplates;
    }

    /// <summary>
    /// Sets the category/domain for these Q&amp;A pairs (e.g., "technical-support").
    /// </summary>
    public QaTemplate WithCategory(string category)
    {
        _category = category;
        if (!_metadata.ContainsKey("category"))
            _metadata["category"] = category;
        return this;
    }

    /// <summary>
    /// Adds tags to all generated examples.
    /// </summary>
    public QaTemplate AddTags(params string[] tags)
    {
        _tags.AddRange(tags);
        return this;
    }

    /// <summary>
    /// Adds metadata key-value pairs to all generated examples.
    /// </summary>
    public QaTemplate WithMetadata(Dictionary<string, string> metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Gets or generates template pairs (question → answer mappings).
    /// </summary>
    public IReadOnlyList<(string Question, string Answer)> GetPairs()
    {
        var count = Math.Min(_questionTemplates.Count, _answerTemplates.Count);
        var pairs = new List<(string, string)>(count);
        for (int i = 0; i < count; i++)
        {
            pairs.Add((_questionTemplates[i], _answerTemplates[i]));
        }
        return pairs;
    }

    /// <inheritdoc />
    public string TemplateType => "QA";

    /// <inheritdoc />
    public IReadOnlyList<string> Tags => _tags;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Metadata => _metadata;
}
