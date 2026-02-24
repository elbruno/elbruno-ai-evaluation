using ElBruno.AI.Evaluation.Datasets;

namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Combines multiple ISyntheticDataGenerator instances for hybrid generation.
/// Example: 70% deterministic examples + 30% LLM-powered examples.
/// </summary>
public sealed class CompositeGenerator : ISyntheticDataGenerator
{
    private readonly (ISyntheticDataGenerator Generator, double Weight)[] _generators;

    /// <summary>
    /// Creates a new composite generator with weighted sub-generators.
    /// </summary>
    public CompositeGenerator(
        params (ISyntheticDataGenerator generator, double weight)[] generators)
    {
        ArgumentNullException.ThrowIfNull(generators);
        if (generators.Length == 0)
            throw new ArgumentException("At least one generator is required.", nameof(generators));

        _generators = generators;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var totalWeight = _generators.Sum(g => g.Weight);
        var examples = new List<GoldenExample>(count);

        int generated = 0;
        for (int i = 0; i < _generators.Length; i++)
        {
            var (generator, weight) = _generators[i];
            var share = i == _generators.Length - 1
                ? count - generated
                : (int)Math.Round(count * (weight / totalWeight));

            if (share <= 0) continue;

            var batch = await generator.GenerateAsync(share, ct).ConfigureAwait(false);
            examples.AddRange(batch);
            generated += batch.Count;
        }

        return examples;
    }
}
