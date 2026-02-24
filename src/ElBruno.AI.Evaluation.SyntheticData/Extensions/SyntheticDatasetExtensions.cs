using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.SyntheticData.Generators;

namespace ElBruno.AI.Evaluation.SyntheticData.Extensions;

/// <summary>
/// Extension methods for GoldenDataset and synthetic data operations.
/// </summary>
public static class SyntheticDatasetExtensions
{
    /// <summary>
    /// Augments an existing dataset with synthetically generated examples.
    /// </summary>
    public static async Task<GoldenDataset> AugmentWithSyntheticExamplesAsync(
        this GoldenDataset dataset,
        ISyntheticDataGenerator generator,
        int count,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        ArgumentNullException.ThrowIfNull(generator);

        var synthetic = await generator.GenerateAsync(count, ct).ConfigureAwait(false);

        return new GoldenDataset
        {
            Name = dataset.Name,
            Version = dataset.Version,
            Description = dataset.Description,
            CreatedAt = dataset.CreatedAt,
            Tags = [.. dataset.Tags],
            Examples = [.. dataset.Examples, .. synthetic]
        };
    }

    /// <summary>
    /// Merges multiple datasets into a single combined dataset.
    /// </summary>
    public static GoldenDataset Merge(
        this GoldenDataset dataset,
        params GoldenDataset[] otherDatasets)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        var allExamples = new List<GoldenExample>(dataset.Examples);
        var allTags = new List<string>(dataset.Tags);

        foreach (var other in otherDatasets)
        {
            allExamples.AddRange(other.Examples);
            allTags.AddRange(other.Tags);
        }

        return new GoldenDataset
        {
            Name = dataset.Name,
            Version = dataset.Version,
            Description = dataset.Description,
            CreatedAt = dataset.CreatedAt,
            Tags = allTags.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            Examples = allExamples
        };
    }

    /// <summary>
    /// Deduplicates examples by input hash.
    /// </summary>
    public static GoldenDataset Deduplicate(this GoldenDataset dataset)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var unique = new List<GoldenExample>();

        foreach (var example in dataset.Examples)
        {
            if (seen.Add(example.Input))
            {
                unique.Add(example);
            }
        }

        return new GoldenDataset
        {
            Name = dataset.Name,
            Version = dataset.Version,
            Description = dataset.Description,
            CreatedAt = dataset.CreatedAt,
            Tags = [.. dataset.Tags],
            Examples = unique
        };
    }

    /// <summary>
    /// Validates synthetic examples (non-null inputs/outputs, reasonable lengths).
    /// </summary>
    public static IReadOnlyList<ValidationError> ValidateExamples(
        this GoldenDataset dataset,
        ValidationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(dataset);
        var opts = options ?? new ValidationOptions();
        var errors = new List<ValidationError>();
        var seenInputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < dataset.Examples.Count; i++)
        {
            var example = dataset.Examples[i];

            if (opts.FlagNullInputs && string.IsNullOrEmpty(example.Input))
            {
                errors.Add(new ValidationError
                {
                    ExampleIndex = i,
                    ErrorType = "null_input",
                    Message = "Input is null or empty.",
                    Severity = "error"
                });
            }
            else if (example.Input.Length < opts.MinInputLength)
            {
                errors.Add(new ValidationError
                {
                    ExampleIndex = i,
                    ErrorType = "short_input",
                    Message = $"Input length ({example.Input.Length}) is below minimum ({opts.MinInputLength}).",
                    Severity = "warning"
                });
            }
            else if (example.Input.Length > opts.MaxInputLength)
            {
                errors.Add(new ValidationError
                {
                    ExampleIndex = i,
                    ErrorType = "long_input",
                    Message = $"Input length ({example.Input.Length}) exceeds maximum ({opts.MaxInputLength}).",
                    Severity = "warning"
                });
            }

            if (opts.FlagNullOutputs && string.IsNullOrEmpty(example.ExpectedOutput))
            {
                errors.Add(new ValidationError
                {
                    ExampleIndex = i,
                    ErrorType = "null_output",
                    Message = "Expected output is null or empty.",
                    Severity = "error"
                });
            }
            else if (example.ExpectedOutput.Length < opts.MinOutputLength)
            {
                errors.Add(new ValidationError
                {
                    ExampleIndex = i,
                    ErrorType = "short_output",
                    Message = $"Output length ({example.ExpectedOutput.Length}) is below minimum ({opts.MinOutputLength}).",
                    Severity = "warning"
                });
            }
            else if (example.ExpectedOutput.Length > opts.MaxOutputLength)
            {
                errors.Add(new ValidationError
                {
                    ExampleIndex = i,
                    ErrorType = "long_output",
                    Message = $"Output length ({example.ExpectedOutput.Length}) exceeds maximum ({opts.MaxOutputLength}).",
                    Severity = "warning"
                });
            }

            if (opts.FlagDuplicateInputs && !string.IsNullOrEmpty(example.Input) && !seenInputs.Add(example.Input))
            {
                errors.Add(new ValidationError
                {
                    ExampleIndex = i,
                    ErrorType = "duplicate_input",
                    Message = "Duplicate input detected.",
                    Severity = "warning"
                });
            }
        }

        return errors;
    }
}

/// <summary>
/// Represents a validation error found in synthetic examples.
/// </summary>
public sealed class ValidationError
{
    /// <summary>Index of the example in the dataset.</summary>
    public int ExampleIndex { get; init; }

    /// <summary>Type of validation error.</summary>
    public string ErrorType { get; init; } = string.Empty;

    /// <summary>Human-readable error message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Severity level: "error", "warning".</summary>
    public string Severity { get; init; } = "error";
}

/// <summary>
/// Options for validating synthetic datasets.
/// </summary>
public sealed class ValidationOptions
{
    /// <summary>Minimum input length (characters). Default: 1.</summary>
    public int MinInputLength { get; set; } = 1;

    /// <summary>Maximum input length (characters). Default: 5000.</summary>
    public int MaxInputLength { get; set; } = 5000;

    /// <summary>Minimum expected output length (characters). Default: 0.</summary>
    public int MinOutputLength { get; set; }

    /// <summary>Maximum expected output length (characters). Default: 5000.</summary>
    public int MaxOutputLength { get; set; } = 5000;

    /// <summary>Whether to flag examples with null inputs. Default: true.</summary>
    public bool FlagNullInputs { get; set; } = true;

    /// <summary>Whether to flag examples with null expected outputs. Default: false.</summary>
    public bool FlagNullOutputs { get; set; }

    /// <summary>Whether to check for duplicate inputs. Default: true.</summary>
    public bool FlagDuplicateInputs { get; set; } = true;
}
