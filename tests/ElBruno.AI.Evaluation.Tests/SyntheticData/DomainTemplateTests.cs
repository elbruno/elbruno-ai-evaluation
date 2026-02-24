using Xunit;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using ElBruno.AI.Evaluation.SyntheticData.Generators;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class DomainTemplateTests
{
    [Fact]
    public void Constructor_SetsDomain()
    {
        var template = new DomainTemplate("healthcare");
        Assert.Equal("healthcare", template.Domain);
    }

    [Fact]
    public void TemplateType_IncludesDomain()
    {
        var template = new DomainTemplate("finance");
        Assert.Equal("Domain:finance", template.TemplateType);
    }

    [Fact]
    public void WithVocabulary_SetsVocabulary()
    {
        var terms = new List<string> { "diagnosis", "treatment", "patient" };
        var template = new DomainTemplate("healthcare").WithVocabulary(terms);

        Assert.Equal(3, template.Vocabulary.Count);
        Assert.Contains("diagnosis", template.Vocabulary);
    }

    [Fact]
    public void WithVocabulary_ReturnsSameTemplate()
    {
        var template = new DomainTemplate("healthcare");
        var result = template.WithVocabulary(["term1"]);
        Assert.Same(template, result);
    }

    [Fact]
    public void WithConstraints_ReturnsSameTemplate()
    {
        var template = new DomainTemplate("finance");
        var result = template.WithConstraints("only public companies", "no insider info");
        Assert.Same(template, result);
    }

    [Fact]
    public void WithComplianceFramework_ReturnsSameTemplate()
    {
        var template = new DomainTemplate("healthcare");
        var result = template.WithComplianceFramework("HIPAA");
        Assert.Same(template, result);
    }

    [Fact]
    public void AddTags_AddsTags()
    {
        var template = new DomainTemplate("legal").AddTags("legal", "compliance");
        Assert.Contains("legal", template.Tags);
        Assert.Contains("compliance", template.Tags);
    }

    [Fact]
    public void WithMetadata_SetsMetadata()
    {
        var template = new DomainTemplate("healthcare")
            .WithMetadata(new Dictionary<string, string> { ["framework"] = "HIPAA" });

        Assert.Equal("HIPAA", template.Metadata["framework"]);
    }

    [Fact]
    public async Task GenerateAsync_WithDomainTemplate_ProducesExamples()
    {
        var template = new DomainTemplate("healthcare")
            .WithVocabulary(["diagnosis", "treatment", "patient", "symptom"])
            .WithComplianceFramework("HIPAA")
            .AddTags("healthcare");

        var generator = new DeterministicGenerator(template, randomSeed: 42);
        var examples = await generator.GenerateAsync(5);

        Assert.Equal(5, examples.Count);
        Assert.All(examples, ex => Assert.Contains("healthcare", ex.Tags));
    }

    [Fact]
    public void Tags_DefaultsToEmpty()
    {
        var template = new DomainTemplate("test");
        Assert.NotNull(template.Tags);
        Assert.Empty(template.Tags);
    }

    [Fact]
    public void Metadata_DefaultsToEmpty()
    {
        var template = new DomainTemplate("test");
        Assert.NotNull(template.Metadata);
        Assert.Empty(template.Metadata);
    }
}
