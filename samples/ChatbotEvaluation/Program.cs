using ElBruno.AI.Evaluation.Evaluators;

Console.WriteLine("🤖 Chatbot Evaluation Sample");
Console.WriteLine("============================");

var result = new EvaluationResult
{
    Score = 0.85,
    Passed = true,
    Details = "Sample evaluation result"
};

Console.WriteLine($"Score: {result.Score:F2} | Passed: {result.Passed}");
