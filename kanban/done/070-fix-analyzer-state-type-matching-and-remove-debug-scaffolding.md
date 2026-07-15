# Fix analyzer State type matching and remove debug scaffolding

## Description

Code review 2026-06-11, findings 14 and 26 (`code-review-2026-06-11.md`).

**Simple-name type matching (finding 14):** all three analyzers in `source/timewarp-state-analyzer/` identify TimeWarp state types by the base type's *simple* name:

- `state-implementation-analyzer.cs:44` — `symbol.BaseType.Name == "State" && symbol.BaseType.TypeArguments.Length == 1`
- `state-inheritance-analyzer.cs:39` — `!baseTypeSymbol.Name.Equals("State")`
- `state-read-only-public-properties-analyzer.cs:53` — same check, walking the entire base chain

Any consumer deriving from a *different* library's `State<T>` base gets error-severity `TWS001` false positives that break their build. Fix: resolve `` TimeWarp.State.State`1 `` once via `compilation.GetTypeByMetadataName` (in a compilation-start action) and compare with `SymbolEqualityComparer` against the original definition.

**Dead debug scaffolding (finding 26):** `timewarp-state-action-analyzer.cs` ships `DebugRule`/`TWD001` in `SupportedDiagnostics` (line 41) whose only producer, `ReportDebugInformation` (lines 60–64), has zero call sites — a registered diagnostic ID that can never fire. `LaunchDebugger`'s only call site is commented out (line 45); `Debugger.Launch` code in a shipped NuGet analyzer is one accidental uncomment away from freezing every consumer's build. Delete `DebugRule`, `DebugDiagnosticId`, `ReportDebugInformation`, and `LaunchDebugger`.

## Checklist

- [x] Metadata-name matching in all three analyzers (shared helper)
- [x] Analyzer test: a foreign `State<T>` base class produces no diagnostics
- [x] Existing analyzer tests still pass for real TimeWarp states
- [x] Delete TWD001 scaffolding and LaunchDebugger

## Notes

# Implementation Plan: Fix analyzer State type matching and remove debug scaffolding

**Task:** `kanban/in-progress/070-fix-analyzer-state-type-matching-and-remove-debug-scaffolding.md`  
**Findings:** 14 + 26 from `code-review-2026-06-11.md`

---

## Goals

1. Stop matching state types by simple name `"State"`; resolve `` TimeWarp.State.State`1 `` once per compilation and compare with `SymbolEqualityComparer`.
2. Remove dead `TWD001` / `LaunchDebugger` scaffolding from the action analyzer and related docs/releases.
3. Add analyzer tests proving foreign `State<T>` is ignored and real TimeWarp states still diagnose.

---

## Design decisions (confirmed)

| Decision | Choice |
|----------|--------|
| Shared helper | Compilation-start cache: resolve `` State`1 `` once, pass symbol into actions |
| Inheritance depth | Preserve per-analyzer behavior (only change identification) |
| Tests | Foreign-State negative + one positive per analyzer |
| TWD001 cleanup | Code + `AnalyzerReleases.Unshipped.md` + `timewarp-state-action-analyzer.cs.md` |

### Inheritance depths to preserve

| Analyzer | Current match | Keep |
|----------|---------------|------|
| `StateImplementationAnalyzer` | Immediate `BaseType` only | Yes |
| `StateInheritanceAnalyzer` | Direct base in `BaseList` only | Yes |
| `StateReadOnlyPublicPropertiesAnalyzer` | Full base chain walk | Yes |

---

## Files

### Create

| File | Purpose |
|------|---------|
| `source/timewarp-state-analyzer/state-symbol-helpers.cs` | Shared metadata-name resolution + equality helpers |
| `tests/timewarp-state-analyzer-tests/state-implementation-analyzer-tests.cs` | Foreign negative + real-state positive for TWS001 |
| `tests/timewarp-state-analyzer-tests/state-inheritance-analyzer-tests.cs` | Foreign negative + real-state positive for inheritance/sealed rules |
| `tests/timewarp-state-analyzer-tests/state-read-only-public-properties-analyzer-tests-new.cs` or section in new file | Foreign negative + real-state positive for read-only properties |

**Recommendation:** one test file per analyzer (matches existing layout). Put foreign-base cases in each file so failures point at the right analyzer.

### Modify

