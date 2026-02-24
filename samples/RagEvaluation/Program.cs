using ElBruno.AI.Evaluation.Evaluators;

Console.WriteLine("📚 RAG Evaluation Sample");
Console.WriteLine("========================");

var result = new EvaluationResult
{
    Score = 0.92,
    Passed = true,
    Details = "Sample RAG evaluation result"
};

Console.WriteLine($"Score: {result.Score:F2} | Passed: {result.Passed}");
