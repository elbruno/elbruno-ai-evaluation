using Xunit;
using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.Tests.Datasets;

public class GoldenDatasetTests
{
    private static GoldenExample MakeExample(string input = "q", string expected = "a", string? context = null, params string[] tags) =>
        new() { Input = input, ExpectedOutput = expected, Context = context, Tags = [.. tags] };

    [Fact]
    public void Create_WithRequiredProperties_SetsDefaults()
    {
        var ds = new GoldenDataset { Name = "test-ds" };

        Assert.Equal("test-ds", ds.Name);
        Assert.Equal("1.0.0", ds.Version);
        Assert.Empty(ds.Examples);
        Assert.Empty(ds.Tags);
    }

    [Fact]
    public void AddExample_IncreasesCount()
    {
        var ds = new GoldenDataset { Name = "ds" };
        ds.AddExample(MakeExample());
        ds.AddExample(MakeExample("q2", "a2"));

        Assert.Equal(2, ds.Examples.Count);
    }

    [Fact]
    public void AddExample_NullThrows()
    {
        var ds = new GoldenDataset { Name = "ds" };
        Assert.Throws<ArgumentNullException>(() => ds.AddExample(null!));
    }

    [Fact]
    public void GetByTag_ReturnsMatchingExamples()
    {
        var ds = new GoldenDataset { Name = "ds" };
        ds.AddExample(MakeExample("q1", "a1", tags: "math"));
        ds.AddExample(MakeExample("q2", "a2", tags: "science"));
        ds.AddExample(MakeExample("q3", "a3", tags: "math"));

        var mathExamples = ds.GetByTag("math");
        Assert.Equal(2, mathExamples.Count);

        // Case-insensitive
        Assert.Equal(2, ds.GetByTag("MATH").Count);
    }

    [Fact]
    public void GetSubset_ReturnsFilteredDataset()
    {
        var ds = new GoldenDataset { Name = "ds", Version = "2.0.0", Description = "test" };
        ds.AddExample(MakeExample("q1", "a1", context: "ctx"));
        ds.AddExample(MakeExample("q2", "a2"));

        var subset = ds.GetSubset(e => e.Context is not null);

        Assert.Single(subset.Examples);
        Assert.Equal("ds", subset.Name);
        Assert.Equal("2.0.0", subset.Version);
    }

    [Fact]
    public void GetSummary_ReturnsCorrectStatistics()
    {
        var ds = new GoldenDataset { Name = "ds" };
        ds.AddExample(MakeExample("q1", "a1", context: "ctx", tags: "math"));
        ds.AddExample(MakeExample("q2", "a2", tags: ["math", "science"]));
        ds.AddExample(MakeExample("q3", "a3"));

        var summary = ds.GetSummary();

        Assert.Equal(3, summary.TotalExamples);
        Assert.Equal(2, summary.UniqueTags.Count);
        Assert.Equal(1, summary.ExamplesWithContext);
    }

    [Fact]
    public void Version_CanBeCustomized()
    {
        var ds = new GoldenDataset { Name = "ds", Version = "3.1.0" };
        Assert.Equal("3.1.0", ds.Version);
    }
}
