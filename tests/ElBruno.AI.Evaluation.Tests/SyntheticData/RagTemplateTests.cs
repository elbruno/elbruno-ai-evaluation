using Xunit;
using ElBruno.AI.Evaluation.SyntheticData.Templates;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class RagTemplateTests
{
    private static RagTemplate CreateBasicRagTemplate() =>
        new(["AI overview doc.", "ML overview doc."],
            [("What is AI?", "AI is artificial intelligence."),
             ("What is ML?", "ML is machine learning.")]);

    [Fact]
    public void Constructor_SetsTemplateType()
    {
        var template = CreateBasicRagTemplate();
        Assert.Equal("RAG", template.TemplateType);
    }

    [Fact]
    public void Constructor_StoresDocuments()
    {
        var template = CreateBasicRagTemplate();
        Assert.Equal(2, template.Documents.Count);
        Assert.Contains("AI overview doc.", template.Documents);
    }

    [Fact]
    public void Constructor_StoresQaExamples()
    {
        var template = CreateBasicRagTemplate();
        Assert.Equal(2, template.QaExamples.Count);
    }

    [Fact]
    public void AddTags_AddsTags()
    {
        var template = CreateBasicRagTemplate().AddTags("rag", "synthetic");
        Assert.Contains("rag", template.Tags);
        Assert.Contains("synthetic", template.Tags);
    }

    [Fact]
    public void AddTags_ReturnsSameTemplate()
    {
        var template = CreateBasicRagTemplate();
        var result = template.AddTags("tag");
        Assert.Same(template, result);
    }

    [Fact]
    public void WithDocumentsPerExample_ReturnsSameTemplate()
    {
        var template = CreateBasicRagTemplate();
        var result = template.WithDocumentsPerExample(2);
        Assert.Same(template, result);
    }

    [Fact]
    public void WithMetadata_SetsMetadata()
    {
        var template = CreateBasicRagTemplate()
            .WithMetadata(new Dictionary<string, string> { ["retriever"] = "bm25" });

        Assert.Equal("bm25", template.Metadata["retriever"]);
    }

    [Fact]
    public void Tags_DefaultsToEmpty()
    {
        var template = CreateBasicRagTemplate();
        Assert.NotNull(template.Tags);
        Assert.Empty(template.Tags);
    }

    [Fact]
    public void Metadata_DefaultsToEmpty()
    {
        var template = CreateBasicRagTemplate();
        Assert.NotNull(template.Metadata);
        Assert.Empty(template.Metadata);
    }
}
