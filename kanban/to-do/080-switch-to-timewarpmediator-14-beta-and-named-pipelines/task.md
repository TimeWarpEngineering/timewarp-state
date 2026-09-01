# Switch to TimeWarp.Mediator 14-beta and named pipelines

## Description

This is the **real test** of the source-gen rewrite. Drop martinothamar/Mediator. Consume **TimeWarp.Mediator 14.0.0-beta** (`AddGeneratedMediator`), with **`ISender<ClientPipeline>`** on the Blazor client and **`ISender<ServerPipeline>`** on the server.

Do **not** finish **049–051** as a martinothamar release. That path is not the destination.

Wait for mediator **005-003** (NuGet `14.0.0-beta.1`). Do not start on a project reference to unlabeled 13.0.0 git unless 005-001 is done **and** the operator explicitly waives the package.

## Children

- **080-001** Packages + `AddGeneratedMediator` (from **origin/dev**, not stale master)
- **080-002** Named pipelines (`ClientPipeline` / `ServerPipeline`)
- **080-003** Tests, docs; 049–051 are not the destination

## Requirements

- **Start from `origin/dev`.** origin-home `master` is behind (martinothamar 3.0.2 + net10 live on `dev`). Claiming from master and implementing the switch there is wrong.
- Replace `Mediator.Abstractions` / `Mediator.SourceGenerator` with TimeWarp.Mediator 14-beta packages
- Generated registration + membership attributes (`[assembly: MediatorAssembly]`, `[MediatorBehavior]`, `[MediatorScope]`)
- Client vs server senders; no single shared `IMediator` with behavior filters
- Existing tests (test-app, integration, e2e as applicable) on the generated path
- This is soak, not a State NuGet “we switched, ship it” until the beta has actually run

## Out of scope

- Cutting TimeWarp.Mediator 14.0.0 stable
- Nuru 443
- TimeWarp.ServiceGen (Nuru 444)
- Completing 049–051 as martinothamar docs/release

## Notes

- Cross-repo wait: timewarp-mediator **005-003**. Not a same-repo `## Depends on`.
- Ids 058–079 already exist on `origin/dev`; this epic is **080** so it does not collide.

## Session

- Created: 154892 (2026-09-01)
