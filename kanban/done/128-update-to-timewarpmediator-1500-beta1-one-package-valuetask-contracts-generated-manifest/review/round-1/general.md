# Round 1 — general
**Date:** 2026-10-28
**Scope reviewed:** as framework (merge-base `184bc681`..`ae31fb0b`, product paths only)

## Summary

The change moves State onto TimeWarp.Mediator 15.0.0-beta.2. All seven named behaviors return `ValueTask<TResponse>`, take `MessageHandlerDelegate<TRequest, TResponse>`, and call `next(request, cancellationToken)`. `PipelineBehaviorRegistry` now parses `MediatorManifest` v2 JSON, and the obsolete `PipelineBehaviorRegistrySourceGenerator` and its tests are gone. Package references follow the rule "only hosts reference Generators": the State, Blazor, Plus, and Telemetry libraries do not, and the `timewarp-state.csproj` match is only a comment. The ISender/IPublisher fakes have no `CreateStream`. The risk is low.

Checks made against the repo:

- **Sync-throw semantics.** Every behavior awaits `next` inside its existing try, or has nothing to clean up, so a synchronous throw from `next` behaves the same as a faulted task. `Store.GetOrAdd` calls `Publisher.Publish(...).AsTask().ContinueWith`. The generated `Publisher_ClientPipeline.Publish` throws synchronously only for a null notification and routes handlers through an `async` method, so a handler fault still reaches the `ContinueWith` logger. `JsonRequestHandler` uses `.AsTask()`, and a synchronous throw from that `[JSInvokable]` still reaches JS as a rejected call.
- **Build.** `dotnet build timewarp-state.slnx` succeeds. Every warning it prints (NU1510, NU1902, TW0007, CS8600/CS8602, RS0030, BL0010/BL0016) is in a file or line this diff does not touch, so the change adds no warning. No TWM diagnostic fires.
- **Line endings.** The large csproj and `test-app-server/program.cs` churn is CRLF→LF normalization (`.editorconfig` says `end_of_line = lf`). With `-w` the diff is only the intended reference changes.
- **Glossary.** `glossary.md` is `ganda repo audit --fix` scaffolding, which is policy baseline. No term in this review needed a definition.

## Issues

### Issue 1 — Severity: suggestion
- File: documentation/release-notes/ (no entry; `source/Directory.Build.props:9` still `12.0.0-beta.16`)
- Description: This change breaks consumers of the TimeWarp.State packages. Handlers and behaviors move from `Task` to `ValueTask`. `TimeWarp.Mediator.Contracts` and `TimeWarp.Mediator.Analyzers` become `TimeWarp.Mediator`. Hosts must import `TimeWarp.Mediator.Generated`. `LogTimeWarpStateMiddleware` gains a required `mediatorManifestJson` parameter. `PipelineBehaviorRegistry.Register` and `PipelineBehaviorRegistrySourceGenerator` are removed. `GetComponentOrder` now reads the last parsed manifest, not the service collection. Repo convention gives each breaking feature a release-notes entry and a version bump: `b05ababd` (beta.16 note + props bump), `91e42103` (Mediator 14.0.0-beta.5 dogfood bump + note), and `b9288f80`. This change adds neither. A consumer that upgrades gets compile breaks with no migration notes.
- Suggestion: Add a release note that lists the breaking items above as **Breaking**, the migration-page link, and the beta.2 pin. Follow the repo's version convention: bump to the next beta with a new `release12.0.0-beta.N.md` plus a `toc.yml` entry, or append to `release12.0.0-beta.16.md` if beta.16 is unpublished. `assert-version-ssot` must stay green.
- Status: open

### Issue 2 — Severity: nit
- File: tests/timewarp-state-tests/javascript-interop/json-request-handler-dispatch-tests.cs:190
- Description: Removing the `CreateStream` members left a double blank line before the closing brace in three fakes: this file at 190, `json-request-handler-lifecycle-tests.cs:67`, and `store/store-get-or-add-tests.cs:177`.
- Suggestion: Remove the extra blank lines.
- Status: open
