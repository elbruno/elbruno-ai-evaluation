using Xunit;
using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.Tests.Datasets;

public class DatasetLoaderTests : IDisposable
{
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private readonly JsonDatasetLoader _loader = new();

    public DatasetLoaderTests() => Directory.CreateDirectory(_tempDir);

    public void Dispose() { if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true); }

    private GoldenDataset MakeDataset() => new()
    {
        Name = "test-ds",
        Version = "1.0.0",
        Examples =
        [
            new() { Input = "What is 2+2?", ExpectedOutput = "4", Tags = ["math"] },
            new() { Input = "Capital of France?", ExpectedOutput = "Paris", Context = "Geography facts" }
        ]
    };

    [Fact]
    public async Task SaveAndLoad_RoundTrip_PreservesData()
    {
        var path = Path.Combine(_tempDir, "dataset.json");
        var ds = MakeDataset();

        await _loader.SaveAsync(ds, path);
        var loaded = await _loader.LoadAsync(path);

        Assert.Equal("test-ds", loaded.Name);
        Assert.Equal(2, loaded.Examples.Count);
        Assert.Equal("4", loaded.Examples[0].ExpectedOutput);
        Assert.Contains("math", loaded.Examples[0].Tags);
    }

    [Fact]
    public async Task LoadAsync_MissingFile_ThrowsFileNotFound()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _loader.LoadAsync(Path.Combine(_tempDir, "nope.json")));
    }

    [Fact]
    public async Task LoadAsync_InvalidJson_Throws()
    {
        var path = Path.Combine(_tempDir, "bad.json");
        await File.WriteAllTextAsync(path, "NOT JSON");

        await Assert.ThrowsAnyAsync<Exception>(() => _loader.LoadAsync(path));
    }

    [Fact]
    public async Task LoadFromCsvAsync_ParsesCorrectly()
    {
        var path = Path.Combine(_tempDir, "data.csv");
        await File.WriteAllTextAsync(path, "Input,ExpectedOutput,Context,Tags\nHello,Hi there,,greeting;simple\nBye,Goodbye,,farewell");

        var ds = await _loader.LoadFromCsvAsync(path);

        Assert.Equal(2, ds.Examples.Count);
        Assert.Equal("Hello", ds.Examples[0].Input);
        Assert.Contains("greeting", ds.Examples[0].Tags);
    }

    [Fact]
    public async Task LoadFromCsvAsync_EmptyFile_Throws()
    {
        var path = Path.Combine(_tempDir, "empty.csv");
        await File.WriteAllTextAsync(path, "");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _loader.LoadFromCsvAsync(path));
    }

    [Fact]
    public async Task LoadFromCsvAsync_MissingRequiredColumns_Throws()
    {
        var path = Path.Combine(_tempDir, "bad.csv");
        await File.WriteAllTextAsync(path, "Foo,Bar\n1,2");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _loader.LoadFromCsvAsync(path));
    }
}
