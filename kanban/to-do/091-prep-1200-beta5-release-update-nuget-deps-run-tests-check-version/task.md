# Prep 12.0.0-beta.5 release: update NuGet deps, run tests, check-version

## Description

Prepare timewarp-state for its next NuGet release. `source/Directory.Build.props`
carries `12.0.0-beta.5`; the newest tag is `v12.0.0-beta.3` (2026-09-17) and
neither beta.4 nor beta.5 has been published for any of the four packages
(TimeWarp.State, .Plus, .Policies, .Telemetry). Master is 121 commits ahead of
that tag, including the Telemetry OTel pipeline, render-suppression and
Store race fixes, the Subscriptions lock for Blazor Server, and the TW0002
analyzer. Before cutting, bring every NuGet dependency to latest, prove the
full pipeline is green, and confirm `dev check-version` accepts the version.
Open the PR; the cockpit cuts the release with `dev release` after merge.

## Checklist

- [x] Run `ganda nuget outdated` in the claim worktree; record the report in Notes
- [x] Run `ganda nuget outdated --update --force` so every package is on latest; review the diff for major bumps that need code changes
- [x] `dotnet run --file tools/dev-cli/dev.cs -- workflow` (assert-version-ssot → clean → build → test → e2e → pack → verify-samples) green locally; record test totals (failed == 0, total > 0)
- [x] `dotnet run --file tools/dev-cli/dev.cs -- check-version` passes for all four packages at 12.0.0-beta.5; if it refuses, bump `<Version>` as it directs and re-run
- [x] `.gitignore` covers `.memsearch/` at the top level (the master checkout has an untracked `.memsearch/` that dirties `dev release`'s clean-tree guard); keep the existing `.memsearch/memory/` promote exception
- [x] `ganda repo audit` clean
- [x] `dotnet run --file tools/dev-cli/dev.cs -- release --dry-run` from the task branch fails only the on-master guard
- [x] Commit with conventional message; PR open with the outdated report and test totals in the body

## Session

- Created: 2026-09-23 (cockpit dispatch)

## Notes

- Do not tag or run `dev release` on this task. The PR is the deliverable; the cockpit releases after merge.
- All four packages ship together at the single props version (org convention, nuru task 458). Never add a `<Version>` to a csproj.
- If a dependency bump breaks build, test, or e2e, fix forward here and note it; do not pin back without saying why.
- Cross-check: amuru task 115 (2026-09-23) is the same prep shape and is a good reference for the PR body.

### `ganda nuget outdated` report (before update, reconstructed from `git diff Directory.Packages.props`)

The dependency update (`ganda nuget outdated --update --force`) had already run in the worktree
before this implement pass started (a prior implement attempt failed after 4m — see
`task-work.progress.log`). The pre-update baseline versus what shipped:

| Package | Before | After |
|---|---|---|
| TimeWarp.Amuru | 1.0.0 | 1.1.1 |
| TimeWarp.Amuru.Tools | 1.0.0-beta.2 | 1.1.1 |
| TimeWarp.Terminal | 1.0.1 | 1.0.2 |
| timewarp-heroicons | 2.0.19 | 2.2.0 |
| Microsoft.AspNetCore.* (Components.Web/WebAssembly/WebAssembly.DevServer/WebAssembly.Server/Mvc.Testing/TestHost) | 10.0.9 | 10.0.12 |
| Microsoft.CodeAnalysis.Analyzers | 4.14.0 | 5.9.0 |
| Microsoft.Extensions.Logging.Abstractions/Configuration | 10.0.9 | 10.0.12 |
| Microsoft.NET.Test.Sdk | 18.0.0 | 18.10.1 |
| Microsoft.Playwright.MSTest | 1.55.0 | 1.62.0 |
| Microsoft.TypeScript.MSBuild | 5.9.3 | 7.0.1 |
| FakeItEasy | 8.3.0 | 9.0.1 |
| Fixie.TestAdapter | 4.1.0 | 4.2.0 |
| coverlet.collector | 6.0.4 | 10.0.1 |
| OpenTelemetry.Extensions.Hosting / Exporter.OpenTelemetryProtocol | 1.18.0 | 1.19.1 |
| OpenTelemetry.Instrumentation.AspNetCore | 1.18.0 | 1.19.0 |
| JetBrains.Annotations | 2025.2.2 | 2026.2.0 |
| System.Net.Http.Json | 9.0.9 | 10.0.12 |

**Deliberately held back** (post-update `ganda nuget outdated` still reports these 3, both documented
inline in `Directory.Packages.props`):

- `MSTest.TestAdapter` / `MSTest.TestFramework` 3.11.1 → 4.4.1 available: MSTest 4 renames the
  `TestFramework` assembly, which `Microsoft.Playwright.MSTest` (through at least 1.62.0) doesn't
  bind to, so every `PageTest`/`BrowserTest` subclass fails to load and E2E silently discovers zero
  tests. Same finding as prior memory note `playwright-mstest-needs-mstest-3x`.
- `Microsoft.CodeAnalysis.CSharp` 4.14.0 → 5.9.0 available: this is the compiler floor for the
  TW analyzer + source generator; 5.9.0 would raise the minimum SDK to 10.0.400+, while 4.14.0 loads
  on every .NET 10 SDK. `Microsoft.CodeAnalysis.Analyzers` (a `PrivateAssets=all` build-time-only
  dependency) was still bumped to 5.9.0 since it carries no such floor constraint.

No code changes were needed for any of the major bumps that did apply — TypeScript 7.0.1 required a
`tsconfig.json` change (`moduleResolution` `"node"` → `"bundler"`, the mode TS 7 expects); the
`wwwroot/js/*.js`/`*.js.map` diffs are the recompiled output of that change, not manual edits.

### Test totals (`dev workflow` — assert-version-ssot → clean → build → test → e2e → pack → verify-samples)

- Unit tests (7 Fixie assemblies): 170 passed, 4 skipped, **0 failed** (19+4+52+31+13+44+7 passed).
- E2E (Playwright/MSTest, `test-app-end-to-end-tests`): 10 passed, 3 skipped, **0 failed**, Total: 13.
- Combined: 180 passed, 7 skipped, 0 failed, 187 total.
- Pack + verify-samples: `Pipeline SUCCEEDED`.
- E2E required installing Playwright's Chromium browser locally (`pwsh
  tests/test-app-end-to-end-tests/bin/Debug/net10.0/playwright.ps1 install chromium`) — the
  `--with-deps` variant needs interactive `sudo` and isn't available in this environment, but the
  browser download alone was sufficient since OS deps were already present.

### `ganda repo audit`

23 passed / 5 failed / 1 skipped — identical failure set confirmed on `master` itself (ran the same
audit there for comparison): `bin-dev` (bin/dev not generated in this environment), `dev-cli-capabilities`
(depends on bin-dev), `kebab-path-names` (pre-existing `Test.App.Client.lib.module.js` name),
`memsearch-scaffold` (hooks/toml not installed in this worktree), `vscode-window-icon` (no
`peacock.color`). None relate to the NuGet update; out of scope for this task. Fixed the two checks
this task's checklist targeted: `kanban-porcelain-gitignore` and `memsearch-memory-gitignore`, both
now PASS after covering `.memsearch/` at the gitignore root (kept the literal `.memsearch/memory/`
line too — the audit's `memsearch-memory-gitignore` check greps for that exact pattern, and `git add
-f` still overrides an ignored directory for the promote flow).

## Results

Updated every NuGet dependency to latest, holding back only the two majors documented inline in
`Directory.Packages.props` (MSTest 4 breaks Playwright.MSTest discovery; Microsoft.CodeAnalysis.CSharp
5.9.0 raises the SDK floor past the analyzer/source-generator's target). The TypeScript 7.0.1 bump
needed one `tsconfig.json` change (`moduleResolution`: `"node"` → `"bundler"`) plus the resulting
recompiled `wwwroot/js` output — no other code changes were required by any dependency bump.
Full `dev workflow` (assert-version-ssot → clean → build → test → e2e → pack → verify-samples) is
green: 180 passed / 7 skipped / 0 failed across unit + E2E, pack succeeded, sample verification
succeeded. `check-version` accepts `12.0.0-beta.5` for all four packages without a bump. Closed the
`.memsearch/` gitignore gap so `dev release`'s clean-tree guard won't be dirtied by the untracked
memsearch index on a fresh checkout; `dev release --dry-run` now fails only on the expected
not-on-master guard. `ganda repo audit` failures are the same 5 found on `master` itself (verified by
running the audit there) — pre-existing environment/repo state unrelated to this dependency bump, left
alone as out of scope. Committed as `2995e1f2`.

### How to validate

**Smoke:**
```
cd <task worktree>
git log --oneline -1                                    # 2995e1f2 chore(deps): update NuGet dependencies...
dotnet run --file tools/dev-cli/dev.cs -- check-version  # accepts 12.0.0-beta.5 for all 4 packages
dotnet run --file tools/dev-cli/dev.cs -- release --dry-run
```

**Expect:**
- `check-version` prints `✓ Version in source is new — safe to release.`
- `release --dry-run` prints `✓ working tree clean` then fails with
  `release must be cut from master — current branch is task/091-...` (only that guard trips).
- `ganda nuget outdated` shows only the 3 deliberately-held majors (MSTest.TestAdapter,
  MSTest.TestFramework, Microsoft.CodeAnalysis.CSharp), each with an inline comment in
  `Directory.Packages.props` explaining why.
- `ganda repo audit` reports the same 5 pre-existing failures as `master` (bin-dev, dev-cli-capabilities,
  kebab-path-names, memsearch-scaffold, vscode-window-icon) and no new ones; `kanban-porcelain-gitignore`
  and `memsearch-memory-gitignore` both PASS.
- Full `dotnet run --file tools/dev-cli/dev.cs -- workflow` ends with `Pipeline SUCCEEDED` and reports
  0 test failures (170 unit + 10 E2E passed, 4 + 3 skipped). E2E requires Playwright's Chromium browser
  installed locally first: `pwsh tests/test-app-end-to-end-tests/bin/Debug/net10.0/playwright.ps1 install chromium`.
