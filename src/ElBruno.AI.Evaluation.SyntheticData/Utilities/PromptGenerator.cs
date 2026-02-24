using ElBruno.AI.Evaluation.SyntheticData.Strategies;

namespace ElBruno.AI.Evaluation.SyntheticData.Utilities;

/// <summary>
/// Composes LLM prompts for synthetic data generation based on generation templates.
/// </summary>
public static class PromptGenerator
{
    /// <summary>
    /// Creates a user prompt for the specified generation template and count.
    /// </summary>
    public static string CreatePrompt(GenerationTemplate template, int count)
    {
        var format = template switch
        {
            GenerationTemplate.SimpleQA =>
                """
                Generate {0} Q&A pairs as a JSON array. Each item should have "input" (question) and "expected_output" (answer) fields.
                Return ONLY the JSON array, no other text.
                """,
            GenerationTemplate.RagContext =>
                """
                Generate {0} RAG examples as a JSON array. Each item should have "input" (question), "context" (document excerpt), and "expected_output" (answer grounded in context) fields.
                Return ONLY the JSON array, no other text.
                """,
            GenerationTemplate.QAWithExplanation =>
                """
                Generate {0} Q&A pairs with explanations as a JSON array. Each item should have "input" (question), "expected_output" (answer), and "context" (explanation) fields.
                Return ONLY the JSON array, no other text.
                """,
            GenerationTemplate.QAVariations =>
                """
                Generate {0} Q&A variations as a JSON array. For each concept, provide multiple phrasings. Each item should have "input" (question variation) and "expected_output" (answer) fields.
                Return ONLY the JSON array, no other text.
                """,
            GenerationTemplate.AdversarialCases =>
                """
                Generate {0} adversarial/edge-case examples as a JSON array. Include typos, contradictions, empty-like inputs, and boundary cases. Each item should have "input" and "expected_output" fields.
                Return ONLY the JSON array, no other text.
                """,
            GenerationTemplate.DomainSpecific =>
                """
                Generate {0} domain-specific Q&A examples as a JSON array. Each item should have "input" (domain question), "expected_output" (domain answer), and "context" (domain context) fields.
                Return ONLY the JSON array, no other text.
                """,
            _ =>
                """
                Generate {0} Q&A examples as a JSON array with "input" and "expected_output" fields.
                Return ONLY the JSON array, no other text.
                """
        };

        return string.Format(format, count);
    }
}
