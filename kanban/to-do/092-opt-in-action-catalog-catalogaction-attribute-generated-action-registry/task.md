# Opt-in action catalog: [CatalogAction] attribute + generated action registry

## Description

Consumers need to enumerate the user-facing actions of an app at runtime — first for a Ctrl-K
command palette (timewarp-architecture task 239, GitHub timewarp-architecture#102), later for agent
tools (WebMCP `registerTool`). Today nothing in TimeWarp.State describes actions: there is no
action catalog, metadata registry, description attribute, or runtime enumeration. `[TrackAction]`
(`source/timewarp-state-plus/features/action-tracking/pipeline/track-action-attribute.cs`) is a
busy-indicator marker and must keep that single meaning. `ActionSetMethodSourceGenerator`
(`source/timewarp-state-source-generator/action-set-method-generator.cs`) already parses each
`*ActionSet.Action`'s first explicit constructor to emit `State.Method(args, ct)`; the catalog
should reuse that parse rather than duplicate it.

Contract agreed in timewarp-architecture task 238 (research; kitchen
`kanban/done/238-evaluate-jev-for-ctrl-k-and-webmcp-action-catalog/research/catalog.json` there):
entries carry `name`, `description`, `execute`, `input_schema`, `visibility`, `permissions`.
Consumers own ranking (deterministic C# shortlist; no LLM ranker) and permission enforcement.

## Requirements

- **Attribute (runtime package, opt-in):** `[CatalogAction(Description = "…")]` on the nested
  `Action` class, with optional `Name` (default `<StateWithoutSuffix>.<ActionSetWithoutSuffix>`,
  e.g. `Credentials.AddPasskey`), `Permissions` (string[] — consumer-defined policy/permission ids,
  opaque to State), and `Visibility` (`Human` | `Agent` | `Both`, default `Human`). `Description` is
  required and must be one plain sentence (analyzer below). Opt-in only: an action without the
  attribute is never cataloged (Fetch*/Clear*/Debug/inbound hub actions stay out by default).
- **Generator:** in the existing source-generator project, emit per cataloged action a descriptor
  (name, description, permissions, visibility, state type, action type, parameter list with name /
  CLR type / optional default — from the same ctor parse as the ActionSet method generator) and one
  per-assembly registry (e.g. `ActionCatalog.All` : `IReadOnlyList<ActionCatalogEntry>`), plus an
  `Execute(IStore, object?[] args, CancellationToken)` (or equivalent typed invoker) that calls the
  same generated `State.Method(...)` — no reflection at runtime, AOT/trim safe.
  Multi-assembly apps: document how a consumer aggregates registries (e.g. assembly-level
  attribute + DI registration helper `AddActionCatalog(typeof(Marker).Assembly)`), no global static
  scanning.
- **Input schema:** expose parameters as data (name, type, required); optionally a JSON-schema
  string for primitives/enums. Complex `Command` parameters: record type name only in v1 and say so.
- **Analyzer (existing analyzer project):** `[CatalogAction]` on a type that is not a nested
  `*ActionSet.Action` → error; missing/empty `Description` → error; duplicate `Name` in one
  assembly → error. Add to `AnalyzerReleases.Unshipped.md`.
- **Tests:** generator snapshot tests (single, multiple, parameters with defaults, name override,
  permissions/visibility), a runtime test in `timewarp-state-tests` or `test-app` that enumerates
  the registry and executes an entry end to end through the store, and analyzer tests for each
  diagnostic.
- **Docs:** README / docs page section "Action catalog" with an example; state explicitly that
  permissions are consumer-enforced and that `[TrackAction]` is unrelated.
- **Release:** ship in the next `12.0.0-beta.N`; timewarp-architecture pins forward (never backward).

## Checklist

- [ ] `CatalogActionAttribute` + `ActionCatalogEntry` + visibility enum (runtime package)
- [ ] Generator: descriptors + per-assembly registry + typed execute, reusing the ActionSet ctor parse
- [ ] Multi-assembly aggregation helper
- [ ] Analyzer diagnostics (placement, description, duplicate name)
- [ ] Generator, runtime, analyzer tests
- [ ] Docs section
- [ ] Released; consumer task notified (timewarp-architecture 239)

## Notes

- Consumer: timewarp-architecture task 239 (Ctrl-K command palette) — its command rows depend on
  this release; navigation rows do not.
- Out of scope: ranking, LLM/Jev integration, WebMCP wiring, changing `[TrackAction]`.
- Cockpit session: https://claude.ai/code/session_01QYpqCSgnvvLRpXrMKxu5ED

## Session

- Created: https://claude.ai/code/session_01QYpqCSgnvvLRpXrMKxu5ED (2026-09-30)
