# Task 048: Migrate Mediator - Update Analyzer Tests

## Description

- Update analyzer tests to reference the new Mediator.Abstractions assembly instead of TimeWarp.Mediator.Contracts

## Requirements

- Update assembly references in analyzer test files
- Ensure tests compile and pass with new library

## Checklist

### Implementation
- [ ] Update `tests/timewarp-state-analyzer-tests/timewarp-state-action-analyser-tests.cs`:
  - [ ] Change `TimeWarp.Mediator.Contracts.dll` to `Mediator.Abstractions.dll`
  - [ ] Update any namespace references in test code
- [ ] Run analyzer tests to verify they pass

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

