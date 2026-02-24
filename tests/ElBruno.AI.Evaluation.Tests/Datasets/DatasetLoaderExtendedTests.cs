using Xunit;
using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.Tests.Datasets;

/// <summary>
/// Tests for new DatasetLoader features (SaveToCsv, custom columns, empty datasets).
/// Complements existing DatasetLoaderTests.
/// </summary>
public class DatasetLoaderExtendedTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly JsonDatasetLoader _loader = new();

    public DatasetLoaderExtendedTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    private GoldenDataset MakeDataset() => new()
    {
        Name = "test-ds",
        Version = "1.0.0",
        Examples =
        [
            new() { Input = "What is 2+2?", ExpectedOutput = "4", Tags = ["math"] },
            new() { Input = "Capital of France?", ExpectedOutput = "Paris", Context = "Geography" }
        ]
    };

    [Fact]
    public async Task LoadFromJsonAsync_RoundTrips_WithSaveToJsonAsync()
    {
        var path = Path.Combine(_tempDir, "rt.json");
        var ds = MakeDataset();

        await _loader.SaveAsync(ds, path);
        var loaded = await _loader.LoadAsync(path);

        Assert.Equal(ds.Name, loaded.Name);
        Assert.Equal(ds.Examples.Count, loaded.Examples.Count);
        for (int i = 0; i < ds.Examples.Count; i++)
        {
            Assert.Equal(ds.Examples[i].Input, loaded.Examples[i].Input);
            Assert.Equal(ds.Examples[i].ExpectedOutput, loaded.Examples[i].ExpectedOutput);
        }
    }

    [Fact]
    public async Task SaveAsync_CreatesValidJsonFile()
    {
        var path = Path.Combine(_tempDir, "out.json");
        var ds = MakeDataset();

        await _loader.SaveAsync(ds, path);

        Assert.True(File.Exists(path));
        var content = await File.ReadAllTextAsync(path);
        Assert.Contains("test-ds", content);
        // Verify examples are serialized (camelCase property names, values preserved)
        Assert.True(content.Contains("2+2") || content.Contains("2\\u002B2"), "Expected '2+2' in JSON output");
    }

    [Fact]
    public async Task LoadFromCsvAsync_WithStandardColumns_Works()
    {
        var path = Path.Combine(_tempDir, "custom.csv");
        await File.WriteAllTextAsync(path,
            "Input,ExpectedOutput,Context,Tags\nHello,Hi there,,greeting;simple\nBye,Goodbye,,farewell");

        var ds = await _loader.LoadFromCsvAsync(path);

        Assert.Equal(2, ds.Examples.Count);
        Assert.Equal("Hello", ds.Examples[0].Input);
        Assert.Equal("Hi there", ds.Examples[0].ExpectedOutput);
    }

    [Fact]
    public async Task HandlesEmptyDatasets()
    {
        var path = Path.Combine(_tempDir, "empty.json");
        var ds = new GoldenDataset { Name = "empty", Examples = [] };

        await _loader.SaveAsync(ds, path);
        var loaded = await _loader.LoadAsync(path);

        Assert.Equal("empty", loaded.Name);
        Assert.Empty(loaded.Examples);
    }
}
