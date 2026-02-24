// ============================================================================
// Foundry Local Evaluation Sample
// Demonstrates real LLM evaluation using Azure AI Foundry Local with
// ElBruno.AI.Evaluation evaluators against actual model output.
// ============================================================================

using Azure.AI.Inference;
using ElBruno.AI.Evaluation.Datasets;
using ElBruno.AI.Evaluation.Evaluators;
using ElBruno.AI.Evaluation.Extensions;
using Microsoft.Extensions.AI;

Console.WriteLine("🏭 Foundry Local Evaluation Sample — Real LLM Evaluation");
Console.WriteLine("=========================================================\n");

// ---------------------------------------------------------------------------
// Step 1: Connect to Foundry Local running phi-4-mini
// ---------------------------------------------------------------------------
const string foundryEndpoint = "http://localhost:5272";
const string modelId = "phi-4-mini";

Console.WriteLine($"📌 Step 1: Connecting to Foundry Local at {foundryEndpoint} (model: {modelId})...");

IChatClient chatClient = new ChatCompletionsClient(
    new Uri(foundryEndpoint),
    new Azure.AzureKeyCredential("unused") // Foundry Local doesn't require a real key
).AsIChatClient(modelId);

Console.WriteLine("   ✅ Chat client created\n");

// ---------------------------------------------------------------------------
// Step 2: Build a golden dataset — technical documentation theme
// ---------------------------------------------------------------------------
Console.WriteLine("📌 Step 2: Building golden dataset (technical documentation)...");

var dataset = new GoldenDataset
{
    Name = "tech-docs-foundry",
    Description = "Technical documentation Q&A for evaluating Foundry Local phi-4-mini responses",
    Examples =
    [
        new()
        {
            Input = "What is dependency injection in .NET?",
            ExpectedOutput = "Dependency injection is a design pattern in .NET where objects receive their dependencies from an external source rather than creating them internally. The built-in DI container in Microsoft.Extensions.DependencyInjection supports constructor injection, service lifetimes (transient, scoped, singleton), and interface-based abstractions.",
            Tags = ["dotnet", "patterns"]
        },
        new()
        {
            Input = "How does async/await work in C#?",
            ExpectedOutput = "Async/await in C# enables asynchronous programming without blocking threads. An async method returns a Task or Task<T>. When the compiler encounters await, it captures the current context and yields control back to the caller. When the awaited task completes, execution resumes after the await point.",
            Tags = ["csharp", "async"]
        },
        new()
        {
            Input = "What is the difference between IEnumerable and IQueryable?",
            ExpectedOutput = "IEnumerable executes queries in memory on the client side using LINQ to Objects. IQueryable translates expressions into the provider's query language (e.g., SQL) and executes on the server side. Use IQueryable for database queries to leverage server-side filtering and avoid loading unnecessary data.",
            Tags = ["csharp", "linq"]
        },
        new()
        {
            Input = "Explain the middleware pipeline in ASP.NET Core.",
            ExpectedOutput = "The ASP.NET Core middleware pipeline processes HTTP requests through a chain of middleware components. Each component can handle the request, modify it, or pass it to the next middleware via next(). Middleware is configured in Program.cs using app.UseXxx() methods and executes in the order it is registered.",
            Tags = ["aspnet", "middleware"]
        },
        new()
        {
            Input = "What is Entity Framework Core and how do migrations work?",
            ExpectedOutput = "Entity Framework Core is an ORM for .NET that maps C# classes to database tables. Migrations track model changes over time. You create a migration with 'dotnet ef migrations add', which generates code to update the schema. Apply it with 'dotnet ef database update' to sync the database.",
            Tags = ["efcore", "database"]
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
Console.WriteLine("📌 Step 4: Running evaluation against live Foundry Local model...");
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
    Console.WriteLine("🎉 Foundry Local evaluation complete!");
    Console.WriteLine("📝 Note: Results vary between runs since this uses a real LLM.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine("❌ Connection Error");
    Console.WriteLine("═══════════════════════════════════════════════════════════════════════");
    Console.WriteLine($"   Could not connect to Foundry Local at {foundryEndpoint}");
    Console.WriteLine($"   Error: {ex.Message}\n");
    Console.WriteLine("   Make sure Foundry Local is running:");
    Console.WriteLine("     1. Install: winget install Microsoft.FoundryLocal");
    Console.WriteLine("     2. Start: foundry model run phi-4-mini");
    Console.WriteLine("     3. Re-run this sample: dotnet run");
    Environment.Exit(1);
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
    Console.WriteLine($"   {ex.GetType().Name}: {ex.Message}");
    Environment.Exit(1);
}
