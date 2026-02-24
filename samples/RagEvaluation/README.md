# RAG Pipeline Evaluation Sample

Demonstrates how to evaluate a Retrieval-Augmented Generation (RAG) pipeline using **ElBruno.AI.Evaluation**.

## What This Sample Shows

1. **Mock RAG Pipeline** — Simulates a retrieval + generation system with context-aware responses
2. **RAG Dataset** — Loads `rag-examples.json` with input, expected output, and retrieved context
3. **RAG-Specific Evaluators** — Runs FactualityEvaluator, HallucinationEvaluator, and CoherenceEvaluator
4. **Regression Detection** — Compares a new run against a saved baseline to detect quality drops
5. **JSON Export** — Exports full results with metrics and regression data to JSON

## How to Run

```bash
cd samples/RagEvaluation
dotnet run
```

## Expected Output

The sample will:
- Load 7 RAG examples with context from the golden dataset
- Run a baseline evaluation with 3 evaluators (Factuality, Hallucination, Coherence)
- Create a baseline snapshot capturing per-metric scores
- Run regression detection comparing a "new version" against the baseline
- Print a detailed regression report showing improved/regressed/unchanged metrics
- Export all results to `rag-evaluation-results.json`

## Key APIs Demonstrated

| API | Purpose |
|-----|---------|
| `JsonDatasetLoader.LoadAsync()` | Load RAG golden datasets with context |
| `IChatClient.EvaluateAsync()` | Run evaluators across a full dataset |
| `EvaluationRun.ToBaseline()` | Snapshot current scores as a baseline |
| `IChatClient.CompareBaselineAsync()` | Regression detection against baseline |
| `RegressionReport` | Categorize metrics as improved/regressed/unchanged |
