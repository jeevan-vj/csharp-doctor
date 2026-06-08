# HTML report

CSharp Doctor can emit a self-contained HTML dashboard for scan results. The report is designed for local triage and CI artifacts (similar to NDepend HTML reports, with a SonarQube-style overview and gate-first layout).

## Generate a report

```bash
csharp-doctor . --all-checks --format html
```

Writes `<scan-path>/csharp-doctor-report.html` by default. Stdout prints only the output path — open that file in a browser.

Custom path:

```bash
csharp-doctor . --all-checks --format html --output artifacts/csharp-doctor-report.html
```

## Report sections

| Section | Purpose |
|---------|---------|
| **Summary** | Pass/fail, errors, warnings, debt score, scan mode |
| **Quality gates** | Failed gates first with actual vs limit |
| **Hot spots** | Fix priority, by rule, by file (click to filter findings) |
| **Domain highlights** | Security/SCA, architecture, maintainability |
| **Findings explorer** | Search and filter all diagnostics with recommendations |
| **Recommendation board** | Fix now, next sprint, watch, configure |
| **Raw JSON** | Embedded schema v2 payload for debugging |

## Suggested quality gate workflow

1. Run a clean scan on `main` and save JSON as baseline:

```bash
csharp-doctor . --all-checks --json > .csharp-doctor-baseline.json
```

2. Configure gates in `.csharp-doctor.json`:

```json
{
  "qualityGates": {
    "enabled": true,
    "maxWarnings": 25,
    "maxNewIssues": 0,
    "baselinePath": ".csharp-doctor-baseline.json"
  }
}
```

3. On PRs, generate HTML for reviewers and fail CI on gate violations:

```bash
csharp-doctor . --all-checks --format html --output artifacts/csharp-doctor-report.html
```

## CI artifact example (GitHub Actions)

```yaml
- name: CSharp Doctor scan
  run: |
    dotnet tool run csharp-doctor -- . --all-checks \
      --format html --output "${{ runner.temp }}/csharp-doctor-report.html"

- name: Upload HTML report
  uses: actions/upload-artifact@v4
  with:
    name: csharp-doctor-report
    path: ${{ runner.temp }}/csharp-doctor-report.html
```

## Notes

- HTML uses embedded CSS, JavaScript, and JSON v2 data. No external assets required.
- Findings remain readable without JavaScript; filters and tabs enhance exploration when JS is enabled.
- Pair HTML with `--json` in CI if you need machine-readable output for baselines or downstream tooling.
