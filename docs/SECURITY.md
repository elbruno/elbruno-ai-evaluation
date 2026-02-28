# Security & Safety Guarantees

ElBruno.AI.Evaluation implements multiple safety layers to protect your data and prevent common file-handling vulnerabilities. This document explains the security features you get out of the box.

## File Operations Safety

All file I/O operations use .NET's built-in `System.IO` APIs which are inherently safe against path traversal attacks. When you load or save datasets, the library:

- **Validates file paths** — Uses `Path.Combine()` and `Path.GetDirectoryName()` which are immune to directory traversal attempts (e.g., `../../../etc/passwd` patterns). The `System.IO` namespace automatically normalizes paths and rejects attempts to escape intended directories.
- **Handles missing parent directories** — Before writing files, the library creates parent directories using `Directory.CreateDirectory()`, eliminating the need to manually manage folder structure.
- **Never executes dynamic paths** — JSON/CSV file paths come from explicit method parameters only; the library never constructs paths from user input contained within files.

**Bottom line:** Whether you're loading from `data/customer-support.json`, an absolute path like `C:\datasets\test.csv`, or a relative path, the underlying file I/O is safe from directory-escape exploits.

## File Integrity

Data validation is applied at multiple stages to ensure only expected content is processed:

**Dataset Schema Validation:**
- JSON deserialization enforces the `GoldenDataset` type contract — unexpected fields are ignored, missing required fields throw `InvalidOperationException`. CSV imports require `Input` and `ExpectedOutput` columns; missing or malformed columns are rejected with clear error messages.
- Size limits are not hard-coded; instead, rely on file system and memory constraints. For enterprise workflows, place evaluation datasets in monitored storage (e.g., Azure Blob Storage) with quota policies.

**Content Quality Checks:**
- The `ValidateExamples()` extension method (from `SyntheticDatasetExtensions`) lets you enforce custom validation rules: minimum/maximum input length, regex pattern matching, token counts, or any domain-specific constraint.
- Deduplication via `Deduplicate()` removes accidentally repeated examples, preventing skewed evaluation results.

**Evaluator Safeguards:**
- The `SafetyEvaluator` includes built-in blocklists for PII patterns (email, phone, SSN regex) and profanity detection, flagging potentially unsafe content before it's processed.
- All text processing (tokenization, similarity scoring) handles null, empty, and Unicode strings gracefully without throwing exceptions.

## Best Practices

When using the library in production, follow these guidelines:

**1. Validate Before You Trust**
After loading a dataset, always call `dataset.ValidateExamples(new ValidationOptions { MaxInputLength = 2000 })` with thresholds that match your domain. For RAG scenarios, verify that `Context` fields contain expected schemas.

**2. Use Tagged Subsets for Safety**
Instead of using entire datasets in critical paths, tag examples by risk level (`"production"`, `"staging"`, `"edge-case"`) and load only production-validated subsets with `dataset.GetByTag("production")`.

**3. Store Evaluation Results Securely**
Results persist in SQLite at a path you control. Treat the database file like any other sensitive artifact:
- Store in a secure location (not in `public/` or version control)
- Restrict file permissions to the application process and authorized admins
- For cloud deployments, use managed database services (Azure SQL, RDS) instead of file-based SQLite

**4. Monitor for Anomalies**
Use regression detection (`RegressionDetector`) to catch sudden quality drops. A sharp decline in metrics often indicates corrupted data, changed input distributions, or model drift — all worth investigating before reaching production.

**5. Version Your Datasets**
Datasets support semantic versioning (`1.0.0` format). Always pin the exact dataset version when running evaluations, especially for compliance scenarios. This creates an audit trail and makes it easy to reproduce historical results.

---

**Questions?** Open a [Discussion](https://github.com/elbruno/elbruno-ai-evaluation/discussions) on GitHub or check the [Best Practices guide](best-practices.md) for production deployment patterns.
