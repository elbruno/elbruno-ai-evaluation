using Xunit;
using ElBruno.AI.Evaluation.SyntheticData.Generators;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class CompositeGeneratorTests
{
    private static QaTemplate CreateTemplate(string prefix) =>
        new([$"{prefix} Q1?", $"{prefix} Q2?"],
            [$"{prefix} A1", $"{prefix} A2"]);

    [Fact]
    public async Task GenerateAsync_CombinesGenerators_ReturnsCorrectCount()
    {
        var gen1 = new DeterministicGenerator(CreateTemplate("Math"), randomSeed: 42);
        var gen2 = new DeterministicGenerator(CreateTemplate("Science"), randomSeed: 42);

        var composite = new CompositeGenerator(
            (gen1, 0.5),
            (gen2, 0.5));

        var examples = await composite.GenerateAsync(10);
        Assert.Equal(10, examples.Count);
    }

    [Fact]
    public async Task GenerateAsync_WeightedDistribution_RespectsProportion()
    {
        var mathTemplate = CreateTemplate("Math").AddTags("math");
        var sciTemplate = CreateTemplate("Science").AddTags("science");

        var gen1 = new DeterministicGenerator(mathTemplate, randomSeed: 42);
        var gen2 = new DeterministicGenerator(sciTemplate, randomSeed: 42);

        var composite = new CompositeGenerator(
            (gen1, 0.7),
            (gen2, 0.3));

        var examples = await composite.GenerateAsync(10);

        var mathCount = examples.Count(e => e.Tags.Contains("math"));
        var sciCount = examples.Count(e => e.Tags.Contains("science"));

        Assert.Equal(10, mathCount + sciCount);
        Assert.True(mathCount >= 5, $"Expected at least 5 math examples, got {mathCount}");
        Assert.True(sciCount >= 1, $"Expected at least 1 science example, got {sciCount}");
    }

    [Fact]
    public async Task GenerateAsync_SingleGenerator_WorksLikeRegular()
    {
        var gen = new DeterministicGenerator(CreateTemplate("Solo"), randomSeed: 42);
        var composite = new CompositeGenerator((gen, 1.0));

        var compositeResults = await composite.GenerateAsync(5);
        Assert.Equal(5, compositeResults.Count);
    }

    [Fact]
    public async Task GenerateAsync_ZeroCount_ThrowsArgumentOutOfRange()
    {
        var gen = new DeterministicGenerator(CreateTemplate("Test"), randomSeed: 42);
        var composite = new CompositeGenerator((gen, 1.0));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => composite.GenerateAsync(0));
    }

    [Fact]
    public async Task GenerateAsync_ThreeGenerators_AllContribute()
    {
        var gen1 = new DeterministicGenerator(CreateTemplate("A").AddTags("a"), randomSeed: 1);
        var gen2 = new DeterministicGenerator(CreateTemplate("B").AddTags("b"), randomSeed: 2);
        var gen3 = new DeterministicGenerator(CreateTemplate("C").AddTags("c"), randomSeed: 3);

        var composite = new CompositeGenerator(
            (gen1, 0.34),
            (gen2, 0.33),
            (gen3, 0.33));

        var examples = await composite.GenerateAsync(9);

        Assert.Equal(9, examples.Count);
        Assert.True(examples.Any(e => e.Tags.Contains("a")));
        Assert.True(examples.Any(e => e.Tags.Contains("b")));
        Assert.True(examples.Any(e => e.Tags.Contains("c")));
    }
}
