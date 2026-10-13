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

- [x] `ganda nuget outdated` re-run; Mediator 14.0.0-beta.5 visible
- [x] Bump `Directory.Packages.props` (Mediator x3 + the rest)
- [x] Build: no CS errors; TWM005-TWM011 output reviewed and triaged
- [x] Tests green
- [x] Breakages from the Mediator bump filed on timewarp-mediator (link issue numbers in Results)
- [x] Results: before/after versions table, TWM findings, verdict for Mediator 14.0.0 stable readiness
- [x] `ganda repo audit` clean
- [ ] PR merged

## Results

`ganda nuget outdated` on 2026-10-13 listed four outdated pins. Mediator `14.0.0-beta.5` was on nuget.org. TimeWarp.Nuru and TimeWarp.Nuru.DevCli were already `3.0.0-beta.80`.

| Package | Before | After |
| --- | --- | --- |
| TimeWarp.Mediator.Contracts | 14.0.0-beta.4 | 14.0.0-beta.5 |
| TimeWarp.Mediator.Generators | 14.0.0-beta.4 | 14.0.0-beta.5 |
| TimeWarp.Mediator.Analyzers | 14.0.0-beta.4 | 14.0.0-beta.5 |
| Microsoft.CodeAnalysis.CSharp | 4.14.0 | 4.14.0 (held) |

Every other central pin was already current. `Blazored.LocalStorage` 4.5.0 and `Blazored.SessionStorage` 2.4.0 stay: the outdated tool reports both as not found, and the LocalStorage GitHub release `v4.5.0` is still the latest published release.

### Roslyn floor

`Microsoft.CodeAnalysis.CSharp` stays `4.14.0`. The outdated tool offers `5.9.0` (major). The analyzer and source generator compile against this pin and ship inside the TimeWarp.State packages, so the pin is the consumer compiler floor. `5.9.0` would raise that floor. Mediator's generator requires Roslyn 4.8 or later, and `4.14.0` is above that. `NuspecDependencyAllowList_Should_.PinRoslynAtTheConsumerCompilerFloor` locks the pin. `Microsoft.CodeAnalysis.Analyzers` stays `5.9.0` (already latest).

### TWM findings

`./bin/dev build` (24 projects, samples omitted), `./bin/dev verify-samples` (11 sample projects), and the sample-05 trimmed publish reported zero `TWM001`–`TWM011`. Nothing to fix in State and nothing new to file on timewarp-mediator.

### Workarounds (#63, #64, #65)

Emitted `Test.App.Server` mediator types are `internal`, including `GeneratedMediatorServiceCollectionExtensions`. The server host calls `AddGeneratedMediator<ServerPipeline>()`. `AddServerPipelineMediator` and `server-pipeline-registration.cs` are removed. Test.App.Client does not grant `InternalsVisibleTo` to the server, so the client's internal extension is out of scope and the call compiles.

`Client.Integration.Tests` keeps `Aliases=TestAppServer`. Both hosts grant `InternalsVisibleTo` to that assembly, so both internal `AddGeneratedMediator<TScope>` methods are visible. Dropping the alias reproduces CS0121. That matches the 14.0.0-beta.4 release note (a friend of two hosts still sees duplicates) and the still-open [timewarp-mediator#63](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/63). No new issue.

`.editorconfig` sets `dotnet_diagnostic.CS1591.severity = none`. `<NoWarn>CS1591</NoWarn>` is removed from `test-app-server`; that rebuild reported no CS1591. Generated types are internal, so they are not public API for CS1591. Other projects still list `NoWarn` 1591; those lines are redundant with the editorconfig severity. [timewarp-mediator#65](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/65) stays open upstream.

[timewarp-mediator#64](https://github.com/TimeWarpEngineering/timewarp-mediator/issues/64) asked for internal generated types so host messages can stay internal. The generator emits internal types. This pass left sample action types public.

### Stable-readiness verdict

State's dogfood of `14.0.0-beta.5` is green. State references Contracts, Generators, and Analyzers. The runtime package's smaller public API is outside this compile. The contracts asset is `net10.0`, which this `net11.0` repo consumes. From this consumer, Mediator `14.0.0` stable is ready, with #63 still open for a friend assembly of two hosts. Ship a State prerelease that carries this pin before timewarp-architecture 342.

`verify-samples` published sample 05. The log said publishing without `wasm-tools` optimizations, so the IL linker did not run. Generated hosts call `AddGeneratedMediator`, which is the path the release notes leave free of the new reflection trim annotations.

`ganda repo audit` exits 0. Advisory `kebab-path-names` lists 6 pre-existing paths (Blazor `*.lib.module.js` names and kanban 110's folder).

### How to validate

**Smoke:**

```bash
./bin/dev build
./bin/dev test
./bin/dev pack && ./bin/dev verify-samples
./bin/dev e2e
ganda repo audit
```

**Expect:**

- `build` exits 0 with 0 errors and no `TWM` diagnostics.
- `test` exits 0: analyzer 44 passed; source generator 90 passed; state 120 passed and 1 skipped; plus 40 passed and 1 skipped; telemetry 21 passed; devtools 60 passed; devtools isolation 4 passed; client integration 64 passed and 1 skipped; architecture 7 passed and 1 skipped.
- `pack` prints `Packed set verified`. `verify-samples` prints `Samples verified successfully!` for 11 projects.
- `e2e` exits 0: Failed 0, Passed 12, Total 12. The weather API path runs through `AddGeneratedMediator<ServerPipeline>()`.
- `ganda repo audit` exits 0. The advisory is `kebab-path-names` with the same 6 pre-existing paths.

## Session

- Created: 1013002 (2026-10-13)
- Implementation: implementer oracle (2026-10-13)

## Notes

- Downstream: timewarp-architecture 342 does the same bump and also consumes TimeWarp.State, so a State
  prerelease carrying this bump should go out before 342 lands.
- Mediator issues #63 / #64 / #65 (CS0121, internal request types, CS1591 on generated code) were fixed by
  beta.4 making generated types internal; if State still carries the `NoWarn 1591` or hand-written
  `AddServerPipelineMediator` workarounds, try removing them here and note the outcome.
- Mediator release notes: documentation/release-notes/release-notes-v14.0.0.md in timewarp-mediator.
