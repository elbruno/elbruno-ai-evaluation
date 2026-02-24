using Xunit;
using ElBruno.AI.Evaluation.Metrics;

namespace ElBruno.AI.Evaluation.Tests.Metrics;

public class AggregateScorerTests
{
    [Fact]
    public void ComputeWeightedAverage_EqualWeights_ReturnsSimpleAverage()
    {
        var scores = new List<MetricScore>
        {
            new() { Name = "a", Value = 0.8, Weight = 1.0 },
            new() { Name = "b", Value = 0.6, Weight = 1.0 }
        };

        Assert.Equal(0.7, AggregateScorer.ComputeWeightedAverage(scores), 2);
    }

    [Fact]
    public void ComputeWeightedAverage_DifferentWeights_ReturnsWeightedAverage()
    {
        var scores = new List<MetricScore>
        {
            new() { Name = "a", Value = 1.0, Weight = 3.0 },
            new() { Name = "b", Value = 0.0, Weight = 1.0 }
        };

        Assert.Equal(0.75, AggregateScorer.ComputeWeightedAverage(scores), 2);
    }

    [Fact]
    public void ComputeWeightedAverage_Empty_ReturnsZero()
    {
        Assert.Equal(0.0, AggregateScorer.ComputeWeightedAverage([]));
    }

    [Fact]
    public void ComputeMinimum_ReturnsLowestValue()
    {
        var scores = new List<MetricScore>
        {
            new() { Name = "a", Value = 0.9 },
            new() { Name = "b", Value = 0.3 },
            new() { Name = "c", Value = 0.7 }
        };

        Assert.Equal(0.3, AggregateScorer.ComputeMinimum(scores));
    }

    [Fact]
    public void ComputeMinimum_Empty_ReturnsZero()
    {
        Assert.Equal(0.0, AggregateScorer.ComputeMinimum([]));
    }

    [Fact]
    public void ComputePassRate_AllPass_ReturnsOne()
    {
        var scores = new List<MetricScore>
        {
            new() { Name = "a", Value = 0.8, Threshold = 0.7 },
            new() { Name = "b", Value = 0.9, Threshold = 0.7 }
        };

        Assert.Equal(1.0, AggregateScorer.ComputePassRate(scores));
    }

    [Fact]
    public void ComputePassRate_SomeFail_ReturnsCorrectRate()
    {
        var scores = new List<MetricScore>
        {
            new() { Name = "a", Value = 0.8, Threshold = 0.7 },
            new() { Name = "b", Value = 0.5, Threshold = 0.7 }
        };

        Assert.Equal(0.5, AggregateScorer.ComputePassRate(scores));
    }

    [Fact]
    public void ComputePassRate_Empty_ReturnsZero()
    {
        Assert.Equal(0.0, AggregateScorer.ComputePassRate([]));
    }
}
