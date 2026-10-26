# Bump .NET 11 RC1 to RC2

## Goal (implementer restatement)

The repo still builds and restores on the .NET 11 RC1 SDK and the matching RC1 Microsoft packages. This task raises that floor to the RC2 go-live build so every machine and CI job that honors `global.json` uses SDK `11.0.100-rc.2.26504.105`, and every central package pin that is still on `11.0.0-rc.1.26425.128` moves to `11.0.0-rc.2.26504.105`. The analyzer tests' `Microsoft.NETCore.App.Ref` pin moves with those packages. I read the RC2 ASP.NET Core, libraries, runtime, SDK, C#, and MSBuild notes and fix any break this codebase actually hits. The build and the test suite pass on that SDK. Historical release notes and closed task files stay as records of the RC1 ship. RC3 (`11.0.100-rc.3.26522.148`, 2026-10-25) is a later security release and is outside this task: the RC3 SDK is not installed here, and the acceptance versions are the RC2 strings above.

Acceptance:

- `global.json` sdk version is `11.0.100-rc.2.26504.105` (`allowPrerelease` and `rollForward` unchanged).
- `Directory.Packages.props` has no `11.0.0-rc.1.26425.128` entries. AspNetCore.Components.*, Mvc.Testing, TestHost, Extensions.DependencyInjection, Logging.Abstractions, Logging.Configuration, and System.Net.Http.Json are `11.0.0-rc.2.26504.105`.
- Analyzer reference assemblies use `Microsoft.NETCore.App.Ref` `11.0.0-rc.2.26504.105`.
- RC2 breaking changes that this repo calls are fixed. Notes that do not touch this code need no product change.
- `./bin/dev build` succeeds and `./bin/dev test` is green.

## Description

.NET 11 RC2 shipped 2026-10-13 (go-live). Move the repo from RC1 to RC2. The RC2 SDK is installed on Windows, WSL and the box.

## Requirements

- `global.json` sdk: `11.0.100-rc.1.26425.128` -> `11.0.100-rc.2.26504.105`
- `Directory.Packages.props`: Microsoft.AspNetCore.Components.* and Microsoft.AspNetCore.Mvc.Testing `11.0.0-rc.1.26425.128` -> `11.0.0-rc.2.26504.105` (and any other RC1 versions)
- Review ASP.NET Core RC2 breaking-change notes: https://github.com/dotnet/core/tree/main/release-notes/11.0

## Checklist

- [x] Update global.json SDK version
- [x] Update AspNetCore.Components.* and Mvc.Testing package versions
- [x] Review RC2 breaking changes and fix any fallout
- [x] Build succeeds
- [x] Tests green

## Session

- Created: 2026-10-14
- Implementation: 2026-10-26. Pinned SDK `11.0.100-rc.2.26504.105` and the ten `11.0.0-rc.1.26425.128` package entries (AspNetCore.Components.*, Mvc.Testing, TestHost, Extensions.DependencyInjection, Logging.Abstractions, Logging.Configuration, System.Net.Http.Json) plus `Microsoft.NETCore.App.Ref` in the analyzer tests. `claude.md` states the same SDK pin. Historical release notes and closed kitchens still name the RC1 build they shipped on.
- RC2 notes (ASP.NET Core, libraries, runtime, SDK, C#, MSBuild, NuGet) do not name an API this repo calls. No product code change beyond the pins. `UseWebAssemblyDebugging` warns ASPDEPR011 and NU1510 warns that framework packages are not pruned. Both are warnings, the RC2 notes do not require removing them, and build, pack, and tests stayed green.
- `.gitignore` keeps the `stacked-task-set*.resume.json` line that was already in the worktree. `required-gitignore` and `kanban-porcelain` pass with it present.
- `ganda repo audit` exits 0 (31 passed, 2 advisory warnings). Left in place: `kanban-landed` for task 098 (no worktree on this claim) and `kebab-path-names` for Blazor `*.lib.module.*` filenames, `TimeWarp.State.DevTools.App.targets`, and backlog folder 110. Renaming those on this id would break framework names or another kitchen.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-26T06:30:19Z
- Review oracle: general reviewer (claude, model claude-opus-5-5), effort 1, session not reported, turn cap 80. Round 1 raised 0 findings. Disposition: clean (2026-10-26).

## Results

SDK `11.0.100-rc.2.26504.105` is the `global.json` floor (`rollForward` latestMinor, `allowPrerelease` true). Every former RC1 central package pin and the analyzer `Microsoft.NETCore.App.Ref` pin are `11.0.0-rc.2.26504.105`. `dotnet --version` in this worktree prints that SDK. RC3 (`11.0.100-rc.3.26522.148`, 2026-10-25) is published and is not installed here, so this task stays on the RC2 versions in the requirements.

RC2 review, no code fallout: QuickGrid experimental APIs, `IValidationMessageFormatter`, `BasePath` namespace move, device-bound sessions, `AddRazorRuntimeCompilation` obsoletion, iOS file locking, MemoryCache metric rename, and the SDK/MSBuild/C#/NuGet notes do not match any call site here.

### Review

- Rounds: 1. Roster: general (effort 1).
- Final counts: 0 bug, 0 suggestion, 0 nit. 0 open, 0 fixed, 0 wontfix.
- Disposition: **clean**. The reviewer re-ran `./bin/dev build` (exit 0) and `./bin/dev test` (exit 0) on SDK `11.0.100-rc.2.26504.105`, and confirmed that CI reads `global.json`.
- Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke: from this worktree, `dotnet --version`, then `./bin/dev build`, `./bin/dev test`, `./bin/dev pack`, and `./bin/dev verify-samples`.

Expect: `dotnet --version` prints `11.0.100-rc.2.26504.105`. Build exits 0. `./bin/dev test` exits 0 with analyzer 44 passed, source generator 90 passed, state 120 passed and 1 skipped, plus 40 passed and 1 skipped, telemetry 21 passed, devtools 101 passed, devtools isolation 4 passed, client integration 64 passed and 1 skipped, architecture 7 passed and 1 skipped. Pack lists Microsoft.AspNetCore.Components.Web on TimeWarp.State.Blazor and TimeWarp.State.Plus, and Microsoft.AspNetCore.Components.WebAssembly on TimeWarp.State.DevTools.Wasm, with no allow-list failure. `./bin/dev verify-samples` prints `Samples verified successfully!`. `rg 11.0.0-rc.1.26425.128` in `global.json`, `Directory.Packages.props`, and `tests/timewarp-state-analyzer-tests` finds nothing.
