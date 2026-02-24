namespace ElBruno.AI.Evaluation.SyntheticData.Utilities;

/// <summary>
/// Provides deterministic seed management for reproducible synthetic data generation.
/// </summary>
public static class RandomSeedProvider
{
    /// <summary>
    /// Creates a Random instance from an optional seed. Null = non-deterministic.
    /// </summary>
    public static Random Create(int? seed = null) =>
        seed.HasValue ? new Random(seed.Value) : new Random();

    /// <summary>
    /// Generates a sequence of derived seeds from a master seed for parallel generation.
    /// </summary>
    public static IReadOnlyList<int> DeriveSeeds(int masterSeed, int count)
    {
        var rng = new Random(masterSeed);
        var seeds = new List<int>(count);
        for (int i = 0; i < count; i++)
        {
            seeds.Add(rng.Next());
        }
        return seeds;
    }
}
