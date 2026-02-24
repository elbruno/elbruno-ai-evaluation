# Foundry Local Evaluation Sample

Evaluates a **real local LLM** (Azure AI Foundry Local `phi-4-mini`) using ElBruno.AI.Evaluation — no mocks, real model output.

## Prerequisites

### Install Foundry Local

| Platform | Command |
|----------|---------|
| **Windows** | `winget install Microsoft.FoundryLocal` |
| **Other** | See [Foundry Local documentation](https://learn.microsoft.com/en-us/azure/ai-foundry/foundry-local/get-started) |

### Start the server

```bash
# Run with default model
foundry model run phi-4-mini

# Or start the server directly
foundry local
```

The server runs on **http://localhost:5272** by default.

### Useful commands

```bash
# List available models
foundry model list

# Check server status
foundry status
```

## System Requirements

- **RAM:** 8 GB minimum (16 GB recommended)
- **Disk:** ~4 GB for the phi-4-mini model
- Foundry Local manages model downloads automatically on first run

## Run the Sample

```bash
dotnet run
```

## What It Does

1. Connects to Foundry Local running `phi-4-mini`
2. Sends 5 technical documentation questions (.NET/C# theme) to the real model
3. Runs 5 evaluators against each response:
   - **RelevanceEvaluator** — cosine similarity between input and output terms
   - **HallucinationEvaluator** — checks if output tokens are grounded in the reference
   - **SafetyEvaluator** — scans for PII and unsafe content
   - **CoherenceEvaluator** — checks sentence structure and contradiction detection
   - **FactualityEvaluator** — verifies claims against expected output
4. Prints a formatted results table with per-example metric breakdowns

## Expected Output

```
🏭 Foundry Local Evaluation Sample — Real LLM Evaluation
=========================================================

📌 Step 1: Connecting to Foundry Local at http://localhost:5272 (model: phi-4-mini)...
   ✅ Chat client created

📌 Step 2: Building golden dataset (technical documentation)...
   ✅ Dataset 'tech-docs-foundry' with 5 examples

...

📌 Step 5: Evaluation Results
═══════════════════════════════════════════════════════════════════════
  Run ID:          <guid>
  Dataset:         tech-docs-foundry
  Aggregate Score: 0.XXXX
  Pass Rate:       XX.X%
  All Passed:      True/False
═══════════════════════════════════════════════════════════════════════
```

> **Note:** Results will vary between runs since this uses a real model generating different responses each time. Thresholds are set lower (0.3) for relevance, hallucination, and factuality to account for natural variation in model output.

## Evaluators Used

| Evaluator | Threshold | What It Checks |
|-----------|-----------|----------------|
| Relevance | 0.3 | Term overlap between question and answer |
| Hallucination | 0.3 | Output tokens grounded in reference material |
| Safety | 0.9 | PII patterns and profanity blocklist |
| Coherence | 0.7 | Sentence completeness and contradictions |
| Factuality | 0.3 | Claims supported by expected output |
