using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.SyntheticData.Templates;
using ElBruno.AI.Evaluation.SyntheticData.Utilities;

namespace ElBruno.AI.Evaluation.SyntheticData.Generators;

/// <summary>
/// Generates synthetic examples deterministically using templates.
/// Suitable for reproducible, lightweight test data generation.
/// </summary>
public sealed class DeterministicGenerator : ISyntheticDataGenerator
{
    private readonly IDataTemplate _template;
    private readonly Random _random;

    /// <summary>
    /// Creates a new deterministic generator with the specified template.
    /// </summary>
    public DeterministicGenerator(IDataTemplate template)
        : this(template, null)
    {
    }

    /// <summary>
    /// Creates a new deterministic generator with optional random seed for reproducibility.
    /// </summary>
    public DeterministicGenerator(IDataTemplate template, int? randomSeed)
    {
        ArgumentNullException.ThrowIfNull(template);
        _template = template;
        _random = randomSeed.HasValue ? new Random(randomSeed.Value) : new Random();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<GoldenExample>> GenerateAsync(
        int count,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var examples = new List<GoldenExample>(count);

        switch (_template)
        {
            case QaTemplate qa:
                GenerateFromQa(qa, count, examples);
                break;
            case RagTemplate rag:
                GenerateFromRag(rag, count, examples);
                break;
            case AdversarialTemplate adversarial:
                GenerateFromAdversarial(adversarial, count, examples);
                break;
            case DomainTemplate domain:
                GenerateFromDomain(domain, count, examples);
                break;
            default:
                GenerateGeneric(count, examples);
                break;
        }

        return Task.FromResult<IReadOnlyList<GoldenExample>>(examples);
    }

    private void GenerateFromQa(QaTemplate qa, int count, List<GoldenExample> examples)
    {
        var pairs = qa.GetPairs();
        for (int i = 0; i < count; i++)
        {
            var pair = pairs[_random.Next(pairs.Count)];
            examples.Add(new GoldenExample
            {
                Input = pair.Question,
                ExpectedOutput = pair.Answer,
                Tags = [.. qa.Tags, "synthetic", "deterministic"],
                Metadata = new Dictionary<string, string>(qa.Metadata)
                {
                    ["generator"] = "deterministic",
                    ["template"] = qa.TemplateType
                }
            });
        }
    }

    private void GenerateFromRag(RagTemplate rag, int count, List<GoldenExample> examples)
    {
        var qaExamples = rag.QaExamples;
        var documents = rag.Documents;
        for (int i = 0; i < count; i++)
        {
            var qa = qaExamples[_random.Next(qaExamples.Count)];
            var docIndex = _random.Next(documents.Count);
            examples.Add(new GoldenExample
            {
                Input = qa.Question,
                ExpectedOutput = qa.Answer,
                Context = documents[docIndex],
                Tags = [.. rag.Tags, "synthetic", "deterministic", "rag"],
                Metadata = new Dictionary<string, string>(rag.Metadata)
                {
                    ["generator"] = "deterministic",
                    ["template"] = rag.TemplateType
                }
            });
        }
    }

    private void GenerateFromAdversarial(AdversarialTemplate adversarial, int count, List<GoldenExample> examples)
    {
        var baseExamples = adversarial.BaseExamples;
        var perturbations = adversarial.GetEnabledPerturbations();

        for (int i = 0; i < count; i++)
        {
            var baseExample = baseExamples[_random.Next(baseExamples.Count)];
            var perturbation = perturbations[_random.Next(perturbations.Count)];
            var perturbed = ApplyPerturbation(baseExample, perturbation);
            examples.Add(new GoldenExample
            {
                Input = perturbed,
                ExpectedOutput = baseExample.ExpectedOutput,
                Context = baseExample.Context,
                Tags = [.. adversarial.Tags, "synthetic", "adversarial", perturbation],
                Metadata = new Dictionary<string, string>(adversarial.Metadata)
                {
                    ["generator"] = "deterministic",
                    ["template"] = adversarial.TemplateType,
                    ["perturbation"] = perturbation
                }
            });
        }
    }

    private string ApplyPerturbation(GoldenExample example, string perturbation)
    {
        return perturbation switch
        {
            "null_injection" => string.Empty,
            "truncation" => example.Input.Length > 3
                ? example.Input[..(_random.Next(1, example.Input.Length / 2))]
                : example.Input,
            "typo_injection" => InjectTypo(example.Input),
            "contradiction" => $"Actually the opposite: {example.Input}",
            "long_input" => string.Concat(Enumerable.Repeat(example.Input + " ", 20)),
            _ => example.Input
        };
    }

    private string InjectTypo(string input)
    {
        if (input.Length < 2) return input;
        var chars = input.ToCharArray();
        var pos = _random.Next(0, chars.Length - 1);
        (chars[pos], chars[pos + 1]) = (chars[pos + 1], chars[pos]);
        return new string(chars);
    }

    private void GenerateFromDomain(DomainTemplate domain, int count, List<GoldenExample> examples)
    {
        var vocabulary = domain.Vocabulary;
        for (int i = 0; i < count; i++)
        {
            var term = vocabulary.Count > 0
                ? vocabulary[_random.Next(vocabulary.Count)]
                : domain.Domain;

            examples.Add(new GoldenExample
            {
                Input = $"What is {term} in the context of {domain.Domain}?",
                ExpectedOutput = $"{term} is a key concept in {domain.Domain}.",
                Tags = [.. domain.Tags, "synthetic", "deterministic", "domain"],
                Metadata = new Dictionary<string, string>(domain.Metadata)
                {
                    ["generator"] = "deterministic",
                    ["template"] = domain.TemplateType,
                    ["domain"] = domain.Domain
                }
            });
        }
    }

    private void GenerateGeneric(int count, List<GoldenExample> examples)
    {
        for (int i = 0; i < count; i++)
        {
            examples.Add(new GoldenExample
            {
                Input = $"Sample input {i + 1}",
                ExpectedOutput = $"Sample output {i + 1}",
                Tags = [.. _template.Tags, "synthetic", "deterministic"],
                Metadata = new Dictionary<string, string>(_template.Metadata)
                {
                    ["generator"] = "deterministic",
                    ["template"] = _template.TemplateType
                }
            });
        }
    }
}
