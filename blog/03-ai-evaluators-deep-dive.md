# Deep Dive: 5 AI Evaluators Every .NET Developer Needs

Evaluators are the engines of AI quality measurement. Each one answers a specific question: *Is this output relevant? Is it truthful? Does it make sense?* 

ElBruno.AI.Evaluation includes five production-ready evaluators, each with a clear purpose and tunable threshold. Let's walk through each one with real code and output examples.

## 1. RelevanceEvaluator

**Question:** Does the output actually answer the input question?

**How it works:** Uses cosine similarity to compare the terms in the input with the terms in the output. High term overlap = high relevance.

**Threshold:** Default 0.6 (60% similarity)

**When to use:** Always. This is your first line of defense against completely off-topic responses.

```csharp
var evaluator = new RelevanceEvaluator(threshold: 0.6);

// Good example
var result1 = await evaluator.EvaluateAsync(
    input: "What is the capital of France?",
    output: "The capital of France is Paris, a city on the Seine River."
);
Console.WriteLine(result1);
// Output: [PASS] Score=0.85 — Cosine similarity between input and output terms: 0.850. Input terms: 5, Output terms: 11.

// Bad example  
var result2 = await evaluator.EvaluateAsync(
    input: "What is the capital of France?",
    output: "The weather today is sunny. I like pizza."
);
Console.WriteLine(result2);
// Output: [FAIL] Score=0.12 — Cosine similarity between input and output terms: 0.120. Input terms: 5, Output terms: 8.
```

**Real-world scenario:** Your support bot receives "How do I upgrade my plan?" It responds with an essay about weather patterns. RelevanceEvaluator catches this immediately.

## 2. FactualityEvaluator

**Question:** Are the claims in the output actually true (based on reference material)?

**How it works:** Extracts claims (sentences) from the output, checks how many tokens overlap with the expected output. Token overlap > 50% = claim is supported.

**Threshold:** Default 0.8 (80% of claims supported)

**When to use:** When you have ground-truth reference material. Essential for RAG systems, documentation bots, news summarization.

```csharp
var evaluator = new FactualityEvaluator(threshold: 0.8);

var result = await evaluator.EvaluateAsync(
    input: "Tell me about Python",
    output: "Python is a programming language created by Guido van Rossum in 1991. It's known for readability.",
    expectedOutput: "Python was created by Guido van Rossum. It became popular for web development and data science."
);
Console.WriteLine(result);
// Output: [PASS] Score=0.87 — 2/3 claims supported (86.67%). 
// The "created by Guido van Rossum" claim is supported. The "1991" and "readability" claims have partial support.
```

**Real-world scenario:** Your RAG chatbot answers questions using company documents. FactualityEvaluator ensures answers stay grounded in the documents, not LLM hallucinations.

## 3. CoherenceEvaluator

**Question:** Does the output make logical sense? Are sentences complete? Any contradictions?

**How it works:** Checks for:
- Incomplete sentences (< 3 words)
- Contradictory phrases ("is/is not" both present)
- Excessive repetition (same sentence appearing multiple times)

**Threshold:** Default 0.7

**When to use:** Catching obviously broken outputs, detecting when the LLM starts generating garbage.

```csharp
var evaluator = new CoherenceEvaluator(threshold: 0.7);

var result1 = await evaluator.EvaluateAsync(
    input: "Explain machine learning",
    output: "Machine learning is. A is is not. The the the algorithm algorithm algorithm learns patterns from data."
);
Console.WriteLine(result1);
// Output: [FAIL] Score=0.52 — Issues: 1 incomplete sentence(s), 1 potential contradiction(s), high repetition (66%). 

var result2 = await evaluator.EvaluateAsync(
    input: "Explain machine learning",
    output: "Machine learning is a subset of artificial intelligence. Algorithms learn from data without being explicitly programmed. This enables systems to improve with experience."
);
Console.WriteLine(result2);
// Output: [PASS] Score=1.00 — Output is coherent (3 sentence(s), no issues detected).
```

**Real-world scenario:** Your LLM starts hallucinating due to a prompt injection attack. CoherenceEvaluator detects the incoherent garbage output.

## 4. HallucinationEvaluator

**Question:** Is the output making up facts not present in the source material?

**How it works:** Builds a grounding corpus from the input + expected output. Counts how many tokens in the output appear in the grounding corpus. High overlap = low hallucination.

**Threshold:** Default 0.7 (70% of tokens grounded)

**When to use:** RAG systems, question-answering over documents. Essential for reducing AI-generated falsehoods.

```csharp
var evaluator = new HallucinationEvaluator(threshold: 0.7);

var result = await evaluator.EvaluateAsync(
    input: "What does the customer say about pricing?",
    output: "The customer says pricing is reasonable and competitive. They also mentioned liking the UI.",
    expectedOutput: "The customer says pricing is reasonable and offers good value for the features provided."
);
Console.WriteLine(result);
// Output: [PASS] Score=0.74 — Keyword overlap: 11/15 tokens grounded (73%). 4 potentially hallucinated token(s).
// "competitive" and "UI" are not in the source material but the core message ("pricing is reasonable") is grounded.
```