| File | Change |
|------|--------|
| `source/timewarp-state-analyzer/state-implementation-analyzer.cs` | Compilation-start + helper |
| `source/timewarp-state-analyzer/state-inheritance-analyzer.cs` | Compilation-start + helper |
| `source/timewarp-state-analyzer/state-read-only-public-properties-analyzer.cs` | Compilation-start + helper |
| `source/timewarp-state-analyzer/timewarp-state-action-analyzer.cs` | Delete TWD001 scaffolding |
| `source/timewarp-state-analyzer/AnalyzerReleases.Unshipped.md` | Remove `TWD001` row |
| `source/timewarp-state-analyzer/timewarp-state-action-analyzer.cs.md` | Remove TWD001 sections |

### Leave alone

- Existing fully commented `state-read-only-public-properties-analyzer-tests.cs` — do not re-enable in this task
- `timewarp-state-action-analyser-tests.cs` — regression only (must still pass after scaffolding delete)
- Analyzer project `.csproj` — no package changes needed

---

## Part A — Shared helper

### New file: `state-symbol-helpers.cs`

```csharp
namespace TimeWarp.State.Analyzer;

internal static class StateSymbolHelpers
{
  public const string StateMetadataName = "TimeWarp.State.State`1";

  public static INamedTypeSymbol? GetTimeWarpStateType(Compilation compilation) =>
    compilation.GetTypeByMetadataName(StateMetadataName);

  /// <summary>
  /// True if type is a constructed form of TimeWarp.State.State&lt;T&gt;
  /// (compares OriginalDefinition to the open generic from metadata).
  /// </summary>
  public static bool IsTimeWarpState(
    INamedTypeSymbol? type,
    INamedTypeSymbol timeWarpStateType) =>
    type is not null
    && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, timeWarpStateType);

  /// <summary>
  /// True if type or any base type is TimeWarp.State.State&lt;T&gt;.
  /// </summary>
  public static bool InheritsFromTimeWarpState(
    INamedTypeSymbol? type,
    INamedTypeSymbol timeWarpStateType)
  {
    for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
    {
      if (IsTimeWarpState(current, timeWarpStateType))
        return true;
    }
    return false;
  }
}
```

### Why `OriginalDefinition`

`GetTypeByMetadataName("TimeWarp.State.State`1")` returns the open generic. Consumer code uses constructed forms (`State<CounterState>`). Compare `baseType.OriginalDefinition` to the cached open symbol.

### Missing reference behavior

If the consumer does not reference TimeWarp.State, `GetTypeByMetadataName` returns `null`. Early-out: register no further actions. Do **not** fall back to simple-name matching.

---

## Part B — Analyzer rewrites

### Pattern (all three)

Replace direct RegisterSymbolAction / RegisterSyntaxNodeAction in Initialize with RegisterCompilationStartAction that:
1. Resolves TimeWarp.State.State`1 once
2. Early-returns if null
3. Registers symbol/syntax actions that close over timeWarpState

### 1. state-implementation-analyzer.cs

Current: `symbol.BaseType.Name == "State" && symbol.BaseType.TypeArguments.Length == 1`
New: `StateSymbolHelpers.IsTimeWarpState(symbol.BaseType, timeWarpState)` — immediate base only

### 2. state-inheritance-analyzer.cs

Current: `!baseTypeSymbol.Name.Equals("State")`
New: `!StateSymbolHelpers.IsTimeWarpState(baseTypeSymbol, timeWarpState)` — first base in BaseList only

### 3. state-read-only-public-properties-analyzer.cs

Current: walk base chain with Name == "State"
New: `StateSymbolHelpers.InheritsFromTimeWarpState(classSymbol, timeWarpState)` — full base chain

---

## Part C — Delete debug scaffolding

### timewarp-state-action-analyzer.cs

Delete:
- DebugDiagnosticId = "TWD001"
- DebugRule descriptor
- DebugRule from SupportedDiagnostics
- // LaunchDebugger();
- LaunchDebugger() method
- ReportDebugInformation() method

### AnalyzerReleases.Unshipped.md

Remove TWD001 row

### timewarp-state-action-analyzer.cs.md

Remove TWD001 documentation sections

---

## Part D — Tests

### Infrastructure (copy from existing action tests)

Use CSharpAnalyzerTest with FixieVerifier, Net100 reference assemblies, TimeWarp.State.dll as AdditionalReference.

### Foreign State&lt;T&gt; fixture

Inline OtherLib.State&lt;T&gt; in TestCode. Still add TimeWarp.State.dll so analyzer can resolve real metadata name. Foreign type must not derive from real State.

