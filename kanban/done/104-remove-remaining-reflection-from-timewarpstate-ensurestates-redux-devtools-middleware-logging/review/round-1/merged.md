# Round 1 — merged findings
**Date:** 2026-10-12
**Sources:** general

## Counts

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 3 | 1 |
| nit | 0 | 2 | 1 |

## Issues

### M1 — Severity: suggestion — Status: fixed
- File: source/timewarp-state/extensions/state-registration-registry.cs:39
- Description: Registration depends on each selected assembly's module initializer having already run; `typeof(X).Assembly` does not trigger it per ECMA-335 (untested on Mono/WASM, NativeAOT).
- Suggestion: `RuntimeHelpers.RunModuleConstructor` before lookup.
- Source: general
- Disposition notes: 0f1411f8 — `Apply` runs `RunModuleConstructor(assembly.ManifestModule.ModuleHandle)` per assembly and snapshots `Registrars` after it. Release build still zero IL2xxx/IL3xxx. No new cross-assembly test (test project does not reference Plus); `client-integration-tests` already resolves Plus `ActionTrackingState` through DI.

### M2 — Severity: suggestion — Status: fixed
- File: source/timewarp-state/extensions/state-registration-registry.cs:46
- Description: An assembly compiled without the generator silently registers nothing.
- Suggestion: Log/assert or document the generator requirement.
- Source: general
- Disposition notes: 0f1411f8 — documented in release notes and `cloning.md` (PackageReference gets the generator; ProjectReference must add it as Analyzer). No runtime log: an assembly with no states is a legitimate input, and no logger exists at registration time.

### M3 — Severity: suggestion — Status: fixed
- File: documentation/release-notes/release12.0.0-beta.12.md:18
- Description: Release notes omit behavior changes (open-generic ICloneable states, constructing-assembly registration, private/protected/file-local skipped).
- Suggestion: Add "Behavior changes" bullets.
- Source: general
- Disposition notes: 0f1411f8 — four behavior-change bullets added.

### M4 — Severity: nit — Status: fixed
- File: source/timewarp-state/extensions/service-collection-extensions.log-timewarp-state-middleware.cs (`GetComponentOrder`)
- Description: Public `GetComponentOrder(componentType)` now only matches `IPipelineBehavior<,>`.
- Suggestion: Document or throw.
- Source: general
- Disposition notes: 0f1411f8 — XML `<remarks>` documents it; no throw (only in-repo callers pass `IPipelineBehavior<,>`).

### M5 — Severity: suggestion — Status: wontfix
- File: source/timewarp-state-source-generator/pipeline-behavior-registry-source-generator.cs:23
- Description: Generator uses `CompilationProvider` and walks the assembly's namespaces on every change.
- Suggestion: Syntax-provider pipeline with equatable model.
- Source: general
- Disposition notes: wontfix (orchestrator). Same shape as the existing clone generator in the same package, which already reruns on every compilation; the walk is limited to the source assembly's own types and is cheap relative to that. Worth revisiting together with the clone generator if IDE latency becomes measurable; not a correctness issue for this task.

### M6 — Severity: nit — Status: wontfix
- File: source/timewarp-state-source-generator/state-clone-planner.cs:1874
- Description: Planner `CanName` treats `private protected` nested types as nameable; generated code fails with CS0122. Pre-existing in the clone registry path; the new registration line reuses it.
- Suggestion: Exclude `ProtectedAndInternal`.
- Source: general
- Disposition notes: wontfix (orchestrator). Pre-existing since task 097: the clone path already fails to compile for such a state before the new registration line matters, so excluding it only from `ServiceTypes` changes nothing observable, and changing the shared `CanName` alters clone planning beyond this task's scope. Candidate follow-up for the clone generator.

### M7 — Severity: nit — Status: fixed
- File: source/timewarp-state-source-generator/pipeline-behavior-registry-source-generator.cs:30
- Description: Generator emits a call to `PipelineBehaviorRegistry` without checking it exists.
- Suggestion: Return early when absent.
- Source: general
- Disposition notes: 0f1411f8 — early return when `TimeWarp.State.PipelineBehaviorRegistry` does not resolve. No dedicated test (shared driver always references TimeWarp.State).

## Duplicates / conflicts

- None; single reviewer.
