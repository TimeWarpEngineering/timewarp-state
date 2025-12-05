# Task 039: Migrate Mediator - Update Global Usings

## Description

- Update all global using statements from TimeWarp.Mediator namespace to Mediator namespace
- This enables the new library's types to be available throughout the codebase

## Requirements

- Replace `TimeWarp.Mediator` and `TimeWarp.Mediator.Pipeline` namespaces with `Mediator`
- All files should reference the new namespace

## Checklist

### Implementation
- [ ] Update `source/timewarp-state/global-usings.cs`:
  - [ ] Remove `global using TimeWarp.Mediator.Pipeline;`
  - [ ] Remove `global using TimeWarp.Mediator;`
  - [ ] Add `global using Mediator;`
- [ ] Update `source/timewarp-state-plus/global-usings.cs`:
  - [ ] Remove `global using TimeWarp.Mediator;`
  - [ ] Remove `global using TimeWarp.Mediator.Pipeline;`
  - [ ] Add `global using Mediator;`
- [ ] Update `tests/timewarp-state-tests/global-usings.cs`:
  - [ ] Remove `global using TimeWarp.Mediator;`
  - [ ] Add `global using Mediator;`
- [ ] Update `tests/test-app/test-app-contracts/global-usings.cs`:
  - [ ] Remove `global using TimeWarp.Mediator;`
  - [ ] Add `global using Mediator;`
- [ ] Update `tests/test-app/test-app-client/global-usings.cs`:
  - [ ] Remove `global using TimeWarp.Mediator;`
  - [ ] Remove `global using TimeWarp.Mediator.Pipeline;`
  - [ ] Add `global using Mediator;`
- [ ] Update `tests/client-integration-tests/global-usings.cs`:
  - [ ] Remove `global using TimeWarp.Mediator;`
  - [ ] Add `global using Mediator;`
- [ ] Update `samples/02-action-tracking/wasm/sample-02-wasm/program.cs`:
  - [ ] Remove `using TimeWarp.Mediator;`
  - [ ] Add `using Mediator;`

## Notes

**Files to modify (7 total):**
1. `source/timewarp-state/global-usings.cs`
2. `source/timewarp-state-plus/global-usings.cs`
3. `tests/timewarp-state-tests/global-usings.cs`
4. `tests/test-app/test-app-contracts/global-usings.cs`
5. `tests/test-app/test-app-client/global-usings.cs`
6. `tests/client-integration-tests/global-usings.cs`
7. `samples/02-action-tracking/wasm/sample-02-wasm/program.cs`

## Implementation Notes

