# Task 039: Migrate Mediator - Update Global Usings

## Description

- Update all global using statements from TimeWarp.Mediator namespace to Mediator namespace
- This enables the new library's types to be available throughout the codebase

## Requirements

- Replace `TimeWarp.Mediator` and `TimeWarp.Mediator.Pipeline` namespaces with `Mediator`
- All files should reference the new namespace

## Checklist

### Implementation
- [x] Update `source/timewarp-state/global-usings.cs`:
  - [x] Remove `global using TimeWarp.Mediator.Pipeline;`
  - [x] Remove `global using TimeWarp.Mediator;`
  - [x] Add `global using Mediator;`
- [x] Update `source/timewarp-state-plus/global-usings.cs`:
  - [x] Remove `global using TimeWarp.Mediator;`
  - [x] Remove `global using TimeWarp.Mediator.Pipeline;`
  - [x] Add `global using Mediator;`
- [x] Update `tests/timewarp-state-tests/global-usings.cs`:
  - [x] Remove `global using TimeWarp.Mediator;`
  - [x] Add `global using Mediator;`
- [x] Update `tests/test-app/test-app-contracts/global-usings.cs`:
  - [x] Remove `global using TimeWarp.Mediator;`
  - [x] Add `global using Mediator;`
- [x] Update `tests/test-app/test-app-client/global-usings.cs`:
  - [x] Remove `global using TimeWarp.Mediator;`
  - [x] Remove `global using TimeWarp.Mediator.Pipeline;`
  - [x] Add `global using Mediator;`
- [x] Update `tests/client-integration-tests/global-usings.cs`:
  - [x] Remove `global using TimeWarp.Mediator;`
  - [x] Add `global using Mediator;`
- [x] Update `samples/02-action-tracking/wasm/sample-02-wasm/program.cs`:
  - [x] Remove `using TimeWarp.Mediator;`
  - [x] Add `using Mediator;`

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

All global usings have been updated from `TimeWarp.Mediator` and `TimeWarp.Mediator.Pipeline` namespaces to the new `Mediator` namespace. 

**Note**: The build will fail after this task because subsequent tasks (040-049) need to update the actual code to use the new Mediator library's types and interfaces, which have different signatures:
- `IRequestHandler<,>.Handle()` returns `ValueTask<T>` instead of `Task<T>`
- `IPipelineBehavior<,>.Handle()` uses `MessageHandlerDelegate<,>` instead of `RequestHandlerDelegate<>`
- Pre/post processors have different interfaces
- Source generator configuration differs
