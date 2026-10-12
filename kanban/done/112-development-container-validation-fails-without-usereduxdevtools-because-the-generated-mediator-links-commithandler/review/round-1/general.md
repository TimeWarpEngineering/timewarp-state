# Round 1 — general
**Date:** 2026-10-12
**Scope reviewed:** `git diff origin/master...HEAD` (126 files): modified source/test/doc files, deletion sanity check, leftover-reference grep, TS vs built JS, regression test, migration/release-note API names. Ran `dotnet build timewarp-state.slnx` (0 errors, 160 warnings, all in pre-existing areas such as TW0007/CS8602/RS0030 in client-integration-tests) and `dotnet test tests/timewarp-state-tests` (111 passed, 1 skipped, 0 failed).

## Summary
The removal is clean. Live code has no references to the removed APIs. Built JS matches the TS (the module imports only `timewarp-state.js`, `logger.js` and `constants.js`; the .map files are updated; `types/*.d.ts` are consistent). JsonRequestHandler and JavaScript dispatch are intact. The migration and release-note API names (`AddTimeWarpStateTelemetry`, `AddTimeWarpStateDevToolsExporter`, `AddTimeWarpStateDevTools`, `WithTimeWarpStateDevTools`, `TimeWarpStateTelemetry.ActivitySourceName`) all exist in source. The regression test passes. By reasoning it would have failed before the change: `ContainerProbeAction` is public, so the mediator generator emits `AddGeneratedMediator<ClientPipeline>` and links the Blazor assembly's handlers, including `CommitHandler`, whose dependency was not registered, so `ValidateOnBuild` would throw. One leftover remains in the solution file, plus a few minor items.

## Issues
### Issue 1 — Severity: suggestion
- File: timewarp-state.slnx:4 (and about 35 more lines)
- Description: The `ReduxDevToolsEnabled` solution build type and its per-project mappings remain. The `Configurations` property that defined it was removed from timewarp-state.csproj, so it is now a dead solution configuration named after the removed feature.
- Suggestion: Remove the `<BuildType Name="ReduxDevToolsEnabled" />` entry and every `Solution="ReduxDevToolsEnabled|..."` mapping from the slnx, or record in the task that it is intentionally deferred.
- Status: open

### Issue 2 — Severity: nit
- File: source/timewarp-state-telemetry/readme.md:112, source/timewarp-state-blazor/readme.md:12
- Description: These live readmes still name the removed APIs ("was removed in 12.0.0-beta.11"). This is allowed historical wording, but it keeps references in package readmes.
- Suggestion: Optionally drop the sentences and keep them only in the migration guide.
- Status: open

### Issue 3 — Severity: nit
- File: tests/timewarp-state-tests/architecture/add-timewarp-state-blazor-tests.cs:60
- Description: The method name `ValidateOnBuild_Without_ReduxDevTools` and the Purpose and Design text refer to the removed feature, which no longer exists as an option.
- Suggestion: Rename to something like `ValidateOnBuild_Succeeds_For_Development_Host`.
- Status: open
