# CSharp Doctor

Hybrid .NET code health scanner: Roslyn analyzers + CLI, modeled after [react-doctor](react-doctor).

## Quick start (like `npx react-doctor@latest`)

**Prerequisites:** [.NET SDK 8+](https://dotnet.microsoft.com/download) (SDK 10+ recommended for `dnx`).

From your repo root (where `.sln` or `.csproj` files live):

```bash
# One-shot, no global install (.NET 10+ SDK) — closest to npx
dnx CSharpDoctor.Cli@latest -- .

# Same via dotnet CLI
dotnet tool exec CSharpDoctor.Cli@latest -- .
```

Pin a version in CI:

```bash
dnx CSharpDoctor.Cli@0.1.0 -- . --fail-on error
```

If the package is not on NuGet.org yet, use [install from source](#install) or a local feed first.

## Run against a project

`csharp-doctor` scans C# under a directory. It auto-detects `.sln` / `.csproj`, loads rules, and reads `.csharp-doctor.json` from the scan path or a parent folder.

| What you have | Command |
|---------------|---------|
| Solution or repo root | `csharp-doctor .` |
| Specific folder | `csharp-doctor ./src/MyApp` |
| Specific project or solution | `csharp-doctor --project ./src/MyApp/MyApp.csproj` |
| Only PR changes | `csharp-doctor --diff main` |
| Only staged files | `csharp-doctor --staged` |
| CI: fail on errors | `csharp-doctor . --fail-on error` |
| JSON report | `csharp-doctor . --format json` |
| SARIF (e.g. GitHub Advanced Security) | `csharp-doctor . --format sarif > report.sarif` |
| GitHub Actions annotations | `csharp-doctor . --annotations` |

**Example workflow**

```bash
cd /path/to/your/dotnet-repo

# First run (install globally once)
dotnet tool install -g CSharpDoctor.Cli

# Full scan
csharp-doctor .

# Pre-commit: only what you changed
csharp-doctor --staged

# PR CI: diff against main
csharp-doctor --diff origin/main --fail-on error --annotations
```

Optional config at the solution root: [`.csharp-doctor.json`](docs/configuration.md) (rule severity, ignores, surface filters).

## Install & distribute

### 1. Global tool (recommended for daily use)

After [publishing to NuGet](https://www.nuget.org/packages/CSharpDoctor.Cli):

```bash
dotnet tool install -g CSharpDoctor.Cli
csharp-doctor --help
```

Update:

```bash
dotnet tool update -g CSharpDoctor.Cli
```

### 2. One-shot / `npx` style (`dnx` or `dotnet tool exec`)

No global install; downloads the package to the NuGet cache and runs it (**.NET 10.0.100+ SDK** for `dnx`):

```bash
dnx CSharpDoctor.Cli@latest -- .
dotnet tool exec CSharpDoctor.Cli@latest -- .
```

| react-doctor | csharp-doctor |
|--------------|---------------|
| `npx react-doctor@latest` | `dnx CSharpDoctor.Cli@latest -- .` |
| `npx react-doctor@latest ./apps/web` | `dnx CSharpDoctor.Cli@latest -- ./apps/web` |
| `npx react-doctor@latest --diff` | `dnx CSharpDoctor.Cli@latest -- . --diff main` |

### 3. Local tool (pin version per repo)

In your repository root:

```bash
dotnet new tool-manifest   # once, creates .config/dotnet-tools.json
dotnet tool install CSharpDoctor.Cli --version 0.1.0
dotnet tool run csharp-doctor -- .
# or: dotnet csharp-doctor .
```

Commit `.config/dotnet-tools.json` so everyone (and CI) uses the same version.

### 4. GitHub Actions

Use the composite action in this repo ([`action.yml`](action.yml)):

```yaml
- uses: your-org/csharp-doctor@v1
  with:
    directory: .
    fail-on: error
    diff: main          # optional
    annotations: true   # optional
```

Or install the tool in a step:

```yaml
- uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '10.0.x'
- run: dotnet tool install -g CSharpDoctor.Cli
- run: csharp-doctor . --fail-on error
```

### 5. Build and install from source

```bash
git clone https://github.com/your-org/csharp-doctor.git
cd csharp-doctor
dotnet pack src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj -c Release -o ./nupkg
dotnet tool install -g --add-source ./nupkg CSharpDoctor.Cli

# Smoke test against the bundled sample (expects CSD0001 + CSD0002)
csharp-doctor samples/Sample
```

See [samples/README.md](samples/README.md) for tool-path install and `DOTNET_ROOT` notes on macOS.

### 6. Run without installing (development)

```bash
dotnet run --project src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj -- /path/to/your/project
```

## CLI reference

```bash
csharp-doctor [path]              # full scan (default: current directory)
csharp-doctor --project <path>    # specific .csproj or .sln
csharp-doctor --diff <ref>        # changed files only (e.g. main, origin/main)
csharp-doctor --staged            # staged files only
csharp-doctor --format json       # JSON report
csharp-doctor --format sarif      # SARIF (redirect to a file)
csharp-doctor --fail-on error     # exit 1 when errors present (CI gate)
csharp-doctor --fail-on warning   # stricter gate
csharp-doctor --fail-on none      # never fail on findings
csharp-doctor --annotations       # GitHub Actions workflow commands
csharp-doctor --no-lint           # skip Roslyn analyzers (config-only path)
csharp-doctor --wiring            # also run wiring checks (opt in; whole solution)
csharp-doctor --ef                # also run EF Core migration checks (opt in)
csharp-doctor --no-wiring         # explicitly disable wiring checks
csharp-doctor --no-ef             # explicitly disable EF migration checks
csharp-doctor --all-checks        # enable every inspector (lint + wiring + ef)
```

## Solution-scoped checks (opt in)

By default `csharp-doctor` runs lint only (the Roslyn analyzers above). The wiring and EF
inspectors load the whole solution, so they are **off unless you ask for them**.

| Flag | What it detects |
|------|-----------------|
| `--wiring` | DI wiring gaps in the composition root. Ships `CSDW0001` (unregistered hosted service), `CSDW0008` (unregistered constructor dependency), `CSDW0010` (singleton depends on scoped service). |
| `--ef` | Unsafe EF Core migrations. Ships `CSDE0001` (drop column), `CSDE0002` (NOT NULL column without default), `CSDE0006` (type shrink data loss). Lint also ships `CSDE0008` (`SaveChanges` in loop, `efcore` tag). |
| `--all-checks` | All inspectors. |

Precedence is `--no-X` > `--X` > config `checks.X` > default (`lint=true`, `wiring=false`, `ef=false`).
`--wiring` is ignored with `--staged` (it needs whole-solution context) and warns on stderr.

Enable them for a repo via `.csharp-doctor.json`:

```json
{ "checks": { "wiring": true, "ef": true } }
```

Note: wiring/EF rules are tagged `solution-scoped`. The first `--wiring` run pays a one-time
MSBuild workspace load (~1-2s); lint-only runs are unaffected.

## Rules

**57 rules** across Security, Correctness, Performance, Maintainability, Observability, ASP.NET Core, Wiring, and EF migrations. ASP.NET rules apply only to web projects (detected from `.csproj`).

| ID | Category | Severity | Summary |
|----|----------|----------|---------|
| CSD0001 | Security | Warning | Concatenated SQL `CommandText` |
| CSD0002 | Security | Warning | Weak crypto (`MD5`/`SHA1`) |
| CSD0003 | Security | Error | Insecure deserialization |
| CSD0004 | Security | Warning | Hardcoded secrets in literals |
| CSD0005 | Security | Warning | `System.Random` in security context |
| CSD0006 | Security | Warning | Empty `catch` blocks |
| CSD0007 | Security | Warning | Raw SQL with interpolation/concat |
| CSD0009 | Security | Warning | Unsafe XML DTD processing |
| CSD0010 | Security | Error | Newtonsoft `TypeNameHandling` not None |
| CSD0011 | Security | Error | TLS cert validation bypass |
| CSD0012 | Security | Warning | Weak TLS (`Tls`/`Tls11`) |
| CSD0013 | Security | Warning | Insecure JWT bearer options |
| CSD0014 | Security | Warning | Secrets in attribute arguments |
| CSD0015 | Security | Warning | `Regex` without match timeout |
| CSD0016 | Security | Warning | Path traversal via dynamic file path |
| CSD0017 | Security | Warning | Command injection via `Process.Start` |
| CSD0018 | Security | Warning | SSRF via dynamic HTTP URL |
| CSD0019 | Security | Warning | Open redirect via dynamic URL |
| CSD0101 | Correctness | Warning | Unobserved `Task` |
| CSD0102 | Correctness | Warning | `IDisposable` without `using` |
| CSD0103 | Correctness | Warning | Sync-over-async (`.Result`/`.Wait()`) |
| CSD0104 | Correctness | Info | Nullable `!` misuse |
| CSD0105 | Correctness | Warning | `async void` (non-event) |
| CSD0106 | Correctness | Warning | `DateTime.Now` on persistence fields |
| CSD0107 | Correctness | Warning | Class `Dispose` omits fields |
| CSD0108 | Correctness | Warning | `lock(this)` / `lock(typeof(...))` |
| CSD0201 | Performance | Warning | LINQ materialization in loop |
| CSD0202 | Performance | Warning | String concat in loop |
| CSD0203 | Performance | Info | Boxing in logging |
| CSD0204 | Performance | Warning | `new HttpClient()` in loop |
| CSD0205 | Performance | Warning | Multiple `IEnumerable` enumeration |
| CSD0206 | Performance | Warning | `string.Format` in loop |
| CSDE0008 | Performance | Warning | `SaveChanges` / `SaveChangesAsync` in loop (`efcore` tag) |
| CSD0301 | Maintainability | Info | Public instance fields (`design` tag) |
| CSD0401 | AspNetCore | Warning | Minimal API without auth |
| CSD0402 | AspNetCore | Error | `DbContext` as singleton |
| CSD0403 | AspNetCore | Error | CORS `AllowAnyOrigin` + `AllowCredentials` |
| CSD0404 | AspNetCore | Info | `IFormFile` without size limit |
| CSD0405 | AspNetCore | Warning | `AddNewtonsoftJson()` without version pin |
| CSD0406 | AspNetCore | Info | Missing `UseHttpsRedirection` |
| CSD0407 | AspNetCore | Info | GET query without `AsNoTracking` |
| CSD0408 | AspNetCore | Warning | `WriteAsync` in middleware without `try` |
| CSD0409 | AspNetCore | Warning | Swagger without Development guard |
| CSD0410 | AspNetCore | Warning | `HttpPost` without antiforgery |
| CSD0411 | AspNetCore | Warning | Insecure cookie policy |
| CSD0413 | AspNetCore | Info | `AllowAnonymous` with auth configured |
| CSD0414 | AspNetCore | Warning | `AddAuthentication` without `UseAuthentication` |
| CSD0415 | AspNetCore | Warning | EF `EnableSensitiveDataLogging` |
| CSD0416 | AspNetCore | Warning | EF `EnableDetailedErrors` |
| CSD0501 | Observability | Warning | Sensitive data in logs |
| CSD0502 | Observability | Warning | Exception not passed correctly to `LogError` |
| CSD0503 | Observability | Warning | Raw HTTP request body/headers logged |
| CSDW0001 | Wiring | Warning | Unregistered hosted service (`--wiring`) |
| CSDW0008 | Wiring | Warning | Unregistered constructor dependency (`--wiring`) |
| CSDW0010 | Wiring | Warning | Singleton depends on scoped service (`--wiring`) |
| CSDE0001 | EfMigration | Warning | Migration drops column (`--ef`) |
| CSDE0002 | EfMigration | Warning | NOT NULL column without default (`--ef`) |
| CSDE0006 | EfMigration | Warning | Column type shrink data loss (`--ef`) |

See [docs/rules/README.md](docs/rules/README.md) for the full index, CA complement matrix, and recommended `.editorconfig` preset.

## Configuration

Create `.csharp-doctor.json` at solution root. See [docs/configuration.md](docs/configuration.md).

## Development

```bash
dotnet test
```

Follow TDD: write failing tests first (see `.cursor/rules/csharp-tdd-test-first.mdc`).

## Releasing to NuGet

The CLI is packaged as a [.NET Global Tool](https://learn.microsoft.com/dotnet/core/tools/global-tools). Versions are driven by git tags via [`.github/workflows/release.yml`](.github/workflows/release.yml).

### One-time setup

1. Reserve the `CSharpDoctor.*` prefix on nuget.org (account → ID Prefix Reservations).
2. Create an API key scoped to `Push new packages and package versions` for glob `CSharpDoctor.*`.
3. Store it as the `NUGET_API_KEY` repository secret in GitHub.
4. (Optional, recommended) After the first publish, enable [NuGet Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing) for this repo + workflow and switch the release job to OIDC (the workflow has a commented-out `nuget/login@v1` block ready to flip on, then delete the secret).

### Cut a release

```bash
# Pre-release (recommended for first ship — System.CommandLine dep is still prerelease)
git tag v0.1.0-preview.1 && git push --tags

# Stable
git tag v0.1.0 && git push --tags
```

The workflow restores, builds, tests, packs, pushes `.nupkg` + `.snupkg` to nuget.org with `--skip-duplicate`, and creates a GitHub Release (marked prerelease automatically if the version contains `-`).

### Manual / dry-run pack

```bash
dotnet pack src/CSharpDoctor.Cli/CSharpDoctor.Cli.csproj \
  -c Release -o ./artifacts /p:Version=0.1.0-preview.1

# Smoke test install from local feed
mkdir /tmp/csd-smoke && cd /tmp/csd-smoke
dotnet new tool-manifest
dotnet tool install CSharpDoctor.Cli --version 0.1.0-preview.1 \
  --add-source $OLDPWD/artifacts
dotnet csharp-doctor --help
```

## Privacy

No telemetry by default. Optional OTLP: set `CSHARP_DOCTOR_OTLP_ENDPOINT` and `CSHARP_DOCTOR_OTLP_AUTH_HEADER`.
