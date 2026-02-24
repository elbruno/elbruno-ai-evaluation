// =============================================================================
// Hybrid Evaluation Sample
// Fast deterministic evaluators as first pass, deep LLM review only when needed.
// =============================================================================

using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;

Console.WriteLine("=== Hybrid Evaluation: Fast First Pass ===\n");

// --- Step 1: Golden dataset (5 hardcoded examples) ---
var dataset = new GoldenDataset
{
    Name = "hybrid-golden",
    Description = "Curated examples for hybrid evaluation demo",
    Examples =
    [
        new() { Input = "What is unit testing?",
                 ExpectedOutput = "Unit testing verifies individual components work correctly in isolation." },
        new() { Input = "Explain CI/CD.",
                 ExpectedOutput = "CI/CD automates building, testing, and deploying software changes." },
        new() { Input = "What is code review?",
                 ExpectedOutput = "Code review is the practice of examining code changes before merging." },
        new() { Input = "Define technical debt.",
                 ExpectedOutput = "Technical debt is the cost of shortcuts taken during development." },
        new() { Input = "What is observability?",
                 ExpectedOutput = "Observability lets you understand system behavior from its outputs." }
    ]
};

// Simulated model responses (mix of good and poor quality)
string[] modelOutputs =
[
    "Unit testing checks individual pieces of code to make sure they work properly on their own.",
    "CI/CD is a thing.",  // Too short, low quality
    "Code review involves peers examining source code changes to find bugs and improve quality before merging.",
    "Debt. Technical. Yes.",  // Incoherent
    "Observability provides insights into system health through metrics, logs, and traces."
];

// --- Step 2: Run ElBruno's fast deterministic evaluators ---
IEvaluator[] fastEvaluators =
[
    new RelevanceEvaluator(threshold: 0.5),
    new CoherenceEvaluator(threshold: 0.6),
    new SafetyEvaluator(),
    new ConcisenessEvaluator(minWords: 5, maxWords: 200),
    new CompletenessEvaluator()
];

Console.WriteLine("⚡ Running fast deterministic evaluators...\n");
const double passThreshold = 0.87;
var needsDeepReview = new List<(int Index, string Input, double Score)>();
var passedQuickCheck = 0;

for (int i = 0; i < dataset.Examples.Count; i++)
{
    var example = dataset.Examples[i];
    var output = modelOutputs[i];
    var scores = new List<double>();

    foreach (var eval in fastEvaluators)
    {
        var result = await eval.EvaluateAsync(example.Input, output, example.ExpectedOutput);
        scores.Add(result.Score);
    }

    var avgScore = scores.Average();
    var status = avgScore >= passThreshold ? "✅ PASS" : "❌ FAIL";
    Console.WriteLine($"  [{i + 1}] {status}  score={avgScore:F3}  \"{example.Input}\"");

    // --- Step 3: Filter — below threshold needs deeper review ---
    if (avgScore >= passThreshold)
        passedQuickCheck++;
    else
        needsDeepReview.Add((i + 1, example.Input, avgScore));
}

// --- Step 4: Show how Microsoft evaluators would be plugged in ---
Console.WriteLine($"\n🔬 Deep Review Needed ({needsDeepReview.Count} items):");
foreach (var (idx, input, score) in needsDeepReview)
{
    Console.WriteLine($"  [{idx}] score={score:F3} — \"{input}\"");
}

// -------------------------------------------------------------------------
// To plug in Microsoft's AI Evaluation for deep review, you would:
//
//   // using Microsoft.Extensions.AI.Evaluation;
//   // using Microsoft.Extensions.AI.Evaluation.Quality;
//   //
//   // var msEvaluators = new IEvaluator[]
//   // {
//   //     new RelevanceTruthAndCompletenessEvaluator(),
//   //     new FluencyEvaluator(),
//   //     new CoherenceEvaluator(),
//   //     new GroundednessEvaluator()
//   // };
//   //
//   // foreach (var item in needsDeepReview)
//   // {
//   //     var chatConfig = new ChatConfiguration(chatClient);
//   //     await foreach (var result in msEvaluators)
//   //     {
//   //         var evalResult = await result.EvaluateAsync(messages, chatConfig);
//   //         // Process LLM-judge results for items that failed quick check
//   //     }
//   // }
// -------------------------------------------------------------------------

// --- Step 5: Summary statistics ---
var totalExamples = dataset.Examples.Count;
var deepReviewCount = needsDeepReview.Count;
var llmCallsSaved = passedQuickCheck * fastEvaluators.Length;

Console.WriteLine($"\n📊 Summary:");
Console.WriteLine($"  {passedQuickCheck} of {totalExamples} examples passed quick check, {deepReviewCount} need deep review");
Console.WriteLine($"  Saved {llmCallsSaved} LLM calls by using deterministic first pass");
Console.WriteLine($"  Cost reduction: {(double)passedQuickCheck / totalExamples:P0} of examples need no LLM evaluation");

Console.WriteLine("\n🎉 Hybrid evaluation complete!");
