using Xunit;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using ElBruno.AI.Evaluation.Metrics;
using Microsoft.Extensions.AI;

namespace ElBruno.AI.Evaluation.IntegrationTests;

/// <summary>
/// A mock IChatClient that returns predetermined responses based on input.
/// </summary>
internal sealed class MockChatClient : IChatClient
{
    private readonly Dictionary<string, string> _responses;

    public MockChatClient(Dictionary<string, string> responses) => _responses = responses;

    public MockChatClient(string defaultResponse)
        : this(new Dictionary<string, string>()) => DefaultResponse = defaultResponse;

    public string DefaultResponse { get; set; } = "Default mock response.";

    public async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        var lastMessage = messages.LastOrDefault()?.Text ?? string.Empty;

        var responseText = _responses.TryGetValue(lastMessage, out var r) ? r : DefaultResponse;
        return new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}

public class PipelineIntegrationTests
{
    private static GoldenDataset MakeDataset() => new()
    {
        Name = "integration-test",
        Examples =
        [
            new() { Input = "What is the capital of France?", ExpectedOutput = "The capital of France is Paris." },
            new() { Input = "What is water?", ExpectedOutput = "Water is H2O, a chemical compound." }
        ]
    };

    [Fact]
    public async Task FullPipeline_RunsSuccessfully()
    {
        var client = new MockChatClient(new Dictionary<string, string>
        {
            ["What is the capital of France?"] = "The capital of France is Paris.",
            ["What is water?"] = "Water is H2O, a chemical compound."
        });

        var run = await client.EvaluateAsync(
            MakeDataset(),
            [new RelevanceEvaluator(), new CoherenceEvaluator()]);

        Assert.Equal(2, run.Results.Count);
        Assert.True(run.AggregateScore > 0.0);
        Assert.Equal("integration-test", run.DatasetName);
    }

    [Fact]
    public async Task Pipeline_WithMultipleEvaluators_AggregatesResults()
    {
        var client = new MockChatClient("The capital of France is Paris.");
        var evaluators = new IEvaluator[]
        {
            new RelevanceEvaluator(),
            new CoherenceEvaluator(),
            new SafetyEvaluator()
        };

        var run = await client.EvaluateAsync(MakeDataset(), evaluators);

        Assert.All(run.Results, r =>
        {
            Assert.InRange(r.Score, 0.0, 1.0);
        });
    }

    [Fact]
    public async Task Pipeline_WithBaselineComparison_ProducesReport()
    {
        var client = new MockChatClient("The capital of France is Paris.");
        var evaluators = new IEvaluator[] { new RelevanceEvaluator() };

        var baseline = new BaselineSnapshot
        {
            DatasetName = "integration-test",
            Scores = new() { ["Relevance"] = 0.5 },
            AggregateScore = 0.5
        };

        var report = await client.CompareBaselineAsync(MakeDataset(), evaluators, baseline);

        Assert.NotNull(report);
        Assert.IsType<RegressionReport>(report);
    }

    [Fact]
    public async Task PipelineBuilder_Fluent_BuildsAndRuns()
    {
        var client = new MockChatClient("Paris is the capital of France.");

        var pipeline = new EvaluationPipelineBuilder()
            .WithChatClient(client)
            .WithDataset(MakeDataset())
            .AddEvaluator(new RelevanceEvaluator())
            .Build();

        var run = await pipeline.RunAsync();

        Assert.Equal(2, run.Results.Count);
    }

    [Fact]
    public void PipelineBuilder_MissingClient_Throws()
    {
        var builder = new EvaluationPipelineBuilder()
            .WithDataset(MakeDataset())
            .AddEvaluator(new RelevanceEvaluator());

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }
}
