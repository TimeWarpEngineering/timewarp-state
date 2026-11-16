# Bring package pins current so the merge audit passes

## Description

`ganda pr merge` refuses every PR in this repo because the `package-pin-currency` audit check
fails on master. PR #685 (task 154) had to be merged with `--skip-audit` on 2026-11-16 on
Steven's instruction, with this update to follow.

Blocking (2 of 13 pins fail):

- `TimeWarp.Mediator` 15.0.0-beta.5 → latest 15.0.0-beta.9 (library reference in
  `source/timewarp-state-devtools/timewarp-state-devtools.csproj`, no `PrivateAssets=all`)
- `TimeWarp.Mediator.Generators` 15.0.0-beta.5 → latest 15.0.0-beta.9

Behind but not yet blocking (reported as current at the last meaningful commit, 2026-11-11):

- `TimeWarp.Amuru` / `TimeWarp.Amuru.Tools` 3.0.0-beta.2 → 3.0.0-beta.5
- `TimeWarp.Nuru` / `TimeWarp.Nuru.DevCli` 4.0.0-beta.12 → 4.0.0-beta.14
- `TimeWarp.SourceGenerators` 1.0.0-beta.13 → 1.0.0-beta.16
- `TimeWarp.Terminal` 2.0.0-beta.1 → 2.0.0-beta.2
- `TimeWarp.Build.Tasks` 1.0.0 → 2.0.1-beta.1 (major bump)

Any commit to this repo makes the "last meaningful commit" newer, so the non-blocking pins are
likely to start failing as soon as this task's own PR is audited. Bring them all current here.

## Requirements

1. Run `ganda repo audit --fix` first and commit what it changes (audit fixes are policy).
2. Bump both Mediator pins in `Directory.Packages.props` to the latest release and fix whatever
   breaks. Four beta versions is enough for API or generator changes; known Mediator generator
   gotchas from the 14 line: `order:` is a constructor argument, the pipeline delegate parameter
   must be named `next`, and request types must be public. Check the Mediator release notes for
   beta.6 through beta.9 before guessing.
3. Bump the remaining behind pins listed above. Amuru's exit-code behavior has changed across
   betas before (RunAsync returning codes vs throwing); verify the dev CLI and scripts still
   behave, do not trust old comments.
4. `TimeWarp.Build.Tasks` 1.0.0 → 2.0.1-beta.1 is a major bump. Attempt it; if it needs more than
   mechanical changes, leave it at 1.0.0, say so in Results, and do not exempt it silently.
5. TimeWarp packages come from https://nuget.pkg.github.com/TimeWarpEngineering/index.json as well
   as nuget.org; use whichever feed the audit names for each package.
6. Gates: `./bin/dev build` with 0 warnings and 0 errors; unit tests green; `ganda repo audit`
   with `package-pin-currency` passing. If `./bin/dev test` reports fixie needs a tool restore,
   that is the stale `~/.dotnet/toolResolverCache`; say so rather than working around it.
7. `--no-incremental` builds delete tracked `wwwroot` JS in `timewarp-state-blazor`; rebuild to
   regenerate and check `git status` before finishing.
8. Bump the State package version if a packed dependency changed (Mediator is a packed dependency
   of State; `dev check-version` must pass).

## Goal (implementer restatement)

Make `ganda repo audit` pass `package-pin-currency` on this branch so `ganda pr merge` stops
refusing PRs in this repo. Acceptance:

- `ganda repo audit --fix` output committed first.
- Mediator and Mediator.Generators at the latest beta; any API/generator breakage fixed.
- Amuru(+Tools), Nuru(+DevCli), SourceGenerators, Terminal at their latest versions; dev CLI
  still behaves under the new Amuru.
- Build.Tasks taken to 2.x if mechanical, else left at 1.0.0 and reported (not silently exempted).
- State package version bumped because Mediator (a packed dependency) changes; `dev check-version` passes.
- `./bin/dev build` 0 warnings / 0 errors, `./bin/dev test` green, tracked `wwwroot` JS intact,
  `ganda repo audit` green including `package-pin-currency`.

## Checklist

