namespace ElBruno.AI.Evaluation.Datasets;

/// <summary>
/// Loads golden datasets from various sources.
/// </summary>
public interface IDatasetLoader
{
    Task<GoldenDataset> LoadAsync(string path, CancellationToken ct = default);
}

/// <summary>
/// Loads golden datasets from JSON files.
/// </summary>
public sealed class DatasetLoader : IDatasetLoader
{
    public Task<GoldenDataset> LoadAsync(string path, CancellationToken ct = default)
    {
        // TODO: Implement JSON deserialization
        throw new NotImplementedException();
    }
}
