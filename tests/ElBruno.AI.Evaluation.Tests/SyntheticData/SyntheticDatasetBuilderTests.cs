using Xunit;
using ElBruno.AI.Evaluation.SyntheticData;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using ElBruno.AI.Evaluation.SyntheticData.Strategies;
using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class SyntheticDatasetBuilderTests
{
    [Fact]
    public void Constructor_SetsDatasetName()
    {
        var builder = new SyntheticDatasetBuilder("test-dataset");
        Assert.NotNull(builder);
    }

    [Fact]
    public void WithVersion_ReturnsSameBuilder_ForFluency()
    {
        var builder = new SyntheticDatasetBuilder("ds");
        var result = builder.WithVersion("2.0.0");
        Assert.Same(builder, result);
    }

    [Fact]
    public void WithDescription_ReturnsSameBuilder()
    {
        var builder = new SyntheticDatasetBuilder("ds");
        var result = builder.WithDescription("A test dataset");
        Assert.Same(builder, result);
    }

    [Fact]
    public void WithTags_ReturnsSameBuilder()
    {
        var builder = new SyntheticDatasetBuilder("ds");
        var result = builder.WithTags("qa", "test");
        Assert.Same(builder, result);
    }

    [Fact]
    public async Task BuildAsync_WithoutGenerator_ThrowsInvalidOperation()
    {
        var builder = new SyntheticDatasetBuilder("ds");
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => builder.BuildAsync());
    }

    [Fact]
    public async Task BuildAsync_WithDeterministicGenerator_ReturnsDataset()
    {
        var template = new QaTemplate(
            ["What is {topic}?"],
            ["{topic} is a subject of study."]);

        var dataset = await new SyntheticDatasetBuilder("qa-ds")
            .WithVersion("1.0.0")
            .WithDescription("QA test dataset")
            .WithTags("qa", "synthetic")
            .UseDeterministicGenerator(strategy =>
            {
                strategy.Template = template;
                strategy.RandomSeed = 42;
            })
            .GenerateQaPairs(5)
            .BuildAsync();

        Assert.NotNull(dataset);
        Assert.Equal("qa-ds", dataset.Name);
        Assert.Equal("1.0.0", dataset.Version);
    }

    [Fact]
    public async Task BuildAsync_MetadataPropagated()
    {
        var template = new QaTemplate(
            ["What is AI?"],
            ["AI is artificial intelligence."]);

        var dataset = await new SyntheticDatasetBuilder("meta-ds")
            .WithVersion("2.0.0")
            .WithDescription("Metadata test")
            .WithTags("meta")
            .UseDeterministicGenerator(strategy =>
            {
                strategy.Template = template;
                strategy.RandomSeed = 42;
            })
            .GenerateQaPairs(3)
            .BuildAsync();

        Assert.Equal("meta-ds", dataset.Name);
        Assert.Equal("2.0.0", dataset.Version);
    }

    [Fact]
    public async Task GenerateQaPairs_CountAccuracy()
    {
        var template = new QaTemplate(
            ["Q1?", "Q2?", "Q3?"],
            ["A1", "A2", "A3"]);

        var dataset = await new SyntheticDatasetBuilder("count-ds")
            .UseDeterministicGenerator(strategy =>
            {
                strategy.Template = template;
                strategy.RandomSeed = 42;
            })
            .GenerateQaPairs(5)
            .BuildAsync();

        Assert.Equal(5, dataset.Examples.Count);
    }

    [Fact]
    public async Task GenerateRagExamples_CountAccuracy()
    {
        var template = new RagTemplate(
            ["Document about AI.", "Document about ML."],
            [("What is AI?", "AI is artificial intelligence.")]);

        var dataset = await new SyntheticDatasetBuilder("rag-ds")
            .UseDeterministicGenerator(strategy =>
            {
                strategy.Template = template;
                strategy.RandomSeed = 42;
            })
            .GenerateRagExamples(3)
            .BuildAsync();

        Assert.Equal(3, dataset.Examples.Count);
    }

    [Fact]
    public async Task GenerateAdversarialExamples_CountAccuracy()
    {
        var baseExamples = new List<GoldenExample>
        {
            new() { Input = "What is AI?", ExpectedOutput = "AI is artificial intelligence." }
        };
        var template = new AdversarialTemplate(baseExamples);

        var dataset = await new SyntheticDatasetBuilder("adv-ds")
            .UseDeterministicGenerator(strategy =>
            {
                strategy.Template = template;
                strategy.RandomSeed = 42;
            })
            .GenerateAdversarialExamples(4)
            .BuildAsync();

        Assert.Equal(4, dataset.Examples.Count);
    }

    [Fact]
    public async Task GenerateDomainExamples_CountAccuracy()
    {
        var dataset = await new SyntheticDatasetBuilder("domain-ds")
            .UseDeterministicGenerator(strategy =>
            {
                strategy.Template = new DomainTemplate("healthcare")
                    .WithVocabulary(["diagnosis", "treatment", "patient"]);
                strategy.RandomSeed = 42;
            })
            .GenerateDomainExamples("healthcare", 3)
            .BuildAsync();

        Assert.Equal(3, dataset.Examples.Count);
    }
}