- [x] `ganda repo audit --fix` run and committed
- [x] Mediator + Mediator.Generators at latest; code compiles and tests pass
- [x] Amuru, Nuru, SourceGenerators, Terminal pins at latest
- [x] Build.Tasks major bump done, or reported as deferred with the reason
- [x] State version bumped if a packed dependency changed
- [x] Build clean, tests green, tracked wwwroot JS intact
- [x] `ganda repo audit`: `package-pin-currency` passes

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-16)
- Implemented 2026-11-16 (implement oracle, ganda task work): audit fix, pins, .NET 11 GA + Aspire advisories, beta.18 bump.
- Implement oracle: implemented by implementer-claude (claude, vendor anthropic, model claude-opus-5-5), session not reported, max-turns 500, Done — 2026-11-16T04:42:30Z
- Review round 1: grok session 01a1e2aa-135a-74c2-8d23-9096aed2a212 (2026-11-16), effort 2, roster general. Open finding M1. No disposition yet.
- Review oracle: review by reviewer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1e2aa-135a-74c2-8d23-9096aed2a212, max-turns 120, artifacts general.md, merged.md — 2026-11-16T04:52:51Z
- Fix pass for round 1 (implement-fix-2 oracle, implementer-claude, claude-opus-5-5), 2026-11-16: M1 fixed. Five current-version sentences now name Mediator 15.0.0-beta.9.

## Results

**Summary.** All TimeWarp pins are current. `package-pin-currency` passes, and so does the rest of the audit. Fixing the gates took more than the pins:

- **Audit fix (committed first, b7a570a9).** `ganda repo audit --fix` raised every behind TimeWarp pin to its latest version: Amuru/Amuru.Tools 3.0.0-beta.5, Build.Tasks 2.0.1-beta.1, Mediator/Mediator.Generators 15.0.0-beta.9, Nuru/Nuru.DevCli 4.0.0-beta.14, SourceGenerators 1.0.0-beta.16, Terminal 2.0.0-beta.2. It also refreshed the `.githooks/*.cs` Purpose regions and added `.githooks/prepare-commit-msg(.cs)`. The old comment that named the Amuru pin in `Directory.Packages.props` was replaced.
- **Mediator 15.0.0-beta.9.** The release notes for beta.6, beta.8 and beta.9 list no API changes. beta.8 moved to .NET 11 GA packages, and beta.9 builds its generator and analyzer for net11.0 only. Everything compiled and every test passed without code changes.
- **New NuGet advisories (NU1902/NU1903 as errors).** `Microsoft.Extensions.Validation` 11.0.0-rc.3 (GHSA-m28h-pj36-4364) and `Aspire.Hosting` 13.6.4 (GHSA-f7vh-h5g3-7jrh) broke restore. All `11.0.0-rc.3.26522.148` pins moved to .NET 11 GA `11.0.0`. That includes the `Microsoft.NETCore.App.Ref` reference pack in `analyzer-test-factory.cs`. Aspire moved to 13.6.5, which covers the central pins, the sample-04 `Aspire.AppHost.Sdk`, and the packaged DevTools consumer test. `global.json` now pins SDK `11.0.100` with `allowPrerelease: false`, as `claude.md` planned for GA.
- **SourceGenerators 1.0.0-beta.16 Roslyn collision.** This analyzer-only package now declares `Microsoft.CodeAnalysis.CSharp >= 5.12.0` as a net11.0 dependency (beta.13 declared 5.9.0). That raised NU1605 downgrade errors in `timewarp-state-analyzer-tests` and `timewarp-state-source-generator-tests`, which deliberately test on the 5.9.0 consumer floor. The shipped floor stays 5.9.0, and the `nuspec-dependency-allow-list` test still asserts it. Those two test projects now `Remove` the lint-only `TimeWarp.SourceGenerators` reference (commented in each csproj). The cost: TW0007/TWA0004 lint no longer runs on those two test projects. Follow-up candidate: timewarp-source-generators should stop declaring Roslyn as a package dependency of an analyzer-only package.
- **Build.Tasks 1.0.0 → 2.0.1-beta.1: done, mechanical.** 2.x requires the .NET 11 `dotnet` CLI as the build host, and that is what this repo uses. Generated AssemblyInfo still stamps `CommitHash` and `CommitDate`.
- **Amuru 3.0.0-beta.5.** The dev CLI compiled (pre-commit self-install) and propagates exit codes. A failing fixie step made `./bin/dev test` exit 1, and the green run exits 0. Hooks ran on commit.
- **Version.** `12.0.0-beta.17` is already published, and Mediator is a packed dependency. `TimeWarpStateVersion` and `source/Directory.Build.props` `<Version>` moved to `12.0.0-beta.18`. `./bin/dev check-version` reports "Version in source is new". The dependency updates were added to the unreleased `release12.0.0-beta.18.md`.
- **Stale tool cache (machine state, not repo).** The first `./bin/dev test` failed with `Run "dotnet tool restore" to make the "fixie" command available`, and `dotnet tool restore` did not fix it. The cause was the stale `~/.dotnet/toolResolverCache`. Clearing it (`rm -rf ~/.dotnet/toolResolverCache`) fixed it. No repo workaround was added.
- **Tracked wwwroot JS.** `git status` after build and test shows no deleted or modified `wwwroot` files.
- **Audit advisory left as is.** The `kebab-path-names` warning (7 paths: Blazor `{AssemblyName}.lib.module.*`, `build/{PackageId}.targets`, backlog 110) is the documented task-154 decision in `.editorconfig` and does not block.

