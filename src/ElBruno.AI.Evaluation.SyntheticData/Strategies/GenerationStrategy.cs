namespace ElBruno.AI.Evaluation.SyntheticData.Strategies;

/// <summary>
/// Predefined output format templates for LLM generation.
/// Guides the LLM to produce structured GoldenExample instances.
/// </summary>
public enum GenerationTemplate
{
    /// <summary>Simple Input → Output pairs.</summary>
    SimpleQA = 0,

    /// <summary>Input + Context → Output (RAG-style).</summary>
    RagContext = 1,

    /// <summary>Input → Output + Explanation.</summary>
    QAWithExplanation = 2,

    /// <summary>Multiple variations of Input → Output for the same concept.</summary>
    QAVariations = 3,

    /// <summary>Edge cases and adversarial examples.</summary>
    AdversarialCases = 4,

    /// <summary>Domain-specific examples with metadata.</summary>
    DomainSpecific = 5,
}
