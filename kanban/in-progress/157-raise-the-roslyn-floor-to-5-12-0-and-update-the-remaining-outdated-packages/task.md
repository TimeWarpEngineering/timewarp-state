# Raise the Roslyn floor to 5.12.0 and update the remaining outdated packages

## Description

`ganda nuget outdated` on master (2026-11-16) reports 10 outdated packages. Bring them all current.

| Package | Current | Latest |
|---|---|---|
| Microsoft.CodeAnalysis.Analyzers | 5.9.0 | 5.12.0 |
| Microsoft.CodeAnalysis.CSharp | 5.9.0 | 5.12.0 |
| Microsoft.CodeAnalysis.CSharp.Workspaces | 5.9.0 | 5.12.0 |
| OpenTelemetry | 1.20.1 | 1.21.0 |
| OpenTelemetry.Extensions.Hosting | 1.20.1 | 1.21.0 |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.20.1 | 1.21.0 |
| Microsoft.NET.Test.Sdk | 18.11.0 | 18.12.0 |
| MSTest.TestAdapter | 4.5.2 | 4.5.3 |
| MSTest.TestFramework | 4.5.2 | 4.5.3 |
| coverlet.collector | 10.1.0 | 10.1.1 |

## Decision: raise the Roslyn floor (Steve, 2026-11-16)

Roslyn was pinned at 5.9.0 as the consumer compiler floor (task 151): the analyzer, code fixes, and
source generator ship in the packages and run in the consumer's compiler, so the Roslyn they
compile against sets the minimum SDK a consumer needs.

That floor no longer protects anyone. Since task 156 the repo is on .NET 11 GA (`global.json`
SDK 11.0.100) and the library projects target `net11.0`, so a consumer must already be on a
.NET 11 SDK. SDK 11.0.100 bundles Roslyn 5.12.0 (verified from the installed SDK's
`Roslyn/bincore/Microsoft.CodeAnalysis.CSharp.dll`: `5.12.0-1.26529.118`; SDK 10.0.404 bundles
5.9.0). Accepted cost: .NET 11 preview / RC SDKs that bundle an older Roslyn are no longer
supported. Do not re-deliberate the keep path.

Raising the floor also undoes a workaround from task 156: `TimeWarp.SourceGenerators` beta.16
requires Roslyn 5.12, so the two analyzer test projects held at 5.9 dropped their lint-only
SourceGenerators reference and lost TW0007 / TWA0004.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes (audit fixes are policy).
2. Bump the three Roslyn packages to 5.12.0 in `Directory.Packages.props`; keep them equal.
3. Update the pin comment in `Directory.Packages.props` to the new floor and reason
   (5.12.0 loads on SDK 11.0.100+; state which SDK that is, verified, not copied from here if you
   can check it).
4. Update the guard test `PinRoslynAtTheConsumerCompilerFloor` in
   `tests/timewarp-state-tests/packaging/nuspec-dependency-allow-list-tests.cs` to the new literal
   version and reason. Keep the guard; do not delete it.
5. Restore the `TimeWarp.SourceGenerators` reference on the two analyzer test projects that task
   156 removed it from, so TW0007 and TWA0004 run on them again. Find them from the task 156 diff
   (merge commit `36e0194d`) and remove any comment that explains the removal. Fix any lint
   findings that surface.
6. Confirm the analyzer, code-fix, and source-generator projects (netstandard2.0) still build and
   that Roslyn stays `PrivateAssets` and does not become a package dependency (task 105).
7. Bump the remaining seven packages in the table. `OpenTelemetry` is a library reference, so it
   is a packed dependency change; the other two OpenTelemetry packages are sample-only.
8. Document the new minimum wherever the supported SDK is stated (`documentation/`, readme,
   `claude.md`, overview, release notes for the unreleased version). Search for `5.9` and
   `10.0.400` so no stale floor statement survives, including in the packed readme.
9. Version: 12.0.0-beta.18 is on master but not released. Check `dev check-version`; bump only if
   it requires it, and add these changes to the unreleased release notes.
10. Gates: `./bin/dev build` 0 warnings / 0 errors; unit tests green; `ganda repo audit` passing
    including `package-pin-currency`; `ganda nuget outdated` reports 0 outdated (the two
    `Blazored.*` "not found on nuget.org" lines are pre-existing; leave them and mention them).
11. `--no-incremental` builds delete tracked `wwwroot` JS in `timewarp-state-blazor`; rebuild to
    regenerate and check `git status` before finishing. If fixie asks for a tool restore, that is
    the stale `~/.dotnet/toolResolverCache`; report it rather than working around it in the repo.

## Checklist

- [ ] `ganda repo audit --fix` run and committed
- [ ] Roslyn ×3 at 5.12.0; pin comment and guard test updated
- [ ] SourceGenerators lint restored on the two analyzer test projects
- [ ] Roslyn still not a package dependency
- [ ] OpenTelemetry ×3, Test.Sdk, MSTest ×2, coverlet bumped
- [ ] Docs and release notes state the new floor; no stale 5.9 / 10.0.400 statements
- [ ] Build clean, tests green, tracked wwwroot JS intact
- [ ] `ganda repo audit` passes; `ganda nuget outdated` reports 0 outdated

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-16)

## Notes

Requested by Steven 2026-11-16 after `ganda nuget outdated`: asked why Roslyn was held, pointed
out the repo is on .NET 11, then "create task and launch it".
