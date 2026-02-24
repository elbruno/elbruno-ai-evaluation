using Xunit;
using Microsoft.Extensions.AI;
using ElBruno.AI.Evaluation.SyntheticData.Generators;
using ElBruno.AI.Evaluation.SyntheticData.Strategies;

namespace ElBruno.AI.Evaluation.Tests.SyntheticData;

public class LlmGeneratorTests
{
    private sealed class MockSyntheticChatClient : IChatClient
    {
        private readonly string _jsonResponse;

        public MockSyntheticChatClient(string jsonResponse)
        {
            _jsonResponse = jsonResponse;
        }

        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, _jsonResponse));
            return Task.FromResult(response);
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose() { }
    }

    private static string CreateValidJsonResponse(int count)
    {
        var items = Enumerable.Range(1, count).Select(i =>
            $$"""{"input": "Question {{i}}?", "expected_output": "Answer {{i}}."}""");
        return $"[{string.Join(",", items)}]";
    }

    [Fact]
    public async Task GenerateAsync_WithValidJsonResponse_ReturnsExamples()
    {
        var client = new MockSyntheticChatClient(CreateValidJsonResponse(3));
        var generator = new LlmGenerator(
            client,
            "Generate Q&A pairs for testing.",
            GenerationTemplate.SimpleQA);

        var examples = await generator.GenerateAsync(3);

        Assert.Equal(3, examples.Count);
        Assert.All(examples, ex =>
        {
            Assert.False(string.IsNullOrWhiteSpace(ex.Input));
            Assert.False(string.IsNullOrWhiteSpace(ex.ExpectedOutput));
        });
    }

    [Fact]
    public async Task GenerateAsync_WithZeroCount_ThrowsArgumentOutOfRange()
    {
        var client = new MockSyntheticChatClient("[]");
        var generator = new LlmGenerator(
            client,
            "Generate examples.",
            GenerationTemplate.SimpleQA);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => generator.GenerateAsync(0));
    }

    [Fact]
    public async Task GenerateAsync_WithMalformedJson_ThrowsOrReturnsEmpty()
    {
        var client = new MockSyntheticChatClient("this is not valid json at all");
        var generator = new LlmGenerator(
            client,
            "Generate examples.",
            GenerationTemplate.SimpleQA);

        try
        {
            var examples = await generator.GenerateAsync(3);
            Assert.True(examples.Count <= 3);
        }
        catch (Exception ex)
        {
            Assert.True(
                ex is System.Text.Json.JsonException ||
                ex is InvalidOperationException ||
                ex is FormatException);
        }
    }

    [Fact]
    public void WithTemperature_ReturnsSameGenerator()
    {
        var client = new MockSyntheticChatClient("[]");
        var generator = new LlmGenerator(client, "prompt", GenerationTemplate.SimpleQA);
        var result = generator.WithTemperature(0.5);
        Assert.Same(generator, result);
    }

    [Fact]
    public void WithMaxTokens_ReturnsSameGenerator()
    {
        var client = new MockSyntheticChatClient("[]");
        var generator = new LlmGenerator(client, "prompt", GenerationTemplate.SimpleQA);
        var result = generator.WithMaxTokens(1000);
        Assert.Same(generator, result);
    }

    [Fact]
    public void WithParallelism_ReturnsSameGenerator()
    {
        var client = new MockSyntheticChatClient("[]");
        var generator = new LlmGenerator(client, "prompt", GenerationTemplate.SimpleQA);
        var result = generator.WithParallelism(4);
        Assert.Same(generator, result);
    }

    [Fact]
    public async Task GenerateAsync_RagContextTemplate_ReturnsExamples()
    {
        var json = """
        [
            {"input": "What is RAG?", "expectedOutput": "RAG is retrieval-augmented generation.", "context": "RAG combines retrieval with generation."}
        ]
        """;
        var client = new MockSyntheticChatClient(json);
        var generator = new LlmGenerator(client, "Generate RAG examples.", GenerationTemplate.RagContext);

        var examples = await generator.GenerateAsync(1);
        Assert.Single(examples);
    }

    [Fact]
    public async Task GenerateAsync_CancellationRespected()
    {
        var client = new MockSyntheticChatClient(CreateValidJsonResponse(5));
        var generator = new LlmGenerator(client, "prompt", GenerationTemplate.SimpleQA);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => generator.GenerateAsync(5, cts.Token));
    }
}
