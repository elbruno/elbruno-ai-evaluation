# Contributing to ElBruno.AI.Evaluation

Thank you for your interest in contributing! This guide will help you get started.

## Building

```bash
dotnet build ElBruno.AI.Evaluation.slnx
```

## Testing

```bash
dotnet test ElBruno.AI.Evaluation.slnx
```

## Pull Request Process

1. **Fork** the repository
2. **Create a branch** from `main`: `git checkout -b feature/my-feature`
3. **Make your changes** following the code style guidelines
4. **Build and test** to ensure nothing is broken
5. **Commit** with clear, descriptive messages
6. **Push** to your fork: `git push origin feature/my-feature`
7. **Open a Pull Request** against the `main` branch

## Adding a New Evaluator

1. Create a new class implementing `IEvaluator` in `src/ElBruno.AI.Evaluation/Evaluators/`
2. Add XML doc comments to the class and `EvaluateAsync` method
3. Return an `EvaluationResult` with:
   - `Score`: 0.0-1.0 (higher is better)
   - `Passed`: Compare score against your threshold
   - `Details`: Human-readable explanation
   - `MetricScores`: Optional breakdown of sub-metrics
4. Keep evaluators deterministic and offline when possible
5. Add tests in `tests/ElBruno.AI.Evaluation.Tests/Evaluators/`

Example:
```csharp
public class MyEvaluator : IEvaluator
{
    private readonly double _threshold;

    public MyEvaluator(double threshold = 0.8)
    {
        _threshold = threshold;
    }

    public async Task<EvaluationResult> EvaluateAsync(
        string input,
        string output,
        string? expectedOutput = null,
        CancellationToken cancellationToken = default)
    {
        var score = ComputeScore(output);
        return new EvaluationResult
        {
            Score = score,
            Passed = score >= _threshold,
            Details = $"Score: {score:F2}, Threshold: {_threshold:F2}"
        };
    }
}
```

## Adding New Dataset Formats

To add support for a new dataset format:

1. Add a new static method to `DatasetLoaderStatic` in `src/ElBruno.AI.Evaluation/Datasets/DatasetLoaderStatic.cs`
2. Follow the pattern of `LoadFromCsvAsync` / `LoadFromJsonAsync`
3. Parse the format into `GoldenExample` objects
4. Return a `GoldenDataset` with appropriate metadata
5. Add XML doc comments

Or implement `IDatasetLoader` for more complex scenarios:

```csharp
public class XmlDatasetLoader : IDatasetLoader
{
    public async Task<GoldenDataset> LoadAsync(string path, CancellationToken ct = default)
    {
        // Parse XML and return GoldenDataset
    }

    public async Task SaveAsync(GoldenDataset dataset, string path, CancellationToken ct = default)
    {
        // Serialize to XML
    }
}
```

## Code Style

Follow the `.editorconfig` conventions:
- **Indentation:** 4 spaces
- **Line endings:** CRLF (Windows)
- **Nullable:** Enabled (`<Nullable>enable</Nullable>` in Directory.Build.props)
- **Naming:**
  - `PascalCase` for public members, types, properties
  - `_camelCase` for private fields
  - `camelCase` for local variables and parameters
- **XML doc comments:** Required on all public APIs

## Questions?

Open an issue or discussion on GitHub — we're happy to help!