- **Docs (review round 1, M1).** The current-version sentences in `readme.md:26` (packed into the State nupkg), `claude.md:80`, `claude.md:170`, `documentation/overview.md:44` and `documentation/partials/summary.md:10` now say TimeWarp.Mediator 15.0.0-beta.9, which matches the nuspec dependency. These were left alone on purpose: the "15.0.0-beta.5 or later" feature floors (`documentation/topics/actions.md`, `documentation/topics/analyzers.md`, the comments in `reducer-attribute.cs` and `reducer-handler-emitter.cs`), the release notes, and the `nuspec-dependency-allow-list-tests.cs` fixture XML.

### Gates

Fix pass (round 1 → M1), rerun after the docs edit:

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | Build succeeded, 0 Warning(s), 0 Error(s) |
| Test | `./bin/dev test` | Tests completed successfully (exit 0): analyzer 90, source-generator 174, state 153 (+1 skipped), 40 (+1), telemetry 21, devtools 124, devtools isolation 4, client integration 67 (+1), architecture 7 (+1) |
| Audit | `ganda repo audit` | exit 0, "Repository passes — 1 advisory (non-blocking) warning(s)", 42 passed; only advisory is `kebab-path-names` (pre-existing, task 154) |

Implement pass:

| Gate | Command | Result |
|------|---------|--------|
| Build | `./bin/dev build` | Build succeeded, 0 Warning(s), 0 Error(s) |
| Test | `./bin/dev test` | Tests completed successfully (exit 0): source-generator 174, state 153 (+1 skipped), plus 40 (+1), telemetry 21, devtools 124, devtools isolation 4, client integration 67 (+1), architecture 7 (+1), analyzer 90 |
| Audit | `ganda repo audit` | exit 0, "Repository passes — 1 advisory (non-blocking) warning(s)"; `package-pin-currency` PASS; only advisory is `kebab-path-names` (pre-existing, task 154) |
| Version | `./bin/dev check-version` | Version in source is new (12.0.0-beta.18) |

### How to validate

**Smoke:**

```bash
./bin/dev build && ./bin/dev test && ganda repo audit && ./bin/dev check-version
grep -n 'TimeWarp\.' Directory.Packages.props
git status --short source/timewarp-state-blazor/wwwroot
git grep -n "Mediator 15.0.0-beta" readme.md claude.md documentation/overview.md documentation/partials/summary.md
```

**Expect:** build 0 warnings and 0 errors. Tests completed successfully. The audit exits 0 with `package-pin-currency` PASS, and its only advisory is `kebab-path-names`. check-version reports 12.0.0-beta.18 as new. Mediator pins read 15.0.0-beta.9, Amuru 3.0.0-beta.5, Nuru 4.0.0-beta.14, SourceGenerators 1.0.0-beta.16, Terminal 2.0.0-beta.2, Build.Tasks 2.0.1-beta.1. `wwwroot` git status is empty. The last grep shows five lines, and every one says 15.0.0-beta.9. If fixie asks for `dotnet tool restore`, clear `~/.dotnet/toolResolverCache`; that is machine state.

### Review

Round 1 (effort 2, roster general). Counts after the fix pass: 0 bugs open, 1 fixed, 0 wontfix. M1 (the packed readme said hosts consume Mediator 15.0.0-beta.5) is fixed. The five current-version sentences now say 15.0.0-beta.9. Review round 2 is next. Paths: `review/review-framework.md`, `review/round-1/merged.md`. The implementer restatement matches the original task. No misread.

## Review

- Round 1: M1 fixed in the fix pass (2026-11-16); round 2 pending

## Notes

Requested by Steven 2026-11-16: "skip audit and then we update".
