using Xunit;
using ElBruno.AI.Evaluation.SyntheticData.Generators;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class DeterministicGeneratorTests
{
    private static QaTemplate CreateSimpleQaTemplate() =>
        new(["What is {topic}?", "Explain {topic}."],
            ["{topic} is a concept.", "{topic} can be explained as a subject."]);

    [Fact]
    public async Task GenerateAsync_ReturnsRequestedCount()
    {
        var generator = new DeterministicGenerator(CreateSimpleQaTemplate(), randomSeed: 42);
        var examples = await generator.GenerateAsync(10);
        Assert.Equal(10, examples.Count);
    }

    [Fact]
    public async Task GenerateAsync_WithSameSeed_ProducesSameOutput()
    {
        var template = CreateSimpleQaTemplate();

        var gen1 = new DeterministicGenerator(template, randomSeed: 42);
        var gen2 = new DeterministicGenerator(template, randomSeed: 42);

        var result1 = await gen1.GenerateAsync(5);
        var result2 = await gen2.GenerateAsync(5);

        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(result1[i].Input, result2[i].Input);
            Assert.Equal(result1[i].ExpectedOutput, result2[i].ExpectedOutput);
        }
    }

    [Fact]
    public async Task GenerateAsync_WithDifferentSeeds_ProducesDifferentOutput()
    {
        var template = CreateSimpleQaTemplate();

        var gen1 = new DeterministicGenerator(template, randomSeed: 42);
        var gen2 = new DeterministicGenerator(template, randomSeed: 99);

        var result1 = await gen1.GenerateAsync(5);
        var result2 = await gen2.GenerateAsync(5);

        var anyDifferent = false;
        for (int i = 0; i < 5; i++)
        {
            if (result1[i].Input != result2[i].Input)
            {
                anyDifferent = true;
                break;
            }
        }
        Assert.True(anyDifferent);
    }

    [Fact]
    public async Task GenerateAsync_ZeroCount_ThrowsArgumentOutOfRange()
    {
        var generator = new DeterministicGenerator(CreateSimpleQaTemplate(), randomSeed: 42);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => generator.GenerateAsync(0));
    }

    [Fact]
    public async Task GenerateAsync_ExamplesHaveNonEmptyInputAndOutput()
    {
        var generator = new DeterministicGenerator(CreateSimpleQaTemplate(), randomSeed: 42);
        var examples = await generator.GenerateAsync(3);

        foreach (var ex in examples)
        {
            Assert.False(string.IsNullOrWhiteSpace(ex.Input));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExpectedOutput));
        }
    }

    [Fact]
    public async Task GenerateAsync_WithoutSeed_StillGenerates()
    {
        var generator = new DeterministicGenerator(CreateSimpleQaTemplate());
        var examples = await generator.GenerateAsync(3);
        Assert.Equal(3, examples.Count);
    }

    [Fact]
    public async Task GenerateAsync_TagsPropagatedFromTemplate()
    {
        var template = CreateSimpleQaTemplate().AddTags("qa", "synthetic");
        var generator = new DeterministicGenerator(template, randomSeed: 42);
        var examples = await generator.GenerateAsync(3);

        foreach (var ex in examples)
        {
            Assert.Contains("qa", ex.Tags);
            Assert.Contains("synthetic", ex.Tags);
        }
    }

    [Fact]
    public async Task GenerateAsync_SingleCount_ReturnsSingleExample()
    {
        var generator = new DeterministicGenerator(CreateSimpleQaTemplate(), randomSeed: 42);
        var examples = await generator.GenerateAsync(1);
        Assert.Single(examples);
    }
}
