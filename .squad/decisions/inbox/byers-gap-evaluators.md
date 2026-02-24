# Decision: Gap Evaluators — Differentiation from Microsoft.Extensions.AI.Evaluation

**Author:** Byers (Senior .NET Developer)  
**Date:** 2025-07-15  
**Status:** Implemented

## Decision
Added 5 new evaluators and a static DatasetLoader that fill gaps not covered by Microsoft's official evaluation libraries, plus added SyntheticData to the publish pipeline.

## New Evaluators
| Evaluator | Gap Filled |
|---|---|
| LatencyEvaluator | Operational metrics (Microsoft only does text quality) |
| CostEvaluator | Cost tracking (Microsoft tracks none) |
| ConcisenessEvaluator | Verbosity detection (Microsoft has Fluency, not conciseness) |
| ConsistencyEvaluator | Self-contradiction detection (Microsoft's Coherence is LLM-based flow) |
| CompletenessEvaluator | Heuristic multi-part question coverage (Microsoft's is LLM-based) |

## Key Design Choices
- All evaluators are deterministic/offline — no LLM dependency, works air-gapped
- All implement IEvaluator interface for pipeline compatibility
- Linear decay scoring model for latency/cost ensures graceful degradation rather than hard pass/fail
- DatasetLoaderStatic uses static methods (not instance) to complement existing IDatasetLoader pattern
- CSV parsing is hand-rolled to avoid external dependency (no CsvHelper)

## Rationale
The comparison doc identified specific gaps where Microsoft's libraries don't provide coverage. Our heuristic approach is a feature, not a limitation — it enables offline, deterministic, fast evaluation without API costs.
