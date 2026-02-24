using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Core interface for synthetic data generation strategies.
/// </summary>
public interface ISyntheticDataGenerator
{
    /// <summary>
    /// Generates synthetic GoldenExample instances.
    /// </summary>
    /// <param name="count">Number of examples to generate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Collection of generated GoldenExample instances.</returns>
    Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default);
}
