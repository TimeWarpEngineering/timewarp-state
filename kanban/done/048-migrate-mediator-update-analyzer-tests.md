# Task 048: Migrate Mediator - Update Analyzer Tests

## Description

- Update analyzer tests to reference the new Mediator.Abstractions assembly instead of TimeWarp.Mediator.Contracts

## Requirements

- Update assembly references in analyzer test files
- Ensure tests compile and pass with new library

## ⚠️ Two corrections to the doc

1. The assembly is **`Mediator.dll`**, NOT `Mediator.Abstractions.dll` — the `Mediator.Abstractions` NuGet package ships its assembly as `Mediator.dll` (verified). Both `Mediator.dll` and `TimeWarp.State.dll` are already copied to the analyzer-tests output dir (via the project references), so the bare-filename `MetadataReference.CreateFromFile(...)` resolves.
2. The .NET 10 retarget surfaced a second issue once the reference was fixed: **CS1705** — the analyzer test's in-memory compilation used old default reference assemblies (`System.Runtime 4.2.2.0`) while `TimeWarp.State.dll` is net10 (`System.Runtime 10.0`) and `Mediator.dll` is net8. Fixed by setting `analyzerTest.ReferenceAssemblies = ReferenceAssemblies.Net.Net100` per test, which required bumping `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` 1.1.2 → 1.1.4 (1.1.2 only went up to `Net90`; net10 TimeWarp.State needs `Net100`).

## Checklist

### Implementation
- [x] Update `tests/timewarp-state-analyzer-tests/timewarp-state-action-analyser-tests.cs`:
  - [x] Change `TimeWarp.Mediator.Contracts.dll` → **`Mediator.dll`** (4 occurrences); renamed const to `MediatorAssemblyPath`
  - [x] Set `ReferenceAssemblies.Net.Net100` on each test (CS1705 fix)
- [x] Bump `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` 1.1.2 → 1.1.4 (for `Net100`)
- [x] Run analyzer tests — **4 passed, 0 failed**

## Notes

**Current pattern:**
```csharp
const string TimeWarpMediatorContractsAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
```

**New pattern required:**
```csharp
const string MediatorAbstractionsAssemblyPath = @"Mediator.Abstractions.dll";
```

**Files to modify:**
1. `tests/timewarp-state-analyzer-tests/timewarp-state-action-analyser-tests.cs`

## Implementation Notes

