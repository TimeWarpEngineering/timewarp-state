# Clear the advisory repo audit warnings

## Description

`ganda repo audit` on master (2026-11-11) passes with four advisory warnings. Clear them so the
audit is fully clean and new warnings stand out.

1. **ci-code-coverage** — CI collects code coverage but does not upload it: no
   `actions/upload-artifact` step whose path includes the coverage output.
2. **kebab-path-names** — 7 non-kebab paths:
   - `kanban/backlog/110-state-plus-a-la-carte-modules-pluggable-action-sets--predefined-state-for-any-site/`
     (double hyphen in the slug)
   - `source/timewarp-state-blazor/wwwroot/js/TimeWarp.State.Blazor.lib.module.js`
   - `source/timewarp-state-blazor/wwwroot/js/TimeWarp.State.Blazor.lib.module.js.map`
   - `source/timewarp-state-blazor/wwwroot/types/TimeWarp.State.Blazor.lib.module.d.ts`
   - `source/timewarp-state-blazor/wwwroot/typescript/TimeWarp.State.Blazor.lib.module.ts`
   - `source/timewarp-state-devtools-app/build/TimeWarp.State.DevTools.App.targets`
   - `tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js`
3. **vscode-window-icon** — VS Code window-icon configuration is incomplete
   (`.vscode/settings.json` is missing).
4. **kanban-landed** — a dead claim on 098 with no worktree. Already cleared outside this task
   with `ganda kanban release 098 --dead` (2026-11-11); nothing to do here beyond confirming the
   warning is gone.

## Requirements

1. Run `ganda repo audit --fix` first and commit whatever it changes (audit fixes are policy).
2. **Coverage:** if `--fix` did not add it, add an `actions/upload-artifact` step to
   `.github/workflows/workflow.yml` whose path includes the coverage output. Steps stay pwsh.
3. **Kebab paths — do not rename the six assembly-named files.** Blazor JS initializers
   (`{AssemblyName}.lib.module.js`), their map / d.ts / ts sources, and the NuGet
   `build/{PackageId}.targets` file must match the assembly or package id; renaming them breaks
   JS initializer discovery and package build imports. Exempt them using the mechanism the
   `kebab-path-names` check supports (find it with `ganda repo audit --help` / the check's docs /
   ganda source; expected to be a check-specific key under `[ganda.audit]` in `.editorconfig`,
   with a reason and this task id). Do not turn the whole check off if a per-path exemption exists.
   If no per-path exemption exists, stop and report that in Results rather than disabling the check.
4. **Kebab paths — backlog 110:** fix the double hyphen with the kanban CLI if it can rename a
   slug; never hand-rename or hand-number a kanban folder. Check `ganda kanban --help`. If the CLI
   cannot do it, leave the folder, say so in Results, and exempt nothing for it.
5. **Window icon:** let `--fix` generate the configuration. `.vscode/settings.json` is gitignored
   in this repo (the `vscode-settings` check passes on that), so do not force-add it; commit only
   what the check expects to be tracked.
6. `--no-incremental` builds delete tracked `wwwroot` JS in `timewarp-state-blazor`; if a build
   removes those files, rebuild the project to regenerate them and check `git status` before
   finishing.
7. Finish with `ganda repo audit` and record the remaining warnings (ideally none) in Results.

## Goal (implementer restatement)

Make `ganda repo audit` on this repo finish with no advisory warnings, so a new warning is obvious.

Acceptance criteria:

- `ganda repo audit --fix` runs first, and whatever it changes is committed.
- CI uploads the coverage output. If `--fix` does not add it, `.github/workflows/workflow.yml` gets an
  `actions/upload-artifact` step whose path includes that output. Workflow steps stay pwsh.
- The six assembly-named paths stay as they are. Blazor JS initializers, their map, d.ts, and ts sources,
  and the NuGet `build/{PackageId}.targets` file must keep the assembly or package id. They are exempted
  only through a per-path mechanism the `kebab-path-names` check already supports, with a reason and this
  task id. The whole check is not turned off. If that mechanism does not exist, the paths stay reported
  and Results says so.
- Backlog 110's double hyphen is fixed only by the kanban CLI. The folder is not hand-renamed. If the CLI
  cannot rename a slug, the folder stays, Results says so, and nothing is exempted for it.
- The window-icon check is completed by `--fix`. `.vscode/settings.json` stays untracked because this repo
  gitignores it. Only the files the check expects to be tracked are committed.
- `./bin/dev build` is clean. A `--no-incremental` build that deletes tracked `wwwroot` JS is followed by
  a rebuild that restores those files, and `git status` is checked before finish.
- The last `ganda repo audit` and any remaining warnings are recorded under Results. The dead claim on 098
  is only confirmed gone.
