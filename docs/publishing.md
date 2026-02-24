# Publishing a New Version to NuGet

This guide covers how to publish new versions of the ElBruno.AI.Evaluation packages to NuGet.org using GitHub Actions and NuGet Trusted Publishing (keyless, OIDC-based).

## Packages

| Package | Project | Description |
|---------|---------|-------------|
| `ElBruno.AI.Evaluation` | `src/ElBruno.AI.Evaluation/` | Core library — evaluators, datasets, metrics, pipeline |
| `ElBruno.AI.Evaluation.Xunit` | `src/ElBruno.AI.Evaluation.Xunit/` | xUnit integration — AIAssert, AIEvaluationTest |
| `ElBruno.AI.Evaluation.Reporting` | `src/ElBruno.AI.Evaluation.Reporting/` | Reporting — SQLite store, JSON/CSV export |

> **Maintenance rule:** If a new packable library is added under `src/`, update `.github/workflows/publish.yml` in the same PR so the new project is packed/pushed, and add a matching NuGet Trusted Publishing policy.

## Prerequisites (One-Time Setup)

### 1. Configure NuGet.org Trusted Publishing Policies

1. Sign in to [nuget.org](https://www.nuget.org)
2. Click your username → **Trusted Publishing**
3. Add a policy for **each** package with these values:

| Setting | Value |
|---------|-------|
| **Repository Owner** | `elbruno` |
| **Repository** | `elbruno-ai-evaluation` |
| **Workflow File** | `publish.yml` |
| **Environment** | `release` |

You need to create this policy **three times** — once per package:

- `ElBruno.AI.Evaluation`
- `ElBruno.AI.Evaluation.Xunit`
- `ElBruno.AI.Evaluation.Reporting`

> **Note:** For new packages that don't exist on NuGet.org yet, you must first push them once (the workflow handles this). After the initial push, add the Trusted Publishing policy so future publishes are keyless.

### 2. Configure GitHub Repository

1. Go to the repo **Settings** → **Environments**
2. Create an environment called **`release`**
   - Optionally add **required reviewers** for a manual approval gate
3. Go to **Settings** → **Secrets and variables** → **Actions**
4. Add a repository secret:
   - **Name:** `NUGET_USER`
   - **Value:** `elbruno`

## Publishing a New Version

### Option A: Create a GitHub Release (Recommended)

1. **Update the version** in `Directory.Build.props`:

   ```xml
   <Version>1.1.0</Version>
   ```

2. **Commit and push** the version change to `main`
3. **Create a GitHub Release:**
   - Go to the repo → **Releases** → **Draft a new release**
   - Create a new tag: `v1.1.0` (must match the version in Directory.Build.props)
   - Fill in the release title and notes
   - Click **Publish release**
4. The **Publish to NuGet** workflow runs automatically

### Option B: Manual Dispatch

1. Go to the repo → **Actions** → **Publish to NuGet**
2. Click **Run workflow**
3. Optionally enter a version
4. Click **Run workflow**

## How It Works

```
GitHub Release created (e.g. v1.0.0)
  → GitHub Actions triggers publish.yml
    → Builds + tests all projects
    → Packs three .nupkg files
    → Requests an OIDC token from GitHub
    → Exchanges the token with NuGet.org for a temporary API key
    → Pushes all packages to NuGet.org
    → Temp key expires automatically
```

## Troubleshooting

| Problem | Solution |
|---------|----------|
| Workflow fails at "NuGet login" | Verify the Trusted Publishing policy on nuget.org matches repo owner, name, workflow file, and environment |
| `NUGET_USER` secret not found | Add the secret in GitHub repo Settings → Secrets → Actions |
| Package already exists | `--skip-duplicate` prevents failures. Bump the version number |
| OIDC token errors | Ensure `id-token: write` permission is set in the workflow job |

## Reference Links

- [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
- [NuGet/login GitHub Action](https://github.com/NuGet/login)
- [GitHub Actions OIDC](https://docs.github.com/en/actions/security-for-github-actions/security-hardening-your-deployments/about-security-hardening-with-openid-connect)
