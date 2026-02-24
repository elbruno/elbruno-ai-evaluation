// ============================================================================
// Ollama Evaluation Sample
// Demonstrates real LLM evaluation using a local Ollama instance with
// ElBruno.AI.Evaluation evaluators against actual model output.
// ============================================================================

using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using Microsoft.Extensions.AI;

Console.WriteLine("🦙 Ollama Evaluation Sample — Real LLM Evaluation");
Console.WriteLine("==================================================\n");

// ---------------------------------------------------------------------------
// Step 1: Connect to a local Ollama instance running phi3:mini
// ---------------------------------------------------------------------------
const string ollamaEndpoint = "http://localhost:11434";
const string modelId = "phi3:mini";

Console.WriteLine($"📌 Step 1: Connecting to Ollama at {ollamaEndpoint} (model: {modelId})...");

IChatClient chatClient = new OllamaChatClient(ollamaEndpoint, modelId);
Console.WriteLine("   ✅ Chat client created\n");

// ---------------------------------------------------------------------------
// Step 2: Build a golden dataset — customer support theme
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 2: Building golden dataset (customer support)...");

var dataset = new GoldenDataset
{
    Name = "customer-support-ollama",
    Description = "Customer support Q&A for evaluating Ollama phi3:mini responses",
    Examples =
    [
        new()
        {
            Input = "How do I reset my password?",
            ExpectedOutput = "To reset your password, go to the login page and click 'Forgot Password'. Enter your email address and follow the instructions in the reset email to create a new password.",
            Tags = ["password", "account"]
        },
        new()
        {
            Input = "What is your refund policy?",
            ExpectedOutput = "We offer a full refund within 30 days of purchase if the product is unused and in its original packaging. After 30 days, we can offer store credit or an exchange.",
            Tags = ["refund", "policy"]
        },
        new()
        {
            Input = "How can I track my order?",
            ExpectedOutput = "You can track your order by logging into your account and visiting the 'My Orders' section. You will find a tracking number and a link to the carrier's tracking page.",
            Tags = ["order", "shipping"]
        },
        new()
        {
            Input = "Do you offer international shipping?",
            ExpectedOutput = "Yes, we ship to over 50 countries worldwide. International shipping typically takes 7 to 14 business days. Shipping costs vary by destination and are calculated at checkout.",
            Tags = ["shipping", "international"]
        },
        new()
        {
            Input = "How do I contact customer support?",
            ExpectedOutput = "You can contact our customer support team via email at support@example.com, by phone at 1-800-555-0100 during business hours, or through the live chat on our website.",
            Tags = ["contact", "support"]
        }
    ]
};

Console.WriteLine($"   ✅ Dataset '{dataset.Name}' with {dataset.Examples.Count} examples\n");

// ---------------------------------------------------------------------------
// Step 3: Configure all 5 evaluators
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 3: Configuring evaluators...");

IEvaluator[] evaluators =
[
    new RelevanceEvaluator(threshold: 0.3),
    new HallucinationEvaluator(threshold: 0.3),
    new SafetyEvaluator(),
    new CoherenceEvaluator(),
    new FactualityEvaluator(threshold: 0.3)
];

Console.WriteLine("   ✅ RelevanceEvaluator   (threshold=0.3)");
Console.WriteLine("   ✅ HallucinationEvaluator (threshold=0.3)");
Console.WriteLine("   ✅ SafetyEvaluator      (threshold=0.9)");
Console.WriteLine("   ✅ CoherenceEvaluator   (threshold=0.7)");
Console.WriteLine("   ✅ FactualityEvaluator  (threshold=0.3)\n");

// ---------------------------------------------------------------------------
// Step 4: Send each question to the real model and evaluate responses
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 4: Running evaluation against live Ollama model...");
Console.WriteLine("   ⏳ This may take a minute depending on your hardware...\n");

try
{
    var run = await chatClient.EvaluateAsync(dataset, evaluators);

    // ---------------------------------------------------------------------------
    // Step 5: Print results in a formatted console table
    // ---------------------------------------------------------------------------
    Console.WriteLine("📌 Step 5: Evaluation Results");
    Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
    Console.WriteLine($"  Run ID:          {run.RunId}");
    Console.WriteLine($"  Dataset:         {run.DatasetName}");
    Console.WriteLine($"  Aggregate Score: {run.AggregateScore:F4}");
    Console.WriteLine($"  Pass Rate:       {run.PassRate:P1}");
    Console.WriteLine($"  All Passed:      {run.AllPassed}");
    Console.WriteLine("═══════════════════════════════════════════════════════════════════════\n");

    // Per-example detail
    Console.WriteLine("  ┌─────┬────────┬────────┬──────────────────────────────────────────────────┐");
    Console.WriteLine("  │  #  │ Status │ Score  │ Question                                         │");
    Console.WriteLine("  ├─────┼────────┼────────┼──────────────────────────────────────────────────┤");

    for (int i = 0; i < run.Results.Count; i++)
    {
        var r = run.Results[i];
        var q = dataset.Examples[i].Input;
        var truncQ = q.Length > 48 ? q[..45] + "..." : q;
        var status = r.Passed ? "✅ PASS" : "❌ FAIL";
        Console.WriteLine($"  │ {i + 1,3} │ {status} │ {r.Score:F4} │ {truncQ,-48} │");
    }

    Console.WriteLine("  └─────┴────────┴────────┴──────────────────────────────────────────────────┘\n");

    // Per-example metric breakdown
    Console.WriteLine("  Metric Breakdown:");
    Console.WriteLine("  ─────────────────────────────────────────────────────────────────────");

    for (int i = 0; i < run.Results.Count; i++)
    {
        var r = run.Results[i];
        var q = dataset.Examples[i].Input;
        Console.WriteLine($"\n  [{i + 1}] {q}");
        foreach (var (name, metric) in r.MetricScores)
            Console.WriteLine($"       └─ {metric}");
    }

    Console.WriteLine();
    Console.WriteLine("🎉 Ollama evaluation complete!");
    Console.WriteLine("📝 Note: Results vary between runs since this uses a real LLM.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine("❌ Connection Error");
    Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
    Console.WriteLine($"   Could not connect to Ollama at {ollamaEndpoint}");
    Console.WriteLine($"   Error: {ex.Message}\n");
    Console.WriteLine("   Make sure Ollama is running:");
    Console.WriteLine("     1. Install Ollama: https://ollama.com");
    Console.WriteLine("     2. Pull the model: ollama pull phi3:mini");
    Console.WriteLine("     3. Start the server: ollama serve");
    Console.WriteLine("     4. Re-run this sample: dotnet run");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
    Console.WriteLine($"   {ex.GetType().Name}: {ex.Message}");
    Environment.Exit(1);
}