### Test matrix

state-implementation-analyzer-tests.cs:
- Given_ForeignState_WithNoCloneOrCtor → No TWS001
- Given_TimeWarpState_WithoutCloneOrParameterlessCtor → TWS001

state-inheritance-analyzer-tests.cs:
- Given_ForeignState_WithWrongTypeArg → No diagnostics
- Given_TimeWarpState_WithWrongTypeArg → StateInheritanceTypeArgumentRule

state-read-only (new focused tests, don't uncomment old suite):
- Given_ForeignState_WithPublicSetter → No diagnostics
- Given_TimeWarpState_WithPublicSetter → StateReadOnlyPublicPropertiesRule

Existing TW0001 tests must still pass.

---

## Part E — Implementation order

1. Add state-symbol-helpers.cs
2. Rewrite the three state analyzers
3. Delete TWD001 scaffolding; update Unshipped + .md
4. Add tests
5. Build & test
6. Grep for leftover simple-name matches and TWD001

---

## Verification steps

```bash
dotnet build ./source/timewarp-state-analyzer/timewarp-state-analyzer.csproj
dotnet build ./tests/timewarp-state-analyzer-tests/timewarp-state-analyzer-tests.csproj
dotnet fixie timewarp-state-analyzer-tests
```

Grep gates:
```bash
rg 'BaseType\.Name\s*==\s*"State"|Name\.Equals\("State"\)' source/timewarp-state-analyzer/
rg 'TWD001|LaunchDebugger|DebugRule|ReportDebugInformation' source/timewarp-state-analyzer/
```

---

## Out of scope

- Fixing ICloneable simple-name matching
- IAction / IState string matching in action analyzer
- Re-enabling fully commented read-only property test suite
- Changing diagnostic IDs, severities, or messages
- Unifying inheritance depth across analyzers

---

## Done when

- Metadata-name matching in all three analyzers (shared helper)
- Analyzer test: foreign State&lt;T&gt; → no diagnostics
- Existing analyzer tests still pass for real TimeWarp states (+ new positives)
- TWD001 scaffolding and LaunchDebugger deleted (code + Unshipped + docs)

## Results

### What was implemented
1. **Shared helper** (`state-symbol-helpers.cs`) — resolve `TimeWarp.State.State`1` once per compilation via `GetTypeByMetadataName`; match with `SymbolEqualityComparer` on `OriginalDefinition`.
2. **Three state analyzers** — switched to `RegisterCompilationStartAction`; early-out if TimeWarp.State is missing; inheritance depth preserved per analyzer (immediate base / first BaseList / full chain).
3. **TWD001 cleanup** — removed `DebugRule`, `LaunchDebugger`, `ReportDebugInformation`, Unshipped row, docs, and orphaned `.editorconfig` severity.
4. **Tests** — foreign `OtherLib.State<T>` negatives + real TimeWarp positives for each of the three analyzers.

### Files changed
**Created:**
- `source/timewarp-state-analyzer/state-symbol-helpers.cs`
- `tests/timewarp-state-analyzer-tests/state-implementation-analyzer-tests.cs`
- `tests/timewarp-state-analyzer-tests/state-inheritance-analyzer-tests.cs`
- `tests/timewarp-state-analyzer-tests/state-read-only-public-properties-analyzer-tests-new.cs`

**Modified:**
- `source/timewarp-state-analyzer/state-implementation-analyzer.cs`
- `source/timewarp-state-analyzer/state-inheritance-analyzer.cs`
- `source/timewarp-state-analyzer/state-read-only-public-properties-analyzer.cs`
- `source/timewarp-state-analyzer/timewarp-state-action-analyzer.cs`
- `source/timewarp-state-analyzer/AnalyzerReleases.Unshipped.md`
- `source/timewarp-state-analyzer/timewarp-state-action-analyzer.cs.md`
- `.editorconfig` (orphaned TWD001 severity removed)

### Key decisions
- Metadata-name match only; no simple-name fallback when reference is missing.
- Per-analyzer inheritance depth preserved (not unified).
- Old fully-commented read-only suite left alone; new focused tests in `*-tests-new.cs`.
- ICloneable / IAction / IState string matching left out of scope.

### Test outcomes
- **10/10 passed** (`dotnet test` on `timewarp-state-analyzer-tests`)
- Grep gates clean for simple-name `"State"` matching and TWD001 scaffolding in analyzer source
- Review: **PASS** (one minor editorconfig leftover fixed)
