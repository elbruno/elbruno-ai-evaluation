// =============================================================================
// Synthetic Data Generation Sample
// Demonstrates generating Q&A, RAG, and adversarial datasets using
// ElBruno.AI.Evaluation.SyntheticData with round-trip JSON persistence.
// =============================================================================

using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.SyntheticData.Generators;
using ElBruno.AI.Evaluation.SyntheticData.Templates;

Console.WriteLine("=== Synthetic Data Generation Demo ===\n");

// --- 1. Generate Q&A dataset using QaTemplate ---
var qaTemplate = new QaTemplate(
    questionTemplates: ["What is {0}?", "Explain {0} in simple terms.", "How does {0} work?"],
    answerTemplates: ["{0} is a core concept in AI.", "{0} works by processing data through models.", "{0} can be explained as a pattern recognition system."]
).WithCategory("AI Basics").AddTags("qa", "synthetic");

var qaGenerator = new DeterministicGenerator(qaTemplate, randomSeed: 42);
var qaExamples = await qaGenerator.GenerateAsync(5);

Console.WriteLine("📝 Q&A Dataset (5 examples):");
foreach (var ex in qaExamples)
    Console.WriteLine($"  Q: {ex.Input}\n  A: {ex.ExpectedOutput}\n");

// --- 2. Generate RAG dataset with RagTemplate ---
var ragTemplate = new RagTemplate(
    documents: [
        "Neural networks consist of layers of interconnected nodes.",
        "Transformers use self-attention mechanisms for sequence processing.",
        "Fine-tuning adapts pre-trained models to specific tasks."
    ],
    qaExamples: [
        ("What are neural networks?", "Neural networks consist of layers of interconnected nodes."),
        ("How do transformers work?", "Transformers use self-attention mechanisms.")
    ]
).AddTags("rag", "synthetic");

var ragGenerator = new DeterministicGenerator(ragTemplate, randomSeed: 42);
var ragExamples = await ragGenerator.GenerateAsync(4);

Console.WriteLine("📚 RAG Dataset (4 examples):");
foreach (var ex in ragExamples)
    Console.WriteLine($"  Q: {ex.Input}\n  A: {ex.ExpectedOutput}\n  Context: {ex.Context?[..Math.Min(60, ex.Context.Length)]}...\n");

// --- 3. Generate adversarial dataset with AdversarialTemplate ---
var baseExamples = qaExamples.Take(3).ToList();
var adversarialTemplate = new AdversarialTemplate(baseExamples)
    .WithNullInjection()
    .WithTruncation()
    .WithTypoInjection()
    .AddTags("adversarial", "edge-case");

var adversarialGenerator = new DeterministicGenerator(adversarialTemplate, randomSeed: 42);
var adversarialExamples = await adversarialGenerator.GenerateAsync(4);

Console.WriteLine("⚠️  Adversarial Dataset (4 examples):");
foreach (var ex in adversarialExamples)
    Console.WriteLine($"  Input: {ex.Input}\n  Tags: [{string.Join(", ", ex.Tags)}]\n");

// --- 4. Combine into a single dataset ---
var dataset = new GoldenDataset
{
    Name = "synthetic-combined",
    Description = "Combined synthetic dataset with Q&A, RAG, and adversarial examples",
    Tags = ["synthetic", "demo"]
};
foreach (var ex in qaExamples.Concat(ragExamples).Concat(adversarialExamples))
    dataset.AddExample(ex);

Console.WriteLine($"📦 Combined dataset: {dataset.Examples.Count} total examples\n");

// --- 5. Save to JSON and load back (round-trip) ---
var outputPath = Path.Combine(Directory.GetCurrentDirectory(), "synthetic-dataset.json");
await DatasetLoaderStatic.SaveToJsonAsync(dataset, outputPath);
Console.WriteLine($"💾 Saved to: {outputPath}");

var loaded = await DatasetLoaderStatic.LoadFromJsonAsync(outputPath);
Console.WriteLine($"📂 Loaded back: {loaded.Name} — {loaded.Examples.Count} examples");
Console.WriteLine($"✅ Round-trip verified: {dataset.Examples.Count == loaded.Examples.Count}");

// Clean up
File.Delete(outputPath);
Console.WriteLine("\n🎉 Synthetic data generation complete!");
