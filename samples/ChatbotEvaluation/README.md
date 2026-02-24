# Chatbot Evaluation Sample

Demonstrates how to evaluate a chatbot using **ElBruno.AI.Evaluation**.

## What This Sample Shows

1. **Mock IChatClient** — A deterministic echo client that returns known responses
2. **Dataset Loading** — Loads the bundled `chatbot-examples.json` golden dataset
3. **Multi-Evaluator Pipeline** — Runs HallucinationEvaluator, RelevanceEvaluator, and SafetyEvaluator
4. **Console Reporting** — Prints per-example scores and pass/fail status
5. **SQLite Persistence** — Saves evaluation results to a local SQLite database
6. **Baseline Comparison** — Creates a baseline snapshot and runs regression detection

## How to Run

```bash
cd samples/ChatbotEvaluation
dotnet run
```

## Expected Output

The sample will:
- Load 7 chatbot Q&A examples from the golden dataset
- Evaluate each example against 3 evaluators (Hallucination, Relevance, Safety)
- Print a detailed results table with per-metric scores
- Save results to `evaluation-results.db` (SQLite)
- Compare against a baseline and report regressions

## Key APIs Demonstrated

| API | Purpose |
|-----|---------|
| `JsonDatasetLoader.LoadAsync()` | Load golden datasets from JSON |
| `IChatClient.EvaluateAsync()` | Evaluate a dataset with multiple evaluators |
| `EvaluationRun.ToBaseline()` | Create a baseline snapshot from results |
| `IChatClient.CompareBaselineAsync()` | Detect regressions against a baseline |
| `RegressionReport` | View improved/regressed/unchanged metrics |
