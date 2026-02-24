# Ollama Evaluation Sample

Evaluates a **real local LLM** (Ollama `phi3:mini`) using ElBruno.AI.Evaluation — no mocks, real model output.

## Prerequisites

### Install Ollama

| Platform | Command |
|----------|---------|
| **Windows** | `winget install Ollama.Ollama` |
| **macOS** | `brew install ollama` |
| **Linux** | `curl -fsSL https://ollama.com/install.sh \| sh` |

Or download from [https://ollama.com](https://ollama.com).

### Pull the model

```bash
ollama pull phi3:mini
```

### Start Ollama

```bash
ollama serve
```

This starts the Ollama API server on **http://localhost:11434**.

## Run the Sample

```bash
dotnet run
```

## What It Does

1. Connects to a local Ollama instance running `phi3:mini`
2. Sends 5 customer support questions to the real model
3. Runs 5 evaluators against each response:
   - **RelevanceEvaluator** — cosine similarity between input and output terms
   - **HallucinationEvaluator** — checks if output tokens are grounded in the reference
   - **SafetyEvaluator** — scans for PII and unsafe content
   - **CoherenceEvaluator** — checks sentence structure and contradiction detection
   - **FactualityEvaluator** — verifies claims against expected output
4. Prints a formatted results table with per-example metric breakdowns

## Expected Output

```
🦙 Ollama Evaluation Sample — Real LLM Evaluation
==================================================

📌 Step 1: Connecting to Ollama at http://localhost:11434 (model: phi3:mini)...
   ✅ Chat client created

📌 Step 2: Building golden dataset (customer support)...
   ✅ Dataset 'customer-support-ollama' with 5 examples

...

📌 Step 5: Evaluation Results
═══════════════════════════════════════════════════════════════════════
  Run ID:          <guid>
  Dataset:         customer-support-ollama
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
