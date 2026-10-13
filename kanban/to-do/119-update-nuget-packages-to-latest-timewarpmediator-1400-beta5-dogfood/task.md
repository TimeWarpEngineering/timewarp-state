# Update NuGet packages to latest (TimeWarp.Mediator 14.0.0-beta.5 dogfood)

## Description

TimeWarp.Mediator `14.0.0-beta.5` shipped 2026-10-13 (https://github.com/TimeWarpEngineering/timewarp-mediator/releases/tag/v14.0.0-beta.5).
It carries the 22 repository-review changes (mediator kanban 013, PR #95): net10.0-only runtime and Contracts,
smaller runtime public API, generator handler binding and type-switch fixes, incremental generator pipeline,
TWM005-TWM011 diagnostics replacing generated-code compile errors, trimming/AOT annotations on the reflection
path. State consumes `TimeWarp.Mediator.Contracts`, `.Generators` and `.Analyzers` at `14.0.0-beta.4`.
This is the dogfood pass the maintainer asked for before Mediator `14.0.0` stable.

Also take every other pin in `Directory.Packages.props` to latest per the repo audit policy. At filing time
`ganda nuget outdated` listed Microsoft.CodeAnalysis (4.14.0 → 5.9.0, major) and TimeWarp.Nuru / Nuru.DevCli
(prerelease patch). Mediator beta.5 was pushed but nuget.org had not indexed it yet; re-run `ganda nuget outdated`
when starting.

Filed by Claude (timewarp-mediator cockpit session) at Steven's request.

## Requirements

- All three TimeWarp.Mediator pins at `14.0.0-beta.5`; other pins at latest unless a bump breaks the build
  and the reason is recorded here (Microsoft.CodeAnalysis major: keep the Roslyn components on the floor that
  the shipped analyzers require; record the decision).
- Clean build with zero new TWM diagnostics, or each new TWM diagnostic triaged: fixed in State, or filed on
  timewarp-mediator with a repro when the generator is wrong.
- Full test suite green. Any failure caused by the Mediator bump is filed on timewarp-mediator before this
  task closes.
- `ganda repo audit` clean.

## Checklist

- [ ] `ganda nuget outdated` re-run; Mediator 14.0.0-beta.5 visible
- [ ] Bump `Directory.Packages.props` (Mediator x3 + the rest)
- [ ] Build: no CS errors; TWM005-TWM011 output reviewed and triaged
- [ ] Tests green
- [ ] Breakages from the Mediator bump filed on timewarp-mediator (link issue numbers in Results)
- [ ] Results: before/after versions table, TWM findings, verdict for Mediator 14.0.0 stable readiness
- [ ] `ganda repo audit` clean; PR merged

## Session

- Created: 1013002 (2026-10-13)

## Notes

- Downstream: timewarp-architecture 342 does the same bump and also consumes TimeWarp.State, so a State
  prerelease carrying this bump should go out before 342 lands.
- Mediator issues #63 / #64 / #65 (CS0121, internal request types, CS1591 on generated code) were fixed by
  beta.4 making generated types internal; if State still carries the `NoWarn 1591` or hand-written
  `AddServerPipelineMediator` workarounds, try removing them here and note the outcome.
- Mediator release notes: documentation/release-notes/release-notes-v14.0.0.md in timewarp-mediator.
