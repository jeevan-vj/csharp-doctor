# NDepend Gap Analysis

Comparison of NDepend default catalog entries against CSharpDoctor rules, built-in CA/IDE analyzers, and planned implementation tracks.

Source catalogs:
- [NDepend Rules Explorer](https://www.ndepend.com/default-rules/NDepend-Rules-Explorer.html#!)
- [docs/ndepend-rules.md](ndepend-rules.md) (359 entries)
- [docs/rules/README.md](rules/README.md) (95 CSharpDoctor rules/checks)

## Executive Summary

NDepend lists **359** entries. CSharpDoctor implements **95** actionable rules/checks plus **reporting surfaces** (quality gates, hot spots) today. Most remaining NDepend value is in trend metrics, coverage ingestion, churn-aware prioritization, and advanced custom-rule expressions—not more one-off Roslyn IDs.

### Implementation Progress (as of 2026-06)

| Batch | Status | Shipped |
|-------|--------|---------|
| **Batch 1** (P1 Roslyn) | **Done** | `CSD0109`–`CSD0112`, `CSD0302`–`CSD0310`, `CodeMetricsHelper` |
| **Batch 2** (P2 inspectors) | **Done** | `CSDA0001`–`CSDA0011`, `CSDS0001`/`CSDS0002`, `--architecture`, `--sca`, `architecturePolicy` |
| **Batch 3** (P2/P3 Roslyn) | **Done** | `CSD0113`–`CSD0120`, `CSD0118`, `CSD0008` |
| **Batch 4** (P3 reporting) | **Partial** | JSON schema v2, `qualityGates`, `hotSpots`, debt score, baseline `maxNewIssues` |
| **Batch 5** (external/defer) | **Not started** | Naming, Unity, ApiCompat, coverlet gates, full NDepend debt model |

Inspectors: `lint` (default), `wiring`, `ef`, `architecture`, `sca`. See [docs/rules/README.md](rules/README.md) and [docs/configuration.md](configuration.md).

### Status Distribution

| Status | Count | Meaning |
|--------|------:|---------|
| Implemented | 4 | |
| Partial | 8 | |
| Covered by CA/IDE | 33 | |
| Candidate | 46 | |
| Metrics/reporting | 148 | |
| Defer | 76 | |
| Out of scope | 44 | |
| **Total** | **359** | |

### Implementation Track Distribution

| Track | Count |
|-------|------:|
| Reporting/metrics | 148 |
| External/built-in | 125 |
| Roslyn analyzer | 51 |
| Solution inspector | 35 |

### Recommended Implementation Batches

| Batch | Priority | Track | Focus | Status |
|-------|----------|-------|-------|--------|
| **Batch 1** | P1 | Roslyn analyzer | Code size/complexity thresholds, `GC.Collect`, `NotImplementedException`, public constants, readonly/static mutability, culture-aware parsing | Done |
| **Batch 2** | P2 | Solution inspector | Namespace cycles, layer dependencies, dead code reachability, SCA, config-driven architecture rules | Done |
| **Batch 3** | P2 | Roslyn analyzer | Disposable event unsubscribe, immutability regression, P/Invoke, threading pairing, collection pitfalls | Done |
| **Batch 4** | P3 | Reporting/metrics | Quality gates, hot spots, trend metrics, code diff summary, statistics | Partial |
| **Batch 5** | — | External/defer | Naming conventions, Unity, API breaking changes, coverage gates, NDepend debt model | Deferred |

## Classification Legend

| Status | Description |
|--------|-------------|
| **Implemented** | Direct or close CSharpDoctor rule exists |
| **Partial** | CSharpDoctor catches a narrower variant |
| **Covered by CA/IDE** | Better handled by built-in .NET analyzers or editor rules |
| **Candidate** | Worth implementing in CSharpDoctor |
| **Metrics/reporting** | Aggregate gates, trends, stats, or diff—not per-finding analyzer IDs |
| **Defer** | Possible but high-noise, high-cost, or outside current focus |
| **Out of scope** | Unity, pure naming/style, NDepend-specific features we should not clone |

| Track | Description |
|-------|-------------|
| **Roslyn analyzer** | File/symbol diagnostics in `CSharpDoctor.Analyzers` |
| **Solution inspector** | Graph/reachability rules via `WiringInspector`-style inspectors |
| **Reporting/metrics** | Quality gates, hot spots, trends, diff, statistics in CLI/JSON output |
| **External/built-in** | Recommend CA, IDE, StyleCop, coverlet, NuGet audit, or ApiCompat |

## Current CSharpDoctor Coverage

CSharpDoctor surfaces that overlap NDepend categories:

| CSharpDoctor Surface | NDepend Overlap |
|---------------------|-----------------|
| Security (`CSD00xx`) | Security, partial .NET Framework Usage |
| Correctness (`CSD01xx`) | Design (dispose), .NET Framework Usage (threading) |
| Performance (`CSD02xx`) | Code Smells (loops), partial boxing |
| Maintainability (`CSD03xx`) | Visibility (public fields) |
| AspNetCore (`CSD04xx`) | Partial Architecture (UI/DAL/EF) |
| Observability (`CSD05xx`) | No NDepend equivalent |
| Wiring (`CSDWxxxx`, `--wiring`) | Partial Architecture (DI lifetime) |
| EF Migration (`CSDExxxx`, `--ef`) | No NDepend equivalent |
| Architecture (`CSDAxxxx`, `--architecture`) | Architecture, Dead Code, partial Samples of Custom rules |
| SCA (`CSDSxxxx`, `--sca`) | Security (NuGet vulnerability audit) |
| Reporting (`qualityGates`, `hotSpots`, JSON v2) | Partial Quality Gates, partial Hot Spots |

## Category Gap Analysis

### Quality Gates

**16 entries** — Metrics/reporting: 16

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Quality Gates Evolution | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Percentage Coverage | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Percentage Coverage on New Code | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Percentage Coverage on Refactored Code | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Blocker Issues | Partial | `qualityGates.maxErrors` | Reporting/metrics | — | JSON schema v2 + CI exit code |
| Critical Issues | Partial | `qualityGates.maxCriticalIssues` | Reporting/metrics | — | Security-category errors |
| New Blocker / Critical / High Issues | Partial | `qualityGates.maxNewIssues` | Reporting/metrics | — | Baseline JSON compare on `identity` |
| Critical Rules Violated | Partial | `qualityGates.maxIssuesPerRule` | Reporting/metrics | — | Per-rule caps in config |
| Treat Compiler Warnings as Error | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Percentage Debt | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Debt | Partial | `qualityGates.maxDebtScore` | Reporting/metrics | — | Weighted severity debt score |
| New Debt since Baseline | Partial | `qualityGates.maxNewIssues` | Reporting/metrics | — | New diagnostic identities vs baseline JSON |
| Debt Rating per Namespace | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Annual Interest | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| New Annual Interest since Baseline | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |
| Unnecessary NDepend SuppressMessageAttribute Usage | Metrics/reporting | — | Reporting/metrics | P3 | Aggregate gates over findings/debt/coverage |

### Hot Spots

**7 entries** — Metrics/reporting: 7

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Types Hot Spots | Partial | `hotSpots.fixPriority` | Reporting/metrics | — | File/rule ranking; no type graph × churn yet |
| Types to Fix Priority | Partial | `hotSpots.fixPriority` | Reporting/metrics | — | Priority = debt × count |
| Issues to Fix Priority | Implemented | `hotSpots.fixPriority` | Reporting/metrics | — | Top N in JSON + console |
| Debt and Issues per Rule | Implemented | `hotSpots.issuesByRule` | Reporting/metrics | — | JSON schema v2 |
| New Debt and Issues per Rule | Partial | `maxNewIssues` + baseline | Reporting/metrics | — | New identities only; no per-rule delta view |
| Debt and Issues per Code Element | Partial | `hotSpots.issuesByFile` | Reporting/metrics | — | File-level buckets |
| New Debt and Issues per Code Element | Partial | baseline compare | Reporting/metrics | — | Identity-level new issues only |

### Code Smells

**10 entries** — Covered by CA/IDE: 2, Candidate: 6, Defer: 2

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Avoid types too big | Candidate | — | Roslyn analyzer | P1 | Type LOC threshold |
| Avoid types with too many methods | Candidate | — | Roslyn analyzer | P1 | Method count threshold |
| Avoid types with too many fields | Candidate | — | Roslyn analyzer | P1 | Field count threshold |
| Avoid methods too big, too complex | Candidate | — | Roslyn analyzer | P1 | LOC + cyclomatic complexity thresholds |
| Avoid methods with too many parameters | Covered by CA/IDE | CA1501/CA1502 | External/built-in | — |  |
| Avoid methods with too many overloads | Covered by CA/IDE | CA1021 | External/built-in | — |  |
| Avoid methods potentially poorly commented | Defer | — | Roslyn analyzer | P4 | Comment ratio heuristic |
| Avoid types with poor cohesion | Defer | — | Solution inspector | P3 | LCOM metric; noisy |
| Avoid methods with too many local variables | Candidate | — | Roslyn analyzer | P1 | Local variable count |
| Avoid unmaintainable code | Candidate | — | Roslyn analyzer | P1 | Composite maintainability index |

### Code Smells Regression

**9 entries** — Metrics/reporting: 9

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| From now, all types added should respect basic quality principles | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| From now, all types added should be 100% covered by tests | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| From now, all methods added should respect basic quality principles | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| Avoid decreasing code coverage by tests of types | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| Avoid making complex methods even more complex | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| Avoid making large methods even larger | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| Avoid adding methods to a type that already had many methods | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| Avoid adding instance fields to a type that already had many instance fields | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |
| Avoid transforming an immutable type into a mutable one | Metrics/reporting | — | Reporting/metrics | P3 | Baseline regression policy; needs persisted baseline |

### Object Oriented Design

**14 entries** — Partial: 1, Covered by CA/IDE: 8, Candidate: 1, Defer: 4

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Avoid interfaces too big | Covered by CA/IDE | CA1501 | External/built-in | — | Interface size |
| Base class should not use derivatives | Defer | — | Roslyn analyzer | P3 | Downcast to derived in base |
| Class shouldn't be too deep in inheritance tree | Covered by CA/IDE | CA1501 | External/built-in | — | Inheritance depth |
| Class with no descendant should be sealed if possible | Covered by CA/IDE | CA1852 | External/built-in | — |  |
| Overrides of Method() should call base.Method() | Defer | — | Roslyn analyzer | P3 | Requires override analysis |
| Do not hide base class methods | Covered by CA/IDE | CA1065/CS0108 | External/built-in | — | Member hiding |
| A stateless class or structure might be turned into a static type | Covered by CA/IDE | CA1822/CA1052 | External/built-in | — |  |
| Non-static classes should be instantiated or turned to static | Defer | — | Roslyn analyzer | P4 | Usage analysis required |
| Methods should be declared static if possible | Covered by CA/IDE | CA1822 | External/built-in | — | Recommended in docs/rules README |
| Constructor should not call a virtual method | Covered by CA/IDE | CA2214 | External/built-in | — |  |
| Avoid the Singleton pattern | Defer | — | Roslyn analyzer | P4 | Style/opinion |
| Don't assign static fields from instance methods | Candidate | — | Roslyn analyzer | P2 | Static field mutation from instance |
| Avoid empty interfaces | Covered by CA/IDE | CA1040 | External/built-in | — |  |
| Avoid types initialization cycles | Partial | CSDW0008 | Solution inspector | — | DI resolution cycles only |

### Design

**16 entries** — Implemented: 1, Partial: 2, Covered by CA/IDE: 7, Candidate: 2, Defer: 4

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Avoid custom delegates | Defer | — | Roslyn analyzer | P4 | Prefer Action/Func |
| Types with disposable instance fields must be disposable | Partial | CSD0107 | Roslyn analyzer | — | Class Dispose omits fields |
| Disposable types with unmanaged resources should declare finalizer | Covered by CA/IDE | CA1063 | External/built-in | — |  |
| Methods that create disposable object(s) and that don't call Dispose() | Implemented | CSD0102 | Roslyn analyzer | — | Local disposable without using |
| Disposable Types Must Unsubscribe Events | Candidate | — | Roslyn analyzer | P2 | Event handler leak pattern |
| Classes that are candidate to be turned into structures | Defer | — | Roslyn analyzer | P4 | Class vs struct heuristic |
| Avoid namespaces with few types | Defer | — | Solution inspector | P4 | Namespace organization |
| Nested types should not be visible | Covered by CA/IDE | CA1034 | External/built-in | — |  |
| Declare types in namespaces | Covered by CA/IDE | IDE0130 | External/built-in | — |  |
| Empty static constructor can be discarded | Covered by CA/IDE | IDE0059 | External/built-in | — |  |
| Instances size shouldn't be too big | Defer | — | Roslyn analyzer | P4 | Instance field size |
| Attribute classes should be sealed | Covered by CA/IDE | CA1813 | External/built-in | — |  |
| Don't use obsolete types, methods or fields | Covered by CA/IDE | CS0618/CA1041 | External/built-in | — | Compiler + CA1041 |
| Do implement methods that throw NotImplementedException | Candidate | — | Roslyn analyzer | P1 | Abstract/interface stubs |
| Override equals and operator equals on value types | Covered by CA/IDE | CA1815 | External/built-in | — |  |
| Boxing/unboxing should be avoided | Partial | CSD0203 | Roslyn analyzer | — | Logging boxing only |

### Architecture

**13 entries** — Partial: 1, Candidate: 6, Defer: 6

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Avoid namespaces mutually dependent | Candidate | — | Solution inspector | P2 | Mutual namespace dependency |
| Avoid namespaces dependency cycles | Candidate | — | Solution inspector | P2 | Namespace graph analysis |
| Avoid partitioning the code base through many small library Assemblies | Defer | — | Solution inspector | P3 | Cohesion/coupling metric |
| Enforcing Clean Architecture | Defer | — | Solution inspector | P3 | Opinionated preset; config-driven later |
| UI layer shouldn't use directly DB types | Partial | CSD0402, CSD0407 | Roslyn analyzer | — | ASP.NET/EF patterns, not layer graph |
| UI layer shouldn't use directly DAL layer | Partial | CSDA0010/0011 | Solution inspector | P2 | Config-driven namespace/assembly policy (not opinionated preset) |
| Assemblies with poor cohesion (RelationalCohesion) | Defer | — | Solution inspector | P3 | Cohesion/coupling metric |
| Namespaces with poor cohesion (RelationalCohesion) | Defer | — | Solution inspector | P3 | Cohesion/coupling metric |
| Assemblies that don't satisfy the Abstractness/Instability principle | Defer | — | Solution inspector | P3 | Cohesion/coupling metric |
| Avoid excessive class coupling | Candidate | — | Solution inspector | P3 | Coupling metrics |
| Higher cohesion - lower coupling | Defer | — | Solution inspector | P3 | Cohesion/coupling metric |
| Avoid mutually-dependent types | Candidate | — | Solution inspector | P2 | Type-level coupling cycles |
| Example of custom rule to check for dependency | Implemented | CSDA0010/0011 | Solution inspector | — | `architecturePolicy` in `.csharp-doctor.json` |

### API Breaking Changes

**9 entries** — Defer: 9

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| API Breaking Changes: Types | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| API Breaking Changes: Methods | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| API Breaking Changes: Fields | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| API Breaking Changes: Interfaces and Abstract Classes | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| Broken serializable types | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| Avoid changing enumerations Flags status | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| API: New publicly visible types | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| API: New publicly visible methods | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |
| API: New publicly visible fields | Defer | ApiCompat | External/built-in | P4 | Binary/API diff between builds |

### Code Coverage

**14 entries** — Defer: 14

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Code should be tested | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| New Types and Methods should be tested | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Methods refactored should be tested | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Assemblies and Namespaces should be tested | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Types almost 100% tested should be 100% tested | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Namespaces almost 100% tested should be 100% tested | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Types that used to be 100% covered by tests should still be 100% covered | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Types tagged with FullCoveredAttribute should be 100% covered | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Types 100% covered should be tagged with FullCoveredAttribute | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Methods should have a low C.R.A.P score | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Complex Methods should be 100% tested | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Test Methods | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Methods directly called by test Methods | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |
| Methods directly and indirectly called by test Methods | Defer | coverlet | External/built-in | P4 | Requires coverage ingestion; not syntax-only |

### Dead Code

**4 entries** — Candidate: 3, Defer: 1

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Potentially Dead Types | Candidate | — | Solution inspector | P2 | Reachability from entry points |
| Potentially Dead Methods | Candidate | — | Solution inspector | P2 | Reachability from entry points |
| Potentially Dead Fields | Candidate | — | Solution inspector | P2 | Unused field analysis |
| Wrong usage of IsNotDeadCodeAttribute | Defer | — | Solution inspector | P4 | NDepend-specific attribute |

### Security

**11 entries** — Implemented: 3, Partial: 2, Covered by CA/IDE: 1, Candidate: 3, Defer: 2

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Software Composition Analysis (SCA) | Implemented | CSDS0001 | Solution inspector | — | `dotnet list package --vulnerable` (`--sca`) |
| Transitive Software Composition Analysis (SCA) | Implemented | CSDS0002 | Solution inspector | — | `--include-transitive` via `--sca` |
| Don't use CoSetProxyBlanket and CoInitializeSecurity | Defer | — | Roslyn analyzer | P4 | COM interop niche |
| Don't use System.Random for security purposes | Implemented | CSD0005 | Roslyn analyzer | — | Security-context heuristic |
| Don't use DES/3DES weak cipher algorithms | Implemented | CSD0002 | Roslyn analyzer | — | MD5/SHA1/DES family |
| Don't disable certificate validation | Implemented | CSD0011 | Roslyn analyzer | — | TLS cert bypass |
| Review publicly visible event handlers | Defer | — | Roslyn analyzer | P4 | Manual review style; noisy |
| Pointers should not be publicly visible | Candidate | — | Roslyn analyzer | P3 | Unsafe pointer visibility |
| Seal methods that satisfy non-public interfaces | Covered by CA/IDE | CA1811 | External/built-in | — | Explicit interface implementation |
| Review commands vulnerable to SQL injection | Partial | CSD0001, CSD0007 | Roslyn analyzer | — | Targets concat/interpolation, not all SQL patterns |
| Review data adapters vulnerable to SQL injection | Partial | CSD0007 | Roslyn analyzer | — | Raw SQL interpolation |

### Visibility

**12 entries** — Partial: 1, Candidate: 1, Defer: 10

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Methods that could have a lower visibility | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Types that could have a lower visibility | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Fields that could have a lower visibility | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Types that could be declared as private, nested in a parent type | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Avoid publicly visible constant fields | Candidate | — | Roslyn analyzer | P1 | Public const exposure |
| Fields should be declared as private or protected | Partial | CSD0301 | Roslyn analyzer | — | Public instance fields only |
| Constructors of abstract classes should be declared as protected or private | Defer | CA1812 | External/built-in | P4 | Visibility analysis |
| Avoid public methods not publicly visible | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Event handler methods should be declared as private or protected | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Wrong usage of CannotDecreaseVisibilityAttribute | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Exception classes should be declared as public | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |
| Methods that should be declared as 'public' in C#, 'Public' in VB.NET | Defer | CA1812/IDE0051 | External/built-in | P4 | Visibility tightening; noisy |

### Immutability

**15 entries** — Covered by CA/IDE: 2, Candidate: 2, Defer: 11

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Fields should be marked as ReadOnly when possible | Candidate | — | Roslyn analyzer | P1 | readonly field opportunities |
| Avoid non-readonly static fields | Candidate | — | Roslyn analyzer | P1 | Mutable static state |
| Avoid static fields with a mutable field type | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Structures should be immutable | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Immutable struct should be declared as readonly | Covered by CA/IDE | CA1815 | External/built-in | — |  |
| Property Getters should be pure | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| A field must not be assigned from outside its parent hierarchy types | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Don't assign a field from many methods | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Do not declare read only fields with mutable reference types | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Public read only array fields can be modified | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Types tagged with ImmutableAttribute must be immutable | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Types immutable should be tagged with ImmutableAttribute | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Methods tagged with PureAttribute must be pure | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Pure methods should be tagged with PureAttribute | Defer | CA1819/CA2104 | External/built-in | P4 | Immutability/purity analysis |
| Record should be immutable | Covered by CA/IDE | CA1069 | External/built-in | — | Record immutability |

### Naming Conventions

**23 entries** — Out of scope: 23

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Instance fields naming convention | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Static fields naming convention | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Interface name should begin with a 'I' | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Abstract base class should be suffixed with 'Base' | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Exception class name should be suffixed with 'Exception' | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Attribute class name should be suffixed with 'Attribute' | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Types name should begin with an Upper character | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Methods name should begin with an Upper character | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Do not name enum values 'Reserved' | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid types with name too long | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid methods with name too long | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid fields with name too long | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid having different types with same name | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid prefixing type name with parent namespace name | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid naming types and namespaces with the same identifier | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Don't call your method Dispose | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Methods prefixed with 'Try' should return a boolean | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Properties and fields that represent a collection of items should be named Items. | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| DDD ubiquitous language check | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid fields with same name in class hierarchy | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Avoid various capitalizations for method name | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Controller class name should be suffixed with 'Controller' | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |
| Nested class members should not mask outer class' static members | Out of scope | StyleCop/IDE | External/built-in | — | Style policy; use .editorconfig |

### Source Files Organization

**6 entries** — Covered by CA/IDE: 1, Candidate: 2, Defer: 3

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Avoid referencing source file out of the project directory | Candidate | — | Roslyn analyzer | P2 | Compile item path check |
| Avoid duplicating a type definition across assemblies | Candidate | — | Solution inspector | P3 | Duplicate type detection |
| Avoid defining multiple types in a source file | Defer | — | Roslyn analyzer | P4 | One type per file style |
| Namespace name should correspond to file location | Covered by CA/IDE | IDE0130 | External/built-in | — |  |
| Types with source files stored in the same directory, should be declared in the same namespace | Defer | — | Roslyn analyzer | P4 | Directory/namespace alignment |
| Types declared in the same namespace, should have their source files stored in the same directory | Defer | — | Roslyn analyzer | P4 | Directory/namespace alignment |

### .NET Framework Usage

**32 entries** — Partial: 1, Covered by CA/IDE: 12, Candidate: 10, Defer: 6, Out of scope: 3

#### System

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Mark ISerializable types with SerializableAttribute | Covered by CA/IDE | CA2237 | External/built-in | — |  |
| Mark assemblies with CLSCompliant (deprecated) | Out of scope | — | External/built-in | — | Deprecated attribute |
| Mark assemblies with ComVisible (deprecated) | Out of scope | — | External/built-in | — | Deprecated attribute |
| Mark attributes with AttributeUsageAttribute | Covered by CA/IDE | CA1018 | External/built-in | — |  |
| Remove calls to GC.Collect() | Candidate | — | Roslyn analyzer | P1 | Explicit GC.Collect |
| Don't call GC.Collect() without calling GC.WaitForPendingFinalizers() | Candidate | — | Roslyn analyzer | P1 | GC.Collect pairing |
| Enum Storage should be Int32 | Covered by CA/IDE | CA1008 | External/built-in | — |  |
| Do not raise too general exception types | Covered by CA/IDE | CA2201 | External/built-in | — |  |
| Do not raise reserved exception types | Covered by CA/IDE | CA2201 | External/built-in | — |  |
| Uri fields or properties should be of type System.Uri | Covered by CA/IDE | CA1056 | External/built-in | — |  |
| Types should not derive from System.ApplicationException | Covered by CA/IDE | CA1064 | External/built-in | — |  |
| Don't Implement ICloneable | Covered by CA/IDE | CA1048 | External/built-in | — |  |

#### System.Collections

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Collection properties should be read only | Covered by CA/IDE | CA2227 | External/built-in | — |  |
| Don't use .NET 1.x HashTable and ArrayList | Candidate | — | Roslyn analyzer | P2 | Legacy collection types |
| Caution with List.Contains() | Defer | — | Roslyn analyzer | P4 | O(n) Contains on List |
| Prefer return collection abstraction instead of implementation | Covered by CA/IDE | CA1002 | External/built-in | — |  |

#### System.Runtime.InteropServices

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| P/Invokes should be static and not be publicly visible | Candidate | — | Roslyn analyzer | P2 | P/Invoke visibility |
| Move P/Invokes to NativeMethods class | Candidate | — | Roslyn analyzer | P2 | P/Invoke organization |
| NativeMethods class should be static and internal | Candidate | — | Roslyn analyzer | P2 | NativeMethods pattern |

#### System.Threading

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Don't create threads explicitly | Covered by CA/IDE | CA2008 | External/built-in | — |  |
| Don't use dangerous threading methods | Partial | CSD0108 | Roslyn analyzer | — | lock(this) only |
| Monitor TryEnter/Exit must be both called within the same method | Candidate | — | Roslyn analyzer | P2 | Monitor lock pairing |
| ReaderWriterLock AcquireLock/ReleaseLock must be both called within the same method | Candidate | — | Roslyn analyzer | P3 | RW lock pairing |
| Don't tag instance fields with ThreadStaticAttribute | Covered by CA/IDE | CA1401 | External/built-in | — |  |
| Method non-synchronized that read mutable states | Candidate | — | Roslyn analyzer | P2 | Thread safety heuristic |

#### System.Xml

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Method should not return concrete XmlNode | Defer | — | Roslyn analyzer | P4 | XmlNode return type |
| Types should not derive from System.Xml.XmlDocument | Defer | — | Roslyn analyzer | P4 | XmlDocument subclass |

#### System.Globalization

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Float and Date Parsing must be culture aware | Candidate | — | Roslyn analyzer | P1 | Culture-invariant parsing |

#### System.Reflection

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Mark assemblies with assembly version | Defer | — | Solution inspector | P4 | Assembly version consistency |
| Assemblies should have the same version | Defer | — | Solution inspector | P4 | Assembly version consistency |
| Assemblies Referenced in Multiple Versions | Defer | — | Solution inspector | P4 | Assembly version consistency |

#### Microsoft.Contracts

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Public methods returning a reference needs a contract to ensure that a non-null reference is returned | Out of scope | — | External/built-in | — | Code Contracts deprecated |

### API Usage

**9 entries** — Out of scope: 9

#### Unity

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Classes tagged with InitializeOnLoad should have a static constructor | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| Avoid using non-generic GetComponent | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| Avoid empty Unity message | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| Avoid using Time.fixedDeltaTime with Update | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| Use CreateInstance to create a scriptable object | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| The SerializeField attribute is redundant on public fields | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| InitializeOnLoadMethod should tag only static and parameterless methods | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| Prefer using SetPixels32() over SetPixels() | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |
| Don't use System.Reflection in performance critical messages | Out of scope | — | External/built-in | — | Unity-specific; not general .NET scope |

### Defining JustMyCode

**7 entries** — Out of scope: 7

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Discard generated Assemblies from JustMyCode | Out of scope | — | External/built-in | — | NDepend debt partitioning; CSharpDoctor uses ignores |
| Discard generated Namespaces from JustMyCode | Out of scope | — | External/built-in | — | NDepend debt partitioning; CSharpDoctor uses ignores |
| Discard generated Types from JustMyCode | Out of scope | — | External/built-in | — | NDepend debt partitioning; CSharpDoctor uses ignores |
| Discard generated and designer Methods from JustMyCode | Out of scope | — | External/built-in | — | NDepend debt partitioning; CSharpDoctor uses ignores |
| Discard generated Fields from JustMyCode | Out of scope | — | External/built-in | — | NDepend debt partitioning; CSharpDoctor uses ignores |
| JustMyCode code elements | Out of scope | — | External/built-in | — | NDepend debt partitioning; CSharpDoctor uses ignores |
| NotMyCode code elements | Out of scope | — | External/built-in | — | NDepend debt partitioning; CSharpDoctor uses ignores |

### Trend Metrics

**78 entries** — Metrics/reporting: 78

#### Issues

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| # New Issues since Baseline | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues Fixed since Baseline | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues Worsened since Baseline | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues with severity Blocker | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues with severity Critical | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues with severity High | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues with severity Medium | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues with severity Low | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Blocker/Critical/High Issues | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Issues | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Suppressed Issues | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

#### Rules

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| # Rules | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Rules Violated | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Critical Rules Violated | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

#### Quality Gates

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| # Quality Gates | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Quality Gates Warn | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Quality Gates Fail | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

#### Debt

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Percentage Debt (Metric) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Debt (Metric) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| New Debt since Baseline (Metric) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Annual Interest (Metric) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| New Annual Interest since Baseline (Metric) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Breaking Point | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Breaking Point of Blocker / Critical / High Issues | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

#### Code Size

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| # Lines of Code | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code (JustMyCode) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code (NotMyCode) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code Added since the Baseline | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Source Files | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Line Feed | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # IL Instructions | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # IL Instructions (NotMyCode) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Comments | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Percentage of Comments | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Assemblies | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Namespaces | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Types | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Public Types | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Classes | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Abstract Classes | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Interfaces | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Structures | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Abstract Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Concrete Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Fields | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

#### Maximum and Average

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Max # Lines of Code for Methods (JustMyCode) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average # Lines of Code for Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average # Lines of Code for Methods with at least 3 Lines of Code | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Max # Lines of Code for Types (JustMyCode) | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average # Lines of Code for Types | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Max Cyclomatic Complexity for Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average Cyclomatic Complexity for Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Max IL Cyclomatic Complexity for Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average IL Cyclomatic Complexity for Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Max IL Nesting Depth for Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average IL Nesting Depth for Methods | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Max # of Methods for Types | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average # Methods for Types | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Max # of Methods for Interfaces | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average # Methods for Interfaces | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

#### Coverage

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Percentage Code Coverage | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code Covered | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code Not Covered | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code Uncoverable | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code in Types 100% Covered | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of Code in Methods 100% Covered | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Max C.R.A.P Score | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Average C.R.A.P Score | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| Percentage JustMyCode Coverage | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of JustMyCode Covered | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Lines of JustMyCode Not Covered | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

#### Third-Party Usage

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| # Third-Party Assemblies Used | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Third-Party Namespaces Used | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Third-Party Types Used | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Third-Party Methods Used | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Third-Party Fields Used | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |
| # Third-Party Code Elements Used | Metrics/reporting | — | Reporting/metrics | P3 | Historical metrics dashboard |

### Code Diff Summary

**25 entries** — Metrics/reporting: 25

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| New assemblies | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Assemblies removed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Assemblies where code was changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| New namespaces | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Namespaces removed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Namespaces where code was changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| New types | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Types removed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Types where code was changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Heuristic to find types moved from one namespace or assembly to another | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Types directly using one or several types changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Types indirectly using one or several types changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| New methods | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Methods removed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Methods where code was changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Methods directly calling one or several methods changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Methods indirectly calling one or several methods changed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| New fields | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Fields removed | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Third party types that were not used and that are now used | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Third party types that were used and that are not used anymore | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Third party methods that were not used and that are now used | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Third party methods that were used and that are not used anymore | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Third party fields that were not used and that are now used | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |
| Third party fields that were used and that are not used anymore | Metrics/reporting | — | Reporting/metrics | P3 | Structural diff reporting |

### Statistics

**13 entries** — Metrics/reporting: 13

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Most used types (Rank) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Most used methods (Rank) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Most used assemblies (#AssembliesUsingMe) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Most used namespaces (#NamespacesUsingMe ) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Most used types (#TypesUsingMe ) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Most used methods (#MethodsCallingMe ) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Namespaces that use many other namespaces (#NamespacesUsed ) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Types that use many other types (#TypesUsed ) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| Methods that use many other methods (#MethodsCalled ) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| High-level to low-level assemblies (Level) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| High-level to low-level namespaces (Level) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| High-level to low-level types (Level) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |
| High-level to low-level methods (Level) | Metrics/reporting | — | Reporting/metrics | P3 | Graph analytics rankings |

### Samples of Custom rules

**16 entries** — Candidate: 10, Defer: 4, Out of scope: 2

#### Check Architecture

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Check that the assembly Asm1 is not using the assembly Asm2 | Implemented | CSDA0011 | Solution inspector | — | `architecturePolicy.forbiddenAssemblyReferences` |
| Check that the namespace N1.N2 is not using the namespace N3.N4.N5 | Implemented | CSDA0010 | Solution inspector | — | `architecturePolicy.forbiddenNamespaceDependencies` |
| Check that the assembly Asm1 is only using the assemblies Asm2, Asm3 or System.Runtime | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |
| Check that the namespace N1.N2 is only using the namespaces N3.N4, N5 or System | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |
| Check that AsmABC is the only assembly that is using System.Collections.Concurrent | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |
| Check that only 3 assemblies are using System.Collections.Concurrent | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |
| Check that all methods that call Foo.Fct1() also call Foo.Fct2(Int32) | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |
| Check that all types that derive from Foo, also implement IFoo | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |
| Check that all types that has the attribute FooAttribute are declared in the namespace N1.N2\* | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |
| Check that all synchronization objects are only used from the namespaces under MyNamespace.Sync | Candidate | — | Solution inspector | P2 | Config-driven custom dependency rules |

#### Check Coverage on particular Code Elements

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Check that the namespace N1.N2 is 100% covered by tests | Defer | coverlet | External/built-in | P4 | Coverage gates on specific elements |
| Check that the assembly Asm is 100% covered by tests | Defer | coverlet | External/built-in | P4 | Coverage gates on specific elements |
| Check that the class Namespace.Foo is 100% covered by tests | Defer | coverlet | External/built-in | P4 | Coverage gates on specific elements |
| Check that the method Namespace.Foo.Method(Int32) is 100% covered by tests | Defer | coverlet | External/built-in | P4 | Coverage gates on specific elements |

#### Custom Naming Conventions

| NDepend Entry | Status | Equivalent | Track | Priority | Notes |
|---------------|--------|------------|-------|----------|-------|
| Check that all types that derive from Foo, has a name that ends up with Foo | Out of scope | StyleCop/IDE | External/built-in | — | Naming policy via editorconfig |
| Check that all namespaces begins with CompanyName.ProductName | Out of scope | StyleCop/IDE | External/built-in | — | Naming policy via editorconfig |

## Implementation Batch Detail

### Batch 1 (P1) — Done

Low-cost Roslyn rules with high signal. **13 candidate entries — all shipped.**

| Entry | Category | Track |
|-------|----------|-------|
| Don't call GC.Collect() without calling GC.WaitForPendingFinalizers() | .NET Framework Usage | Roslyn analyzer |
| Float and Date Parsing must be culture aware | .NET Framework Usage | Roslyn analyzer |
| Remove calls to GC.Collect() | .NET Framework Usage | Roslyn analyzer |
| Avoid methods too big, too complex | Code Smells | Roslyn analyzer |
| Avoid methods with too many local variables | Code Smells | Roslyn analyzer |
| Avoid types too big | Code Smells | Roslyn analyzer |
| Avoid types with too many fields | Code Smells | Roslyn analyzer |
| Avoid types with too many methods | Code Smells | Roslyn analyzer |
| Avoid unmaintainable code | Code Smells | Roslyn analyzer |
| Do implement methods that throw NotImplementedException | Design | Roslyn analyzer |
| Avoid non-readonly static fields | Immutability | Roslyn analyzer |
| Fields should be marked as ReadOnly when possible | Immutability | Roslyn analyzer |
| Avoid publicly visible constant fields | Visibility | Roslyn analyzer |

### Batch 2 (P2) — Done (core); advanced custom samples deferred

Solution inspectors and medium-cost Roslyn rules. **29 candidate entries — 22 shipped; 7 advanced custom-rule samples remain.**

| Entry | Category | Track |
|-------|----------|-------|
| Don't use .NET 1.x HashTable and ArrayList | .NET Framework Usage | Roslyn analyzer |
| Method non-synchronized that read mutable states | .NET Framework Usage | Roslyn analyzer |
| Monitor TryEnter/Exit must be both called within the same method | .NET Framework Usage | Roslyn analyzer |
| Move P/Invokes to NativeMethods class | .NET Framework Usage | Roslyn analyzer |
| NativeMethods class should be static and internal | .NET Framework Usage | Roslyn analyzer |
| P/Invokes should be static and not be publicly visible | .NET Framework Usage | Roslyn analyzer |
| Avoid mutually-dependent types | Architecture | Solution inspector |
| Avoid namespaces dependency cycles | Architecture | Solution inspector |
| Avoid namespaces mutually dependent | Architecture | Solution inspector |
| Example of custom rule to check for dependency | Architecture | Solution inspector |
| UI layer shouldn't use directly DAL layer | Architecture | Solution inspector |
| Potentially Dead Fields | Dead Code | Solution inspector |
| Potentially Dead Methods | Dead Code | Solution inspector |
| Potentially Dead Types | Dead Code | Solution inspector |
| Disposable Types Must Unsubscribe Events | Design | Roslyn analyzer |
| Don't assign static fields from instance methods | Object Oriented Design | Roslyn analyzer |
| Check that AsmABC is the only assembly that is using System.Collections.Concurrent | Samples of Custom rules | Solution inspector |
| Check that all methods that call Foo.Fct1() also call Foo.Fct2(Int32) | Samples of Custom rules | Solution inspector |
| Check that all synchronization objects are only used from the namespaces under MyNamespace.Sync | Samples of Custom rules | Solution inspector |
| Check that all types that derive from Foo, also implement IFoo | Samples of Custom rules | Solution inspector |
| Check that all types that has the attribute FooAttribute are declared in the namespace N1.N2\* | Samples of Custom rules | Solution inspector |
| Check that only 3 assemblies are using System.Collections.Concurrent | Samples of Custom rules | Solution inspector |
| Check that the assembly Asm1 is not using the assembly Asm2 | Samples of Custom rules | Solution inspector |
| Check that the assembly Asm1 is only using the assemblies Asm2, Asm3 or System.Runtime | Samples of Custom rules | Solution inspector |
| Check that the namespace N1.N2 is not using the namespace N3.N4.N5 | Samples of Custom rules | Solution inspector |
| Check that the namespace N1.N2 is only using the namespaces N3.N4, N5 or System | Samples of Custom rules | Solution inspector |
| Software Composition Analysis (SCA) | Security | Solution inspector |
| Transitive Software Composition Analysis (SCA) | Security | Solution inspector |
| Avoid referencing source file out of the project directory | Source Files Organization | Roslyn analyzer |

### Batch 3 (P3 Roslyn + graph) — Done

**4 candidate entries — all shipped:**

| Entry | Rule |
|-------|------|
| ReaderWriterLock pairing | `CSD0118` |
| Pointers publicly visible | `CSD0008` |
| Excessive class coupling | `CSDA0004` |
| Duplicate type across assemblies | `CSDA0005` |

### Batch 4 (P3) — Partial

Reporting/metrics. **23 NDepend entries in scope; 8 partially covered, 15 still open.**

**Shipped**

| Surface | Config / output | NDepend overlap |
|---------|-----------------|-----------------|
| Aggregate gates | `qualityGates.*` | Blocker/critical/debt/new-issue gates |
| Hot-spot ranking | `hotSpots.*`, JSON `fixPriority` | Issues/rule priority, debt per rule/file |
| Debt score | `debtWeights`, JSON `metrics.debtScore` | Simplified Debt gate |
| Baseline compare | `baselinePath`, `maxNewIssues` | New issues since baseline |

**Still open (recommended order)**

| Priority | Item | Track | Notes |
|----------|------|-------|-------|
| **P3a** | Baseline persistence | Reporting | `csharp-doctor --save-baseline`; auto-update baseline in CI |
| **P3b** | Per-rule / per-file delta views | Reporting | `newDebtByRule`, `newIssuesByFile` vs saved baseline |
| **P3c** | Type-level hot spots | Solution inspector + reporting | Rank types via architecture graph + issue density |
| **P3d** | Churn × debt priority | Reporting | Git history multiplier on `fixPriority` (NDepend-style) |
| **P3e** | Code diff summary stats | Reporting | Counts/metrics for `--diff` / `--staged` runs |
| **P3f** | Statistics block in JSON | Reporting | LOC, type/method counts, `# comments` aggregates |
| **P3g** | Quality gate evolution | Reporting | Store gate pass/fail history; trend chart data in JSON |
| **P3h** | Coverage gates | External | Ingest coverlet/OpenCover; `maxCoverage`, `minCoverageOnNewCode` |
| **P3i** | Namespace debt rating | Reporting | Debt score grouped by namespace (not just category) |
| **P3j** | Gate evolution dashboard | Defer | UI beyond CLI/JSON; defer unless API consumers need it |

## Deliberately Not Implementing

These NDepend categories should remain external or out of scope for CSharpDoctor:

- **.NET Framework Usage** (21 entries): 3 out of scope, 6 defer, 12 covered by CA/IDE
- **API Breaking Changes** (9 entries): 9 defer
- **API Usage** (9 entries): 9 out of scope
- **Architecture** (6 entries): 6 defer
- **Code Coverage** (14 entries): 14 defer
- **Code Diff Summary** (25 entries): planned in Batch 4c (`--diff` / `--staged` JSON summary); full NDepend parity deferred
- **Code Smells** (4 entries): 2 defer, 2 covered by CA/IDE
- **Code Smells Regression** (9 entries): 9 metrics/reporting
- **Dead Code** (1 entries): 1 defer
- **Defining JustMyCode** (7 entries): 7 out of scope
- **Design** (11 entries): 4 defer, 7 covered by CA/IDE
- **Hot Spots** (7 entries): 3 partial (JSON/console), 4 metrics/reporting still open (churn, per-element deltas)
- **Immutability** (13 entries): 11 defer, 2 covered by CA/IDE
- **Naming Conventions** (23 entries): 23 out of scope
- **Object Oriented Design** (12 entries): 4 defer, 8 covered by CA/IDE
- **Quality Gates** (16 entries): 6 partial (`qualityGates` config), 10 still open (coverage, compiler warnings, full debt model)
- **Samples of Custom rules** (10 entries): 2 implemented (`CSDA0010`/`CSDA0011`), 2 out of scope, 6 defer (whitelist, call-sequence, attribute rules)
- **Security** (3 entries): 2 defer, 1 covered by CA/IDE
- **Source Files Organization** (4 entries): 3 defer, 1 covered by CA/IDE
- **Statistics** (13 entries): planned in Batch 4c (lightweight JSON block); full NDepend stats deferred
- **Trend Metrics** (78 entries): start with gate history (Batch 4g); full 78-metric time series deferred
- **Visibility** (10 entries): 10 defer

## Next Steps

Batches 1–3 and the core of Batch 2 are complete. Work should focus on **finishing Batch 4** and **expanding architecture policy**, not new Roslyn IDs unless a high-signal gap appears.

### Sprint 1 — Batch 4a: Baselines and deltas (highest ROI)

1. Add `--save-baseline <path>` to persist JSON report (or metrics-only slice) after a green run.
2. Extend JSON with `deltas`: `newIssuesByRule`, `newIssuesByFile`, `newDebtScore` vs `baselinePath`.
3. Document CI recipe: save baseline on `main`, gate PRs with `maxNewIssues` + per-rule caps.
4. Tests: baseline round-trip, delta math, gate failure on new identity only.

### Sprint 2 — Batch 4b: Type-level hot spots

1. Reuse `TypeDependencyGraph` / `SolutionReferenceIndex` to bucket diagnostics by containing type.
2. Add `hotSpots.issuesByType` and merge into `fixPriority` (debt × reference count heuristic).
3. Optional `--architecture` coupling: flag types with high coupling **and** high issue density.

### Sprint 3 — Batch 4c: Diff and statistics

1. When `InspectMode` is `Diff` or `Staged`, emit `diffSummary` in JSON (files touched, issues in scope, new vs baseline).
2. Add lightweight `statistics` block: project count, source file count, approximate LOC (from syntax trees or file read).
3. Surface diff summary in console for `--diff` / `--staged` runs.

### Sprint 4 — Architecture policy v2 (remaining Batch 2 samples)

1. `allowedAssemblyReferences` / `allowedNamespaceDependencies` (whitelist, not just forbid).
2. `requiredInterface` rule: types deriving from `X` must implement `IX` (NDepend sample).
3. `callSequence` rule: methods calling `Foo.Fct1()` must also call `Foo.Fct2(int)` — lower priority, needs CFG.
4. Ship a **Clean Architecture preset** JSON snippet (UI → Contracts → Data layer names).

### Explicitly deferred (Batch 5 — do not schedule yet)

- **Naming conventions** (23 entries) → StyleCop / `.editorconfig`
- **Unity rules** (9 entries) → out of scope
- **API breaking changes** → Microsoft.ApiCompat / package validation
- **Coverage gates** → coverlet + optional `coverlet.msbuild` ingestion (Sprint 1 prerequisite for P3h)
- **Trend metrics** (78 entries) → needs persisted time-series store; start with gate history only
- **Full NDepend debt/interest model** → keep simplified `debtWeights` unless product demands parity

### Ongoing hygiene

1. Keep [docs/rules/README.md](rules/README.md) and this doc in sync when rules ship.
2. Add per-rule `docs/rules/CSD####.md` pages for new IDs (backlog: ~95 pages).
3. Refresh executive status counts when category tables are reconciled against the 359-entry catalog.
