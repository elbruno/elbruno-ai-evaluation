using Xunit;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using ElBruno.AI.Evaluation.SyntheticData.Generators;
using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class AdversarialTemplateTests
{
    private static List<GoldenExample> CreateBaseExamples() =>
    [
        new() { Input = "What is AI?", ExpectedOutput = "AI is artificial intelligence." },
        new() { Input = "What is ML?", ExpectedOutput = "ML is machine learning." }
    ];

    [Fact]
    public void Constructor_SetsTemplateType()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        Assert.Equal("Adversarial", template.TemplateType);
    }

    [Fact]
    public void WithNullInjection_ReturnsSameTemplate()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        var result = template.WithNullInjection(true);
        Assert.Same(template, result);
    }

    [Fact]
    public void WithTruncation_ReturnsSameTemplate()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        var result = template.WithTruncation(true);
        Assert.Same(template, result);
    }

    [Fact]
    public void WithTypoInjection_ReturnsSameTemplate()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        var result = template.WithTypoInjection(true);
        Assert.Same(template, result);
    }

    [Fact]
    public void WithContradictions_ReturnsSameTemplate()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        var result = template.WithContradictions(true);
        Assert.Same(template, result);
    }

    [Fact]
    public void WithLongInputs_ReturnsSameTemplate()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        var result = template.WithLongInputs(true, maxLength: 5000);
        Assert.Same(template, result);
    }

    [Fact]
    public void AddTags_AddsTags()
    {
        var template = new AdversarialTemplate(CreateBaseExamples())
            .AddTags("adversarial", "edge-case");

        Assert.Contains("adversarial", template.Tags);
        Assert.Contains("edge-case", template.Tags);
    }

    [Fact]
    public async Task GenerateAsync_WithAdversarialTemplate_ProducesExamples()
    {
        var template = new AdversarialTemplate(CreateBaseExamples())
            .WithNullInjection()
            .WithTruncation()
            .WithTypoInjection()
            .AddTags("adversarial");

        var generator = new DeterministicGenerator(template, randomSeed: 42);
        var examples = await generator.GenerateAsync(5);

        Assert.Equal(5, examples.Count);
        Assert.All(examples, ex => Assert.Contains("adversarial", ex.Tags));
    }

    [Fact]
    public void Tags_DefaultsToEmpty()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        Assert.NotNull(template.Tags);
        Assert.Empty(template.Tags);
    }

    [Fact]
    public void Metadata_DefaultsToEmpty()
    {
        var template = new AdversarialTemplate(CreateBaseExamples());
        Assert.NotNull(template.Metadata);
        Assert.Empty(template.Metadata);
    }
}