- Fix pass: the end-to-end `dotnet test` collects Cobertura for real. It passes
  `--collect:"XPlat Code Coverage;IncludeTestAssembly=true"` and writes `--results-directory` to the repo's
  `artifacts/coverage`, the directory the workflow uploads. IncludeTestAssembly is set because coverlet skips
  the test assembly by default and this suite does not load the product assemblies in-process. The upload step's `if-no-files-found` is `error`, and its comment
  says a missing file means collection did not write, because that step already skips release and probe.
  The Fixie unit runner stays as it is: `dotnet fixie` does not load the VSTest collector, and those
  projects do not reference `coverlet.collector`. Results records the choice to collect and keep the upload.

## Checklist

- [x] `ganda repo audit --fix` run and its changes committed
- [x] Coverage artifact uploaded by CI
- [x] Six assembly-named paths left in place: no per-path exemption exists, check not turned off
- [x] Backlog 110 slug fixed via kanban CLI, or reported as not possible
- [x] Window-icon configuration complete
- [x] `./bin/dev build` clean; tracked wwwroot JS intact
- [x] Final `ganda repo audit` result recorded
- [x] Fix pass: e2e `dotnet test` writes Cobertura under `artifacts/coverage`; upload fails when that directory has no file

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-11)
- Implementation: 01a1c6ca-60d8-7320-9d63-90efb318b8b5 (2026-11-11)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1c6ca-60d8-7320-9d63-90efb318b8b5, max-turns 500, Done — 2026-11-10T18:59:00Z
- Review oracle round 1: claude (vendor anthropic, model claude-opus-5-5), effort 2, roster general, turn cap 120 — 2026-11-11. Fix pass requested for M1, M2.
- Review oracle: review by reviewer-claude (claude, vendor anthropic, model claude-opus-5-5, response model claude-opus-5-5), session 98302167-8403-46c6-a785-65041a9c820b, max-turns 120, artifacts general.md, merged.md — 2026-11-10T19:00:57Z
- Fix pass: 01a1c6d5-b9b8-7481-9684-402651d09175 (2026-11-11). implement-fix-2. Collect Cobertura from the e2e `dotnet test` and keep the upload.

## Results

`ganda repo audit --fix` wrote `.vscode/settings.json` and nothing else. That file is gitignored
(`vscode-settings` passes), so there was no tracked `--fix` diff to commit. The file sets
`window.title` (includes `timewarp-state`), `timewarp.blurImagePath`, `peacock.color`, and
`peacock.remoteColor`. The fixer also printed "Could not apply the taskbar icon now: Windows-side
ganda not found." The window-icon check still passes here. `kanban-landed` passes; the dead claim
on 098 is gone.

Coverage, first pass: `--fix` left the workflow alone. The check treats the `coverlet.collector`
reference on the end-to-end test project as collection. Added an `actions/upload-artifact@v7` step
named `coverage`, path `artifacts/coverage`. It is a `uses` step. `defaults.run.shell` stays pwsh.
Release and probe skip it, same as the package upload. `ci-code-coverage` then passes: "CI collects
code coverage and uploads it".

Coverage, fix pass (round-1 M1 and M2): the choice is to collect and keep the upload. `scripts/e2e-tests.cs`
now runs `dotnet test` with `--collect:"XPlat Code Coverage;IncludeTestAssembly=true"` and
`--results-directory` set to the absolute repo path `artifacts/coverage`. The directory is emptied
first, and the reset throws unless the path ends in `artifacts/coverage`. IncludeTestAssembly is
set because coverlet skips the test assembly by default, and this Playwright suite does not load
the product assemblies in its own process (the SUT is separate). Without that switch the Cobertura
file has `lines-valid="0"`. VSTest writes `artifacts/coverage/<guid>/coverage.cobertura.xml`. The
upload path is the directory, so the guid folder is part of the artifact. `if-no-files-found` is
`error`. The step `if:` already skips release and probe, so a missing file means the test run wrote
nothing. The step comment says that.

The Fixie unit runner is unchanged. `scripts/test.cs` calls `dotnet fixie`, which does not load the
VSTest collector, and those projects do not reference `coverlet.collector`. Switching every suite
to `dotnet test` is a separate change. `ci-code-coverage` still passes.

Kebab paths: the only per-repo key besides severity is `kebab-path-names.prune`, and that matches
directory basenames only (`KebabPathNamesCheck`, `KebabPathViolationCollector`). The check docs
say there is no path-exclude list. No per-path file exemption exists, so the six assembly-named
files were not renamed and the check was not turned off. The `.editorconfig` comment records why
`kebab-path-names.severity` stays `warning` (task 154).

