# Round 1 — general
**Date:** 2026-10-05
**Scope reviewed:** branch vs master (4002e6f5)

## Summary
The allow-list is sound. `Type.GetType` is gone, every name resolves only through an exact-match, ordinal dictionary that is filled at startup, and the generic constraint plus the `Allow(Type)` check keep non-actions out. Nested `+` names, generics (exact match only) and alias collisions (startup throw) are handled correctly. `dotnet test tests/timewarp-state-tests` passes (62 passed, 1 skipped). I found no bypass. The remaining items are minor consistency and logging points.

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-state/features/javascript-interop/json-request-handler.cs:66
- Description: A null or whitespace name throws `ArgumentException` before any warning is logged, and it is not an `InvalidRequestTypeException`. Requirement 2 says every rejection logs a warning and throws a clear exception. This case still fails closed, but it is inconsistent with the other rejections.
- Suggestion: Log a `JsonRequestOfInvalidType` warning and throw `InvalidRequestTypeException`, or document that empty names are an argument error.
- Status: open

### Issue 2 — Severity: suggestion
- File: source/timewarp-state/features/javascript-interop/json-request-handler.cs:128
- Description: The warning for an invalid payload logs the full caller-supplied `requestAsJson` at Warning level, with no length cap. The unknown-name warning also logs the raw name. Page script controls both values, so it can flood logs or inject large or multi-line content. That is somewhat ironic in a hardening change.
- Suggestion: Log only the type name and the exception at Warning. Keep the payload at Debug, which the handler already does, or truncate it.
- Status: open

### Issue 3 — Severity: nit
- File: source/timewarp-state/features/javascript-interop/json-request-handler.cs:113
- Description: The catch filter is `JsonException or NotSupportedException or TargetInvocationException or MissingMethodException`. System.Text.Json can propagate other exceptions thrown by a constructor or setter, such as `InvalidOperationException` or `ArgumentException`. These escape as raw exceptions with no warning log and no `InvalidRequestTypeException`. The request is still not sent, but the behavior differs from the documented rejection.
- Suggestion: Catch `Exception` (excluding fatal ones) in `CreateRequest`, or add `InvalidOperationException` and `ArgumentException` to the filter.
- Status: open

### Issue 4 — Severity: nit
- File: source/timewarp-state/features/javascript-interop/javascript-dispatch-registry.cs:60
- Description: `Add` mutates the registry name by name. If an alias collides, or the alias is whitespace and `AddName` throws, the type is already registered under its full names. The same happens with duplicate calls in a single `Allow` chain. This only matters if a host catches the startup exception. `GetOrAdd` is also not thread-safe, which is fine for startup-only use. Separately, the `IReduxRequest` branch of the non-action check at json-request-handler.cs:92 is only reachable for DevTools types, so the check is redundant defence in depth rather than a bug.
- Suggestion: Validate the alias before adding any names. Optionally note in the design region that the registry is write-at-startup and read-only afterwards.
- Status: open

### Issue 5 — Severity: suggestion
- File: tests/timewarp-state-tests/javascript-interop/json-request-handler-dispatch-tests.cs
- Description: Requirement 5 is met. It covers an allowed action, an unknown name, a non-action type, bad JSON, empty JSON without a default constructor, and the DevTools path on and off. The gaps are a version-mismatched assembly-qualified name, which is the migration claim in the docs, and a nested `+` type. `null` JSON, the whitespace name, and registry collisions (alias versus another type's full name) are also untested.
- Suggestion: Add a registry unit test for the version-stripped AQN, the alias collision throw, and the `"null"` payload. Requirement 3's "Development" gating is really "UseReduxDevTools() called", so the docs advice to call it only in Development is the actual gate. That is accurate as written.
- Status: open
