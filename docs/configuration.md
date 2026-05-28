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
  "respectInlineDisables": true
}
```

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
