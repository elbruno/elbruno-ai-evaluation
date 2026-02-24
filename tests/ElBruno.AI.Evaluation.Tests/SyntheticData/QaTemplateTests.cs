using Xunit;
using ElBruno.AI.Evaluation.SyntheticData.Templates;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class QaTemplateTests
{
    [Fact]
    public void Constructor_SetsTemplateType()
    {
        var template = new QaTemplate(["Q?"], ["A."]);
        Assert.Equal("QA", template.TemplateType);
    }

    [Fact]
    public void Constructor_InitializesEmptyTags()
    {
        var template = new QaTemplate(["Q?"], ["A."]);
        Assert.NotNull(template.Tags);
        Assert.Empty(template.Tags);
    }

    [Fact]
    public void AddTags_AddsTagsToTemplate()
    {
        var template = new QaTemplate(["Q?"], ["A."])
            .AddTags("qa", "test");

        Assert.Contains("qa", template.Tags);
        Assert.Contains("test", template.Tags);
    }

    [Fact]
    public void AddTags_ReturnsSameTemplate()
    {
        var template = new QaTemplate(["Q?"], ["A."]);
        var result = template.AddTags("tag1");
        Assert.Same(template, result);
    }

    [Fact]
    public void WithCategory_ReturnsSameTemplate()
    {
        var template = new QaTemplate(["Q?"], ["A."]);
        var result = template.WithCategory("technical-support");
        Assert.Same(template, result);
    }

    [Fact]
    public void WithMetadata_SetsMetadata()
    {
        var template = new QaTemplate(["Q?"], ["A."])
            .WithMetadata(new Dictionary<string, string> { ["source"] = "manual" });

        Assert.NotNull(template.Metadata);
        Assert.Equal("manual", template.Metadata["source"]);
    }

    [Fact]
    public void GetPairs_ReturnsQuestionAnswerPairs()
    {
        var template = new QaTemplate(
            ["What is AI?", "What is ML?"],
            ["AI is artificial intelligence.", "ML is machine learning."]);

        var pairs = template.GetPairs();

        Assert.NotEmpty(pairs);
        Assert.All(pairs, pair =>
        {
            Assert.False(string.IsNullOrWhiteSpace(pair.Question));
            Assert.False(string.IsNullOrWhiteSpace(pair.Answer));
        });
    }

    [Fact]
    public void Constructor_WithMultipleTemplates_StoresBoth()
    {
        var questions = new List<string> { "Q1?", "Q2?", "Q3?" };
        var answers = new List<string> { "A1.", "A2.", "A3." };
        var template = new QaTemplate(questions, answers);

        var pairs = template.GetPairs();
        Assert.True(pairs.Count >= 1);
    }

    [Fact]
    public void Metadata_DefaultsToEmpty()
    {
        var template = new QaTemplate(["Q?"], ["A."]);
        Assert.NotNull(template.Metadata);
        Assert.Empty(template.Metadata);
    }
}
