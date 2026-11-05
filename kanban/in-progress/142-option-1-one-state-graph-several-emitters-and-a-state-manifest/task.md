# Option 1: one State graph, several emitters, and a State manifest

## Description

Source: task 140 `report/analysis.md` (options as code, costs, what each unblocks). Read it first. Option 1. Follows Option 0.

Replace the per-generator scans with one `TimeWarpStateGenerator` in the Mediator style: one equatable graph of states, actions, handlers, catalog entries, persistence and clone facts, built once; emitters for the dispatch methods, catalog, persistence, clone, and a State manifest DevTools and analyzers can read.

## Requirements

- One graph, one builder. Analyzers stay a separate project but read the same fact extractors linked as source. Do not copy Mediator's two-builder split.
- Emitters on the graph for everything the four generators emit today; output identical for existing shapes (golden tests).
- State manifest: states and actions known up front for DevTools, dispatch from DevTools, and for consumer analyzers (architecture's coverage checks).
- Incrementality test on the graph; caching pinned.
- Version bump per the repo's release rule.

## Goal (implementer restatement)

Replace the four `[Generator]` classes (ActionSet method, action catalog, persistence, clone) with one
`TimeWarpStateGenerator`. It builds one equatable `StateGraph` from fact extractors and runs several emitters on it.
Add a State manifest that runtime code (DevTools) and other analyzers can read.

Acceptance criteria, in my words:

1. **One graph, one builder.** Fact extractors turn syntax and symbols into equatable facts: actions (dispatch,
   catalog, manifest), persistent states, and states. One `StateGraphBuilder` merges them into a `StateGraph`. The
   clone planner stays a semantic stage. Its equatable plan feeds the same graph. No second graph builder.
2. **Analyzers share the extractors.** The analyzer project stays separate. It compiles the shared fact files as
   linked source (`<Compile Include=... Link=...>`) and uses them instead of its own copies. That covers the
   catalog placement rule, the default catalog name, assembly visibility, and the `State<T>` inheritance check.
3. **Emitters with golden parity.** Emitters cover the dispatch method, catalog, persistence `Load`, and clone and
   registration. Hint names, generated text and diagnostics stay identical to the old generators for every
   existing shape. Golden files captured from the old generators *before* the rewrite pin this, and the existing
   generator suite still passes.
4. **State manifest.** A new emitter writes `TimeWarp.State.StateManifest.g.cs`. It holds an internal
   `TimeWarpStateManifest` (Version, Json) and `[assembly: TimeWarp.State.StateManifestAttribute(version, json)]`.
   The JSON lists each state (type, persistence, clone kind) and its actions (type, telemetry name, method,
   parameters, catalog name, tracked). A public `StateManifest` reader in `TimeWarp.State` parses it without
   reflection over members. An analyzer can read the attribute from a referenced assembly's metadata, and a test
   proves it.
5. **DevTools consumption.** The DevTools export posts the inspected app's manifests at startup. DevTools ingests
   and keeps them, so the inspector knows states and actions before any span arrives. Dispatch from DevTools gets
   its prerequisites in the manifest (catalog names and parameter schemas). A dispatch transport back into the
   inspected app is a follow-up unless it fits.
6. **Incrementality.** Every step is named. A test pins that an unrelated edit leaves the graph and every output
   `Cached` or `Unchanged` with identical text, and that a relevant edit re-emits.
7. **Release.** `12.0.0-beta.17` is not on NuGet yet (beta.16 is the latest), so the repo rule keeps the version
   and adds this work to the beta.17 release notes. Build 0 errors with no new warnings, tests green,
   `ganda repo audit` clean, committed and pushed on the task branch.

## Checklist

- [ ] Graph and builder
- [ ] Emitters with golden parity
- [ ] Manifest and DevTools consumption
- [ ] Incrementality test; build 0/0

## Session

- Created: 66368815-cdd5-46be-bde8-a09d94e1cdc8 (2026-11-06)

## Depends on

- Option 0 task (filed with this one)

## Notes

Steve 2026-11-06 decision; order 0 → 1.
