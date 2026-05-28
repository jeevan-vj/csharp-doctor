# CSharp Doctor rules

CSharp Doctor complements built-in **CA** and **IDE** analyzers. It focuses on high-signal patterns that are easy to miss in CLI scans or are not enabled by default in every repo.

## Rule index

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

ASP.NET rules (`CSD04xx`) run only when the containing project references ASP.NET Core (detected from `.csproj`).

## CA / IDE overlap matrix

| Built-in rule | Relationship |
|---------------|--------------|
| CA2007 (`ConfigureAwait`) | **Not duplicated** — enable CA2007 in `.editorconfig` if needed |
| CA1031 (catch general exception) | **Complement** — CSD0006 targets *empty* catch only |
| CA1062 (validate arguments) | **Not duplicated** |
| CA1822 (mark members static) | **Not duplicated** |
| CA1848 (LoggerMessage) | **Complement** — CSD0203 targets boxing in logging calls |
| CA2000 (dispose objects) | **Complement** — CSD0102 targets local disposable without `using` |
| IDE0059 (unnecessary assignment) | **Not duplicated** |
| CA5394 (`Random` for security) | **Complement** — CSD0005 uses security-context heuristic |
| CA2100 (SQL review) | **Complement** — CSD0007 targets EF/raw SQL interpolation |
| CA3075 (XML DTD) | **Complement** — CSD0009 targets unsafe DTD settings |
| CA2326–2329 (deserialization) | **Complement** — CSD0010 targets Newtonsoft `TypeNameHandling` |
| CA5359 (cert validation) | **Complement** — CSD0011 targets always-true callback |
| CA5386 (TLS) | **Complement** — CSD0012 targets weak `SecurityProtocol` |
| CA5404 (JWT metadata) | **Complement** — CSD0013 targets disabled issuer/HTTPS checks |

## Recommended CA enablement preset

CSharp Doctor intentionally does **not** duplicate many high-value built-in rules. Add this to `.editorconfig` (or `Directory.Build.props` analyzer severities) alongside `csharp-doctor` scans:

```ini
# Async / reliability
dotnet_diagnostic.CA2007.severity = warning   # ConfigureAwait

# Defensive coding
dotnet_diagnostic.CA1062.severity = suggestion # Validate arguments (public APIs)
dotnet_diagnostic.CA1031.severity = suggestion # Do not catch general Exception

# Performance / allocations
dotnet_diagnostic.CA1822.severity = suggestion # Mark members static
dotnet_diagnostic.CA1848.severity = suggestion # LoggerMessage source generators

# Security (pair with CSD rules)
dotnet_diagnostic.CA2100.severity = warning    # Review SQL queries
dotnet_diagnostic.CA3075.severity = warning    # Insecure DTD processing
dotnet_diagnostic.CA5359.severity = error      # Cert validation disabled
dotnet_diagnostic.CA5386.severity = warning    # Avoid hardcoded TLS versions
dotnet_diagnostic.CA5394.severity = warning    # Random for security
dotnet_diagnostic.CA2326.severity = error      # Unsafe deserialization
```

Tune severities per team; `suggestion` keeps local feedback without failing CI.

## Tags and surfaces

| Tag | Meaning |
|-----|---------|
| `security` | Security-sensitive finding |
| `correctness` | Correctness / reliability |
| `performance` | Performance hotspot |
| `design` | API design (excluded from CI by default) |
| `aspnet` | Requires ASP.NET Core project capability |
| `maintainability` | Maintainability / style |

Exclude noisy tags in CI:

```json
{
  "ignore": { "tags": ["design"] },
  "surfaces": { "ciFailure": { "excludeTags": ["design"] } }
}
```