Backlog 110: `ganda kanban --help` has no rename or slug command. The commands are archive, board,
claim, create, delete, depend, done, flatten, folderize, move, next, path, publish, reconcile,
release, reserve, show, steal, and who. `kanban move` only changes the column. The folder was left
as it is, and nothing was exempted for it.

First-pass `ganda repo audit` exited 0. Passed 42, Failed 1, Skipped 3. The one advisory was
`kebab-path-names` (warning) on the seven paths in the description. `ci-code-coverage`,
`vscode-window-icon`, and `kanban-landed` passed.

Fix-pass `ganda repo audit` before the kitchen land exited 1. Passed 41, Failed 2, Skipped 3.
`kanban-landed` was an Error because this kitchen differed from origin-home. Blanket
`ganda repo audit --fix` was not run: the kebab fixer renames paths, and this task must not
rename the six assembly-named files or backlog 110. `ganda kanban reconcile` landed the kitchen.
The audit after that land exited 0. Passed 42, Failed 1, Skipped 3. `ci-code-coverage` passes
("CI collects code coverage and uploads it"). `vscode-window-icon` and `kanban-landed` pass.
`kebab-path-names` warns on the same seven paths.

`./bin/dev build` succeeded with 0 warnings and 0 errors (00:00:16.90). The six assembly-named
paths are still tracked. `git status` shows no change under those `wwwroot` files or the targets
file. This build is not the `--no-incremental` clean in `scripts/build.cs`, which is the path that
deletes `source/timewarp-state-blazor/wwwroot/js`.

### How to validate

Smoke:

```bash
ganda repo audit
git check-ignore -v .vscode/settings.json
git ls-files -- \
  source/timewarp-state-blazor/wwwroot/js/TimeWarp.State.Blazor.lib.module.js \
  source/timewarp-state-blazor/wwwroot/js/TimeWarp.State.Blazor.lib.module.js.map \
  source/timewarp-state-blazor/wwwroot/types/TimeWarp.State.Blazor.lib.module.d.ts \
  source/timewarp-state-blazor/wwwroot/typescript/TimeWarp.State.Blazor.lib.module.ts \
  source/timewarp-state-devtools-app/build/TimeWarp.State.DevTools.App.targets \
  tests/test-app/test-app-client/wwwroot/Test.App.Client.lib.module.js
./bin/dev build
rm -rf /tmp/task-154-coverage
dotnet test tests/test-app-end-to-end-tests/test-app-end-to-end-tests.csproj \
  --configuration Release \
  --settings tests/test-app-end-to-end-tests/playwright-settings/chrome.runsettings \
  --collect:"XPlat Code Coverage;IncludeTestAssembly=true" \
  --results-directory /tmp/task-154-coverage \
  --filter "FullyQualifiedName~HomePage_.Should_.RenderStaticContent"
find /tmp/task-154-coverage -name coverage.cobertura.xml
```

Expect:

- Audit: `ci-code-coverage` and `vscode-window-icon` pass. The only advisory is `kebab-path-names`
  on the seven paths in the description. `kanban-landed` passes once this kitchen has been landed
  on origin-home.
- `.vscode/settings.json` is ignored, and it sets `window.title`, `timewarp.blurImagePath`,
  `peacock.color`, and `peacock.remoteColor`.
- All six assembly-named paths are still tracked.
- Build ends with "Build completed successfully!" and 0 errors. `git status` does not show those
  wwwroot files as deleted.
- The filtered test writes `coverage.cobertura.xml` with `lines-valid` greater than 0. Without a
  running SUT the test itself fails (`ERR_CONNECTION_REFUSED`). That failure is the missing server,
  and the Cobertura file is still written. A full `dev e2e` run writes the same report under the
  repo's `artifacts/coverage/<guid>/`.

### Review

Round 1 (effort 2, roster: general). Counts at merge: bug 1 open, nit 1 open. Fix pass implemented
both. Round 2 is the host's next review node. `review/round-1/merged.md` is unchanged.

- **M1 (bug):** the e2e `dotnet test` now collects Cobertura into `artifacts/coverage`. A filtered
  run wrote `coverage.cobertura.xml` with `lines-valid="332"` and `lines-covered="6"`. The upload
  `if-no-files-found` is `error`.
- **M2 (nit):** the step comment now says the file is missing when that test run wrote nothing.
  Release and probe are already excluded by the step `if:`.

The goal restatement matches the task. The other items (kebab exemption not available, backlog 110
left alone, window icon, 098 claim) were verified in round 1.

Paths: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`.

## Review

- Round 1: open M1, M2 — fix pass applied on this branch. Round 2 has not run.

## Notes

Requested by Steven 2026-11-11 after running `ganda repo audit` on master: "create task and fix
them".
