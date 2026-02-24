using System.Text.Json;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.SyntheticData.Strategies;
using ElBruno.AI.Evaluation.SyntheticData.Utilities;
using Microsoft.Extensions.AI;

namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Generates synthetic examples using an IChatClient.
/// Provides AI-powered, high-variance data suitable for adversarial/edge-case testing.
/// </summary>
public sealed class LlmGenerator : ISyntheticDataGenerator
{
    private readonly IChatClient _chatClient;
    private readonly string _systemPrompt;
    private readonly GenerationTemplate _generationTemplate;
    private double _temperature = 0.7;
    private int _maxTokens = 500;
    private int _parallelism = 1;

    /// <summary>
    /// Creates a new LLM-based generator.
    /// </summary>
    public LlmGenerator(
        IChatClient chatClient,
        string systemPrompt,
        GenerationTemplate generationTemplate)
    {
        ArgumentNullException.ThrowIfNull(chatClient);
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        _chatClient = chatClient;
        _systemPrompt = systemPrompt;
        _generationTemplate = generationTemplate;
    }

    /// <summary>
    /// Sets the temperature for generation (0.0-2.0). Default: 0.7.
    /// </summary>
    public LlmGenerator WithTemperature(double temperature)
    {
        _temperature = temperature;
        return this;
    }

    /// <summary>
    /// Sets the maximum tokens per response. Default: 500.
    /// </summary>
    public LlmGenerator WithMaxTokens(int maxTokens)
    {
        _maxTokens = maxTokens;
        return this;
    }

    /// <summary>
    /// Sets the number of parallel generation requests. Default: 1.
    /// </summary>
    public LlmGenerator WithParallelism(int degree)
    {
        _parallelism = Math.Max(1, degree);
        return this;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var examples = new List<GoldenExample>();
        var batchSize = Math.Max(1, count / _parallelism);
        var tasks = new List<Task<List<GoldenExample>>>();

        for (int i = 0; i < count; i += batchSize)
        {
            var batchCount = Math.Min(batchSize, count - i);
            tasks.Add(GenerateBatchAsync(batchCount, ct));
        }

        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        foreach (var batch in results)
        {
            examples.AddRange(batch);
        }

        return examples.Take(count).ToList();
    }

    private async Task<List<GoldenExample>> GenerateBatchAsync(int count, CancellationToken ct)
    {
        var prompt = PromptGenerator.CreatePrompt(_generationTemplate, count);
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, _systemPrompt),
            new(ChatRole.User, prompt)
        };

        var options = new ChatOptions
        {
            Temperature = (float)_temperature,
            MaxOutputTokens = _maxTokens
        };

        var response = await _chatClient.GetResponseAsync(messages, options, ct).ConfigureAwait(false);
        var content = response.Text ?? string.Empty;

        return ParseExamplesFromResponse(content, count);
    }

    private List<GoldenExample> ParseExamplesFromResponse(string content, int expectedCount)
    {
        var examples = new List<GoldenExample>();

        try
        {
            // Try JSON array parse first
            var jsonStart = content.IndexOf('[');
            var jsonEnd = content.LastIndexOf(']');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = content[jsonStart..(jsonEnd + 1)];
                var items = JsonSerializer.Deserialize<List<JsonElement>>(jsonContent);
                if (items is not null)
                {
                    foreach (var item in items)
                    {
                        var input = item.TryGetProperty("input", out var inp)
                            ? inp.GetString() ?? string.Empty
                            : item.TryGetProperty("question", out var q)
                                ? q.GetString() ?? string.Empty
                                : string.Empty;

                        var output = item.TryGetProperty("expected_output", out var eo)
                            ? eo.GetString() ?? string.Empty
                            : item.TryGetProperty("answer", out var a)
                                ? a.GetString() ?? string.Empty
                                : string.Empty;

                        var context = item.TryGetProperty("context", out var ctx)
                            ? ctx.GetString()
                            : null;

                        if (!string.IsNullOrEmpty(input))
                        {
                            examples.Add(new GoldenExample
                            {
                                Input = input,
                                ExpectedOutput = output,
                                Context = context,
                                Tags = ["synthetic", "llm", _generationTemplate.ToString().ToLowerInvariant()],
                                Metadata = new Dictionary<string, string>
                                {
                                    ["generator"] = "llm",
                                    ["template"] = _generationTemplate.ToString()
                                }
                            });
                        }
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Fallback: treat entire response as a single example
        }

        // If parsing failed or returned nothing, create a single example from the raw response
        if (examples.Count == 0 && !string.IsNullOrWhiteSpace(content))
        {
            examples.Add(new GoldenExample
            {
                Input = $"Generated prompt for {_generationTemplate}",
                ExpectedOutput = content.Trim(),
                Tags = ["synthetic", "llm", "raw"],
                Metadata = new Dictionary<string, string>
                {
                    ["generator"] = "llm",
                    ["template"] = _generationTemplate.ToString(),
                    ["parse_mode"] = "raw"
                }
            });
        }

        return examples;
    }
}
