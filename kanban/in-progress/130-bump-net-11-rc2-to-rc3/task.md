# Bump .NET 11 RC2 to RC3

## Goal (implementer restatement)

Raise the repo from the .NET 11 RC2 build to the RC3 security build shipped 2026-10-25. Machines and CI that honor `global.json` use SDK `11.0.100-rc.3.26522.148`. Every live pin still on `11.0.0-rc.2.26504.105` moves to `11.0.0-rc.3.26522.148`, including the central Microsoft.AspNetCore.* / Microsoft.Extensions.* / System.Net.Http.Json entries and the analyzer tests' `Microsoft.NETCore.App.Ref` pin. I read the RC3 notes under the 11.0 release-notes tree and fix any break this codebase actually hits. `./bin/dev build`, `./bin/dev test`, `./bin/dev pack`, and `./bin/dev verify-samples` pass on that SDK. Closed kitchens and historical release notes keep the versions they shipped on. The workflow installs the SDK from `global.json` and does not pin a separate channel.

Acceptance:

- `global.json` sdk version is `11.0.100-rc.3.26522.148` (`allowPrerelease` and `rollForward` unchanged).
- `Directory.Packages.props` has no `11.0.0-rc.2.26504.105` entries.
- Analyzer tests pin `Microsoft.NETCore.App.Ref` `11.0.0-rc.3.26522.148`.
- `.github/workflows/workflow.yml` uses `global-json-file: global.json` and has no `dotnet-version`.
- Build, test, pack, and sample verification exit 0. Any RC3 fallout in this repo is fixed.

## Description

.NET 11 RC3 shipped 2026-10-25. Task 120 (PR #656) moved the repo from RC1 to RC2. Move it from RC2 to RC3. The RC3 SDK `11.0.100-rc.3.26522.148` is installed on this machine (confirmed 2026-10-26), and the RC3 packages `11.0.0-rc.3.26522.148` are on nuget.org.

## Requirements

- `global.json` sdk: `11.0.100-rc.2.26504.105` -> `11.0.100-rc.3.26522.148`
- `Directory.Packages.props`: every `11.0.0-rc.2.26504.105` pin -> `11.0.0-rc.3.26522.148`. On master that is the Microsoft.AspNetCore.Components.* set, Mvc.Testing, TestHost, the Microsoft.Extensions.* set, System.Net.Http.Json, and the analyzer `Microsoft.NETCore.App.Ref` pin. Grep for `rc.2` to catch any other.
- Review the RC3 notes under https://github.com/dotnet/core/tree/main/release-notes/11.0 for breaking changes that touch this repo and fix any fallout.
- CI follows `global.json`; confirm the workflow does not pin an SDK channel separately.

## Checklist

- [x] Update `global.json` SDK version
- [x] Update every RC2 package pin to RC3
- [x] Review RC3 breaking changes and fix any fallout
- [x] `./bin/dev build` succeeds
- [x] `./bin/dev test` green
- [x] `./bin/dev pack` and `./bin/dev verify-samples` green

## Session

- Created: 259f62af-98bf-4133-8bba-2f8a32eb7caa (2026-10-26)
- Implementation: 01a17887-3352-7152-a5d2-9f25b22f1402 (2026-10-26). Pinned SDK `11.0.100-rc.3.26522.148`, the ten `11.0.0-rc.2.26504.105` central package entries, and `Microsoft.NETCore.App.Ref` in the analyzer tests. `claude.md` states the same SDK pin. The RC3 note is a security release (Brotli CVE-2026-111497, Linux tarball CVE-2026-114148, and a re-release of the Blazor debugger fix). It names no API break for this repo. The Blazor packages it calls out are the pins above. `ASPDEPR011` on `UseWebAssemblyDebugging` is already in the RC2 assembly, so the sample and test host keep that call. `.github/workflows/workflow.yml` uses `global-json-file: global.json` and has no `dotnet-version`.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-26T14:30:14Z
- Review oracle: claude (claude-opus-5-5), effort 1, roster general, 1 round, disposition clean — 2026-10-26

## Results

SDK `11.0.100-rc.3.26522.148` is the `global.json` floor (`rollForward` latestMinor, `allowPrerelease` true). The ten central package pins and the analyzer `Microsoft.NETCore.App.Ref` pin are `11.0.0-rc.3.26522.148`. `dotnet --version` in this worktree prints that SDK. Packed `TimeWarp.State.Blazor` and `TimeWarp.State.DevTools.Wasm` depend on `Microsoft.AspNetCore.Components.Web` and `Microsoft.AspNetCore.Components.WebAssembly` at `11.0.0-rc.3.26522.148`. CI follows `global.json`. The RC3 release folder has the security note only; there is no separate libraries, runtime, SDK, or ASP.NET Core breaking-change page for RC3. The diff is version pins plus the `claude.md` SDK line.

`./bin/dev build` exited 0 (164 warnings, 0 errors). `./bin/dev test` exited 0. `./bin/dev pack` exited 0 and listed `Microsoft.AspNetCore.Components.Web` on TimeWarp.State.Blazor and TimeWarp.State.Plus, and `Microsoft.AspNetCore.Components.WebAssembly` on TimeWarp.State.DevTools.Wasm. `./bin/dev verify-samples` printed `Samples verified successfully!`. Restore warned `NU1510` that `Microsoft.Extensions.Logging.Abstractions` and `System.Net.Http.Json` are shared-framework packages. Those references stay; the build and the tests still exit 0. `ganda repo audit` exited 0 with two pre-existing warnings: `kebab-path-names` (Blazor initializer names, already `severity = warning`) and `kanban-landed` for task 098.

### Review

1 round at effort 1 with the `general` reviewer. Final counts: bug 0, suggestion 0, nit 0 (all statuses 0). Disposition: **clean**, with no wontfix and no escalations. The reviewer confirmed:

- No live `rc.2` pin is left outside kanban.
- The workflow uses `global-json-file: global.json` and has no `dotnet-version`.
- `./bin/dev build` exits 0.
- The analyzer tests pass 44/44 on SDK `11.0.100-rc.3.26522.148`.

Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

### How to validate

Smoke: From this worktree, run `dotnet --version`, then `./bin/dev build`, `./bin/dev test`, `./bin/dev pack`, and `./bin/dev verify-samples`.

Expect: `dotnet --version` prints `11.0.100-rc.3.26522.148`. Build exits 0. `./bin/dev test` exits 0 with analyzer 44 passed, source generator 90 passed, state 128 passed and 1 skipped, plus 40 passed and 1 skipped, telemetry 21 passed, devtools 101 passed, devtools isolation 4 passed, client integration 64 passed and 1 skipped, architecture 7 passed and 1 skipped. Pack lists Microsoft.AspNetCore.Components.Web on TimeWarp.State.Blazor and TimeWarp.State.Plus, and Microsoft.AspNetCore.Components.WebAssembly on TimeWarp.State.DevTools.Wasm, with no allow-list failure. `./bin/dev verify-samples` prints `Samples verified successfully!`. `rg 11.0.0-rc.2.26504.105` in `global.json`, `Directory.Packages.props`, `claude.md`, and `tests/timewarp-state-analyzer-tests` finds nothing. `.github/workflows/workflow.yml` has `global-json-file: global.json` and no `dotnet-version`.

## Notes

Task 120 found no RC2 fallout and changed only `global.json` and `Directory.Packages.props`. Expect the same shape. Since task 129, restores use the isolated `artifacts/nuget-packages` folder, so the first restore after the bump downloads the RC3 set fresh.
