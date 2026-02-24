# A Guide to Choosing the Right Evaluators for Your AI App

You've seen the landscape: ElBruno's five deterministic evaluators, Microsoft's LLM-powered suite, agent-focused metrics, safety analysis. The question now: **Which evaluators should I actually use?**

This post organizes evaluators by **scenario, not toolkit**. Answer the question "What am I building?" and get a recommended evaluator set.

## Scenario 1: Building a Chatbot

**Your Goal:** Conversational AI that answers user questions, stays on-topic, doesn't hallucinate or expose PII.

**Recommended Evaluators:**

```csharp
var evaluators = new List<IEvaluator>
{
    new RelevanceEvaluator(0.7),        // Answers the question (ElBruno)
    new CoherenceEvaluator(0.7),        // Output makes sense (ElBruno)
    new SafetyEvaluator(0.95),          // No PII or harmful content (ElBruno)
    new HallucinationEvaluator(0.75),   // No made-up facts (ElBruno)
};

// If you need nuanced judgment (optional):
var deepEval = new MicrosoftRelevanceEvaluator();  // Does it really address the question?
```

**Thresholds:**
- Relevance: 0.7+ (must address the question)
- Coherence: 0.7+ (must make sense)
- Safety: 0.95+ (safety is non-negotiable)
- Hallucination: 0.75+ (don't make up facts)

**CI/CD:** Use ElBruno evaluators for fast gates. Optionally use Microsoft for periodic deep analysis.

---

## Scenario 2: Building a RAG (Retrieval-Augmented Generation) System

**Your Goal:** Answer questions using retrieved documents. Responses must be grounded in source material, factually accurate, and cite sources correctly.

**Recommended Evaluators:**

```csharp
var evaluators = new List<IEvaluator>
{
    new HallucinationEvaluator(0.8),    // Must be grounded in retrieved context (ElBruno)
    new FactualityEvaluator(0.9),       // Claims must match source documents (ElBruno)
    new RelevanceEvaluator(0.8),        // Answer must address the query (ElBruno)
    new SafetyEvaluator(0.95),          // No PII from documents (ElBruno)
    
    // Optional: Deep analysis
    new MicrosoftGroundednessEvaluator(), // LLM judges if response is grounded
};

// Golden dataset must include context
var dataset = new GoldenDataset
{
    Examples = new()
    {
        new GoldenExample
        {
            Input = "What is the company's return policy?",
            ExpectedOutput = "30-day returns, no questions asked.",
            Context = "[From company docs] Our return policy: 30 days from purchase, full refund, no questions asked.",
            Tags = new() { "rag", "policy" }
        }
    }
};
```

**Thresholds:**
- Hallucination: 0.80+ (very strict—can't make things up)
- Factuality: 0.90+ (claims must match documents)
- Relevance: 0.80+ (must answer the question)
- Safety: 0.95+ (don't expose sensitive info)

**CI/CD:** All ElBruno evaluators are offline—perfect for regression gates. Use Microsoft's groundedness evaluator for release reviews.

---

## Scenario 3: Building an Agent (Tool-Calling System)

**Your Goal:** An agent that makes decisions, calls tools, orchestrates workflows. Must interpret requests correctly, use the right tools, and report accurate results.

**Recommended Evaluators:**

```csharp
// ElBruno: Quick offline checks
var quickChecks = new List<IEvaluator>
{
    new CoherenceEvaluator(0.7),        // Plan makes sense
    new SafetyEvaluator(0.95),          // No malicious tool calls
};

// Microsoft: Agent-specific evaluation (REQUIRED for agents)
var agentEvals = new[]
{
    new IntentResolutionEvaluator(),    // Did agent understand the user's intent?
    new TaskAdherenceEvaluator(),       // Did agent complete the requested task?
    new ToolCallAccuracyEvaluator(),    // Were tools called with correct arguments?
};

// Combined
var pipeline = new EvaluationPipelineBuilder()
    .WithChatClient(chatClient)
    .ForEach(quickChecks, e => pipeline.AddEvaluator(e))
    .ForEach(agentEvals, e => pipeline.AddEvaluator(e))
    .Build();
```

**Key Differences:**
- **IntentResolution:** Did the agent understand what the user *actually* wanted?
- **TaskAdherence:** Did it complete the full task, not just part of it?
- **ToolCallAccuracy:** Were tool arguments correct? Did it use the right tools?

**CI/CD:** Use ElBruno for fast safety checks. Use Microsoft's agent evaluators for release gates (LLM-powered, sophisticated judgment).

---

## Scenario 4: Need Fast CI/CD Regression Gates

**Your Goal:** Catch quality drops before they reach production. Speed matters. Cost matters. External calls not allowed.

**Recommended Evaluators:**

```csharp
var cicdEvaluators = new List<IEvaluator>
{
    new RelevanceEvaluator(0.7),        // Fast, offline
    new CoherenceEvaluator(0.7),        // Fast, offline
    new SafetyEvaluator(0.95),          // Fast, offline
    new HallucinationEvaluator(0.75),   // Fast, offline
};

// All ElBruno—no external calls, no costs
var results = await pipeline.RunAsync();  // < 1 second for 100 examples

// Compare against baseline
var regression = await pipeline.RunWithBaselineAsync();
if (regression.HasRegressions)
    throw new Exception("Quality regression detected—blocking deployment");
```

**Why ElBruno?**
- No LLM calls = no latency
- No API keys needed
- Reproducible (same input = same output)
- Perfect for CI/CD gates
- ~$0 cost for 1000s of evaluations

---

## Scenario 5: Need Comprehensive Quality Review

**Your Goal:** Quarterly or pre-release review. Need nuanced judgment. Budget available for LLM calls. Want professional reports.

**Recommended Evaluators:**

```csharp
// Phase 1: Quick ElBruno gate
var quickPass = new[] { /* relevance, coherence, safety */ };
var quickResults = await quickPipeline.RunAsync();

if (quickResults.AggregateScore < 0.7)
    throw new Exception("Failed quick checks");

// Phase 2: Deep Microsoft evaluation
var deepEvals = new[]
{
    new MicrosoftRelevanceEvaluator(),       // Nuanced judgment
    new MicrosoftCompletenessEvaluator(),    // Does it answer fully?
    new MicrosoftFluencyEvaluator(),         // Well-written?
    new MicrosoftGroundednessEvaluator(),    // Factually grounded?
    new MicrosoftCoherenceEvaluator(),       // Logically consistent?
};

var deepResults = await microsoftPipeline.RunAsync();

// Phase 3: Generate professional report
var report = await htmlReporter.GenerateReportAsync(deepResults);
await File.WriteAllTextAsync("quality-review-q1-2025.html", report);

// Phase 4: Archive in Azure
await azureClient.UploadReportAsync(report);
```

**Microsoft's Advantages:**
- Sophisticated judgment (not just heuristics)
- LLM-powered (understands nuance, context)
- HTML reports for stakeholders
- Azure integration for enterprise scale
- Professional, auditable results

---

## Scenario 6: In Air-Gapped or Regulated Environment

**Your Goal:** No external API calls allowed. No internet. Compliance required.

**Recommended Evaluators:**

```csharp
// ONLY ElBruno—100% offline
var evaluators = new List<IEvaluator>
{
    new RelevanceEvaluator(0.7),        // No calls
    new FactualityEvaluator(0.8),       // No calls
    new CoherenceEvaluator(0.7),        // No calls
    new HallucinationEvaluator(0.75),   // No calls
    new SafetyEvaluator(0.95),          // No calls (local blocklist)
};

// Everything runs locally
var results = await pipeline.RunAsync();

// Results stored in SQLite (local, portable)
await store.SaveAsync(results);

// Can export to CSV for compliance audit
var csv = await csvExporter.ExportAsync(results);
await File.WriteAllTextAsync("evaluation-audit.csv", csv);
```

**Why ElBruno?**
- Zero external dependencies
- No network calls
- SQLite is self-contained
- CSV/JSON export for compliance
- Fully portable and auditable

---

## Evaluator Selection Matrix

| Scenario | Relevance | Factuality | Coherence | Hallucination | Safety | Microsoft (Opt) |
|----------|-----------|-----------|-----------|---------------|--------|---|
| Chatbot | ✅ (0.7) | ✅ (0.8) | ✅ (0.7) | ✅ (0.75) | ✅ (0.95) | ⚠️ (deep only) |
| RAG | ✅ (0.8) | ✅ (0.9) | ✅ (0.7) | ✅ (0.8) | ✅ (0.95) | ✅ Groundedness |
| Agent | ✅ (fast) | — | ✅ (0.7) | — | ✅ (0.95) | ✅ IntentRes, TaskAdh, ToolCall |
| CI/CD Gate | ✅ (0.7) | ⚠️ (if needed) | ✅ (0.7) | ⚠️ (if needed) | ✅ (0.95) | ❌ (too slow) |
| Quality Review | ✅ (quick) | ✅ (quick) | ✅ (quick) | ✅ (quick) | ✅ (quick) | ✅ (REQUIRED) |
| Air-Gapped | ✅ (only option) | ✅ (only option) | ✅ (only option) | ✅ (only option) | ✅ (only option) | ❌ (no internet) |

---

## Decision Tree: Quick Reference

```
What am I building?
├─ Chatbot?
│  └─ Use: Relevance, Coherence, Safety (ElBruno)
│     Optional: Microsoft Relevance for deep review
│
├─ RAG System?
│  └─ Use: Hallucination, Factuality, Relevance, Safety (ElBruno)
│     Optional: Microsoft Groundedness
│
├─ Agent?
│  └─ Use: Coherence, Safety (ElBruno)
│     MUST USE: IntentResolution, TaskAdherence, ToolCallAccuracy (Microsoft)
│
├─ Need Fast CI/CD Gate?
│  └─ Use: ALL ElBruno (Relevance, Coherence, Safety, Hallucination)
│     Never use Microsoft (too slow for gates)
│
├─ Need Comprehensive Review?
│  └─ Phase 1: All ElBruno (fast gate)
│     Phase 2: All Microsoft (deep analysis)
│     Export: HTML reports + Azure
│
└─ Air-Gapped / Regulated?
   └─ Use: ONLY ElBruno (Relevance, Factuality, Coherence, Hallucination, Safety)
      Never use Microsoft (no internet)
```

## Implementation Example: Thoughtful Selection

```csharp
public class EvaluatorFactory
{
    public static List<IEvaluator> GetEvaluators(ScenarioType scenario)
    {
        return scenario switch
        {
            ScenarioType.Chatbot => new()
            {
                new RelevanceEvaluator(0.7),
                new CoherenceEvaluator(0.7),
                new SafetyEvaluator(0.95),
                new HallucinationEvaluator(0.75),
            },
            
            ScenarioType.RAG => new()
            {
                new HallucinationEvaluator(0.8),
                new FactualityEvaluator(0.9),
                new RelevanceEvaluator(0.8),
                new SafetyEvaluator(0.95),
            },
            
            ScenarioType.FastGate => new()
            {
                new RelevanceEvaluator(0.7),
                new CoherenceEvaluator(0.7),
                new SafetyEvaluator(0.95),
            },
            
            _ => throw new ArgumentException($"Unknown scenario: {scenario}")
        };
    }
}

// Usage
var evaluators = EvaluatorFactory.GetEvaluators(ScenarioType.Chatbot);
```

---

## Key Takeaways

1. **Match evaluators to your scenario**, not to the toolkit
2. **ElBruno for gates and baselines** (fast, offline, deterministic)
3. **Microsoft for deep analysis** (LLM-powered, nuanced, reporting)
4. **Combine both for production** (quick checks + second opinion)
5. **Always include Safety** evaluators for customer-facing systems
6. **Version your evaluator configs** like you version code

---

*You now have the full developer journey: from understanding both toolkits to building production pipelines to choosing evaluators wisely. Go build something remarkable with .NET AI.*
