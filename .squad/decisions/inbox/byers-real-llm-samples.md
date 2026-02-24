# Decision: Real LLM Evaluation Samples

**Author:** Byers (Senior .NET Developer)  
**Date:** 2025-02-24  
**Status:** Implemented

## Decision
Created two new sample projects demonstrating real LLM evaluation (no mocks) using local model providers with ElBruno.AI.Evaluation evaluators.

## Key Design Choices
- **OllamaEvaluation** uses `Microsoft.Extensions.AI.Ollama` (preview) — no stable release exists yet
- **FoundryLocalEvaluation** uses `Microsoft.Extensions.AI.AzureAIInference` + `Azure.AI.Inference` (both preview)
- Evaluation thresholds set to 0.3 for relevance/hallucination/factuality (heuristic evaluators measure token overlap, not semantic match — real LLM output diverges from expected text)
- Safety (0.9) and Coherence (0.7) thresholds kept at defaults since they evaluate output structure, not content similarity
- Both samples use try/catch with actionable error messages guiding users to install/start the local model server

## Impact
- Two new sample projects added to samples/ and root README
- Demonstrates the toolkit works with real model providers, not just mocks
- Preview NuGet packages may require version bumps as stable releases ship
