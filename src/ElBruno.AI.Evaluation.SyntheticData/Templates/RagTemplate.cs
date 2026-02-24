namespace ElBruno.AI.Evaluation.SyntheticData.Templates;

/// <summary>
/// Template for generating RAG examples (question + context → answer).
/// Suitable for evaluating retrieval-augmented generation systems.
/// </summary>
public sealed class RagTemplate : IDataTemplate
{
    private readonly List<string> _tags = [];
    private Dictionary<string, string> _metadata = [];
    private int _documentsPerExample = 1;

    /// <summary>
    /// Creates a new RAG template with document contexts and question-answer pairs.
    /// </summary>
    public RagTemplate(
        IReadOnlyList<string> documents,
        IReadOnlyList<(string Question, string Answer)> qaExamples)
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(qaExamples);
        Documents = documents;
        QaExamples = qaExamples;
    }

    /// <summary>
    /// Sets the number of document chunks per example. Default: 1.
    /// </summary>
    public RagTemplate WithDocumentsPerExample(int count)
    {
        _documentsPerExample = Math.Max(1, count);
        return this;
    }

    /// <summary>
    /// Sets tags for the RAG examples.
    /// </summary>
    public RagTemplate AddTags(params string[] tags)
    {
        _tags.AddRange(tags);
        return this;
    }

    /// <summary>
    /// Adds metadata to all generated examples.
    /// </summary>
    public RagTemplate WithMetadata(Dictionary<string, string> metadata)
    {
        _metadata = metadata;
        return this;
    }

    /// <summary>
    /// Gets the underlying documents (context sources).
    /// </summary>
    public IReadOnlyList<string> Documents { get; }

    /// <summary>
    /// Gets the Q&amp;A pairs that anchor the examples.
    /// </summary>
    public IReadOnlyList<(string Question, string Answer)> QaExamples { get; }

    /// <inheritdoc />
    public string TemplateType => "RAG";

    /// <inheritdoc />
    public IReadOnlyList<string> Tags => _tags;

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Metadata => _metadata;
}
