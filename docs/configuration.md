# Configuration

CSharp Doctor reads `.csharp-doctor.json` from the scan directory or any ancestor up to the solution/git boundary.

## Example

```json
{
  "lint": true,
  "failOn": "error",
  "rules": {
    "csharp-doctor/CSD0001": "error",
    "csharp-doctor/CSD0201": "warning"
  },
  "categories": {
    "Performance": "warning",
    "AspNetCore": "warning",
    "Maintainability": "info"
  },
  "ignore": {
    "rules": ["csharp-doctor/CSD0203"],
    "files": ["**/Migrations/**"],
    "tags": ["design"]
  },
  "surfaces": {
    "prComment": { "excludeTags": ["design"] },
    "ciFailure": { "excludeTags": ["design"] }
  },
  "respectInlineDisables": true,
  "checks": {
    "lint": true,
    "wiring": false,
    "ef": false,
    "architecture": false,
    "sca": false
  },
  "architecturePolicy": {
    "forbiddenNamespaceDependencies": [
      { "from": "MyApp.UI", "to": "MyApp.Data" }
    ],
    "forbiddenAssemblyReferences": [
      { "from": "MyApp.UI", "to": "MyApp.Data" }
    ]
  },
  "qualityGates": {
    "enabled": true,
    "maxErrors": 0,
    "maxWarnings": 25,
    "maxDebtScore": 100,
    "maxNewIssues": 0,
    "baselinePath": ".csharp-doctor-baseline.json",
    "maxIssuesPerRule": {
      "csharp-doctor/CSD0001": 0
    },
    "debtWeights": { "error": 10, "warning": 3, "info": 1 }
  },
  "hotSpots": {
    "topRules": 10,
    "topFiles": 10,
    "topElements": 15
  }
}
```

## Quality gates and hot spots

When `qualityGates.enabled` is true, aggregate thresholds are evaluated after surface filtering (same set used for CI failure). Failed gates set exit code `1` even when `failOn` is `none`.

JSON output (`--json`) uses **schema version 2** and includes:

- `metrics` — error/warning/info counts and weighted `debtScore`
- `qualityGates` — per-gate pass/fail with limits and actuals
- `hotSpots` — `issuesByRule`, `issuesByFile`, and `fixPriority` rankings

Save a prior JSON report as `baselinePath` to gate on `maxNewIssues` (compares diagnostic `identity` values).

## Precedence

1. CLI flags (`--fail-on`, `--no-lint`, etc.)
2. `.csharp-doctor.json`
3. Built-in defaults

## Inline suppression

```csharp
// csharp-doctor-disable-next-line
var x = dangerous();

// csharp-doctor-disable-line csharp-doctor/CSD0001
```

## ASP.NET capability gating

Rules tagged `aspnet` (for example `CSD0401`–`CSD0416`) are reported only when the source file belongs to a project that references ASP.NET Core (`Microsoft.NET.Sdk.Web` or `Microsoft.AspNetCore.*` packages). Class libraries without those references will not surface ASP.NET findings.

## Rule and tag reference

See [docs/rules/README.md](rules/README.md) for the full rule index and CA complement matrix.