**Real-world scenario:** Your RAG chatbot adds details that sound plausible but aren't in the documents. HallucinationEvaluator catches this.

## 5. SafetyEvaluator

**Question:** Does the output contain unsafe content?

**How it works:** Checks for:
- Profanity and blocked terms (configurable)
- PII patterns: emails, phone numbers, SSNs
- Harmful instructions

**Threshold:** Default 0.9 (allows minimal violations)

**When to use:** Always, especially for customer-facing systems. Non-negotiable for compliance.

```csharp
var evaluator = new SafetyEvaluator(threshold: 0.9);

var result1 = await evaluator.EvaluateAsync(
    input: "Hello",
    output: "This is a damn problem. Contact me at john.doe@example.com"
);
Console.WriteLine(result1);
// Output: [FAIL] Score=0.70 — 2 violation(s): Blocked terms: damn; Email address detected.

var result2 = await evaluator.EvaluateAsync(
    input: "Help",
    output: "I can help you with that. Please be patient."
);
Console.WriteLine(result2);
// Output: [PASS] Score=1.00 — No safety violations detected.

// Custom blocklist
var customEvaluator = new SafetyEvaluator(
    threshold: 0.9,
    additionalBlocklist: new[] { "proprietary", "confidential" }
);
```

**Real-world scenario:** Your bot accidental exposes customer email addresses or SSNs from training data. SafetyEvaluator catches this before it reaches production.

## Combining Evaluators

Real-world evaluation uses multiple evaluators:

```csharp
var dataset = new JsonDatasetLoader().LoadAsync("support-bot.json");

var evaluators = new List<IEvaluator>
{
    new RelevanceEvaluator(0.6),      // Must answer the question
    new FactualityEvaluator(0.8),      // Must be accurate
    new CoherenceEvaluator(0.7),       // Must make sense
    new HallucinationEvaluator(0.7),   // Must not make things up
    new SafetyEvaluator(0.95)          // Must be safe
};

var pipeline = new EvaluationPipelineBuilder()
    .WithChatClient(chatClient)
    .WithDataset(dataset)
    .ForEach(evaluators, e => _pipeline.AddEvaluator(e))
    .Build();

var run = await pipeline.RunAsync();

foreach (var result in run.Results)
{
    // Only pass if ALL evaluators agree
    bool fullyPass = result.MetricScores.Values.All(m => m.Passed);
    Console.WriteLine($"Example: {result.Passed ? "PASS" : "FAIL"}");
    
    foreach (var (metric, score) in result.MetricScores)
    {
        Console.WriteLine($"  {metric}: {score.Value:F2} [{(score.Passed ? "✓" : "✗")}]");
    }
}
```

## Creating Custom Evaluators

The `IEvaluator` interface is simple. Build your own:

```csharp
public class LengthEvaluator : IEvaluator
{
    private readonly int _minWords;
    
    public LengthEvaluator(int minWords = 10)
    {
        _minWords = minWords;
    }
    
    public Task<EvaluationResult> EvaluateAsync(
        string input, 
        string output, 
        string? expectedOutput = null, 
        CancellationToken ct = default)
    {
        var wordCount = output.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        var score = Math.Min(1.0, (double)wordCount / _minWords);
        
        return Task.FromResult(new EvaluationResult
        {
            Score = score,
            Passed = wordCount >= _minWords,
            Details = $"Output has {wordCount} words (threshold: {_minWords})",
            MetricScores = new()
            {
                ["length"] = new MetricScore 
                { 
                    Name = "Length", 
                    Value = score, 
                    Threshold = 1.0 
                }
            }
        });
    }
}

// Use it
var customEval = new LengthEvaluator(minWords: 50);
var result = await customEval.EvaluateAsync("Explain quantum computing", "...");
```

## Choosing Thresholds

Different use cases need different thresholds:

| Scenario | Relevance | Factuality | Coherence | Hallucination | Safety |
|----------|-----------|------------|-----------|---------------|--------|
| Support Bot | 0.7 | 0.8 | 0.8 | 0.7 | 0.95 |
| Content Generation | 0.6 | 0.5 | 0.8 | 0.6 | 0.9 |
| RAG Q&A | 0.7 | 0.9 | 0.7 | 0.85 | 0.95 |
| Brainstorming | 0.5 | 0.3 | 0.6 | 0.4 | 0.85 |

Start conservative (high thresholds), then relax as you understand what's normal for your LLM.

## Try It Yourself

Evaluate your first LLM output:

```csharp
var evaluators = new List<IEvaluator>
{
    new RelevanceEvaluator(),
    new FactualityEvaluator(),
    new CoherenceEvaluator(),
    new HallucinationEvaluator(),
    new SafetyEvaluator()
};

string input = "Your test question";
string output = "Your LLM's response";
string expected = "Reference material";

foreach (var evaluator in evaluators)
{
    var result = await evaluator.EvaluateAsync(input, output, expected);
    Console.WriteLine($"{evaluator.GetType().Name}: {result}");
}
```

---

*With these five evaluators in your toolkit, you can measure virtually any dimension of AI quality. In the next post, we'll see how to integrate them into your xUnit test suite.*
