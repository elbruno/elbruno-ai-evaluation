# ReportingShowcase

This sample demonstrates **end-to-end evaluation result management** using the **ElBruno.AI.Evaluation.Reporting** package, showing how to:

1. **Run evaluators** on sample data
2. **Store results** in SQLite for persistence and querying
3. **Export to JSON** for archival and downstream processing
4. **Export to CSV** for analysis in spreadsheets or BI tools
5. **Print a console summary** for immediate visibility

## What This Shows

### The Problem
After running evaluations, you need to:
- Persist results for compliance and auditing
- Export data for analysis, dashboards, and reporting
- Query historical runs to detect regressions
- Share results with non-technical stakeholders

### The Solution
This sample uses `ReportingShowcase` to demonstrate all built-in export formats working together on a realistic customer-support chatbot evaluation scenario.

## Running the Sample

```bash
cd samples/ReportingShowcase
dotnet run
```

You'll see:
1. **SQLite storage** — Results persisted in `evaluation.db`
2. **JSON export** — Structured output for programmatic processing in `results.json`
3. **CSV export** — Tabular format in `results.csv` 
4. **Console reporter** — Real-time summary with pass/fail breakdown

## File Outputs

After running, you'll find in the executable directory:

- **evaluation.db** — SQLite database with `Runs` and `Results` tables
- **results.json** — Complete run metadata, metrics, and scores
- **results.csv** — Tabular format with one row per example + per-evaluator metrics as columns

## Code Highlights

### Simulating Evaluators
```csharp
var evaluators = new IEvaluator[]
{
    new RelevanceEvaluator(threshold: 0.6),
    new HallucinationEvaluator(threshold: 0.7)
};
```

### Running Evaluations and Building the Run
```csharp
foreach (var (example, output) in dataset.Examples.Zip(sampleOutputs))
{
    var result = await evaluator.EvaluateAsync(example.Input, output, example.ExpectedOutput);
    run.Results.Add(new EvaluationResult { /* ... */ });
}
```

### SQLite Persistence
```csharp
using (var store = await SqliteResultStore.CreateAsync(dbPath))
{
    await store.SaveRunAsync(run);  // Persists entire run with results
    var savedRuns = await store.GetRunsAsync(datasetName);  // Query by dataset
}
```

### Multi-Format Export
```csharp
var jsonExporter = new JsonExporter();
await jsonExporter.ExportAsync(run, "results.json");

var csvExporter = new CsvExporter();
await csvExporter.ExportAsync(run, "results.csv");

var reporter = new ConsoleReporter { Verbose = true };
reporter.Report(run);  // Pretty-printed console output
```

## Real-World Use Cases

### 1. CI/CD Integration
Store results in SQLite per commit, query to detect regressions before merging.

### 2. Monitoring Dashboards
Export to JSON, push to your observability platform (Grafana, Datadog, etc.) for trending.

### 3. Compliance & Auditing
CSV exports satisfy audit requirements with timestamped, queryable history.

### 4. Team Communication
Console reports provide instant visibility to stakeholders without tooling overhead.

## Next Steps

- See **[docs/best-practices.md](../../docs/best-practices.md)** for production patterns
- See **[docs/evaluation-metrics.md](../../docs/evaluation-metrics.md)** for evaluator tuning
- Explore **[samples/EvaluationJourney](../EvaluationJourney/)** for baseline + regression detection
