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

## Checklist

- [x] `ganda repo audit --fix` run and its changes committed
- [x] Coverage artifact uploaded by CI
- [x] Six assembly-named paths left in place: no per-path exemption exists, check not turned off
- [x] Backlog 110 slug fixed via kanban CLI, or reported as not possible
- [x] Window-icon configuration complete
- [x] `./bin/dev build` clean; tracked wwwroot JS intact
- [x] Final `ganda repo audit` result recorded

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-11)
- Implementation: 01a1c6ca-60d8-7320-9d63-90efb318b8b5 (2026-11-11)
- Implement oracle: implemented by implementer-grok (grok, vendor xai, model grok-4.7, response model grok-4.7-build), session 01a1c6ca-60d8-7320-9d63-90efb318b8b5, max-turns 500, Done — 2026-11-10T18:59:00Z

## Results

`ganda repo audit --fix` wrote `.vscode/settings.json` and nothing else. That file is gitignored
(`vscode-settings` passes), so there was no tracked `--fix` diff to commit. The file sets
`window.title` (includes `timewarp-state`), `timewarp.blurImagePath`, `peacock.color`, and
`peacock.remoteColor`. The fixer also printed "Could not apply the taskbar icon now: Windows-side
ganda not found." The window-icon check still passes here. `kanban-landed` passes; the dead claim
on 098 is gone.

Coverage: `--fix` left the workflow alone. The check treats the `coverlet.collector` reference on
the end-to-end test project as collection, and it inserts an upload only into a job whose command
was patched or whose dev-cli source already passes coverage flags. This workflow runs
`dev workflow`, and that source does not pass those flags. Added an `actions/upload-artifact@v7`
step named `coverage`, path `artifacts/coverage`, `if-no-files-found: warn`. It is a `uses` step.
`defaults.run.shell` stays pwsh. Release and probe skip it, same as the package upload.
`ci-code-coverage` then passes: "CI collects code coverage and uploads it".

`scripts/test.cs` (Fixie) and `scripts/e2e-tests.cs` (`dotnet test`) do not pass `--coverage` or
`--collect:"XPlat Code Coverage"`, so a run may write no file under `artifacts/coverage`. The
warn setting keeps that from failing the job. The path is the directory the check names
(`artifacts/coverage` / `coverage.cobertura.xml`).

Kebab paths: the only per-repo key besides severity is `kebab-path-names.prune`, and that matches
directory basenames only (`KebabPathNamesCheck`, `KebabPathViolationCollector`). The check docs
say there is no path-exclude list. No per-path file exemption exists, so the six assembly-named
files were not renamed and the check was not turned off. The `.editorconfig` comment records why
`kebab-path-names.severity` stays `warning` (task 154).

Backlog 110: `ganda kanban --help` has no rename or slug command. The commands are archive, board,
claim, create, delete, depend, done, flatten, folderize, move, next, path, publish, reconcile,
release, reserve, show, steal, and who. `kanban move` only changes the column. The folder was left
as it is, and nothing was exempted for it.

Final `ganda repo audit` exited 0. Passed 42, Failed 1, Skipped 3. The one advisory is
`kebab-path-names` (warning) on the seven paths in the description. `ci-code-coverage`,
`vscode-window-icon`, and `kanban-landed` pass.

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
```

Expect:

- Audit exits 0. `ci-code-coverage`, `vscode-window-icon`, and `kanban-landed` pass. The only
  advisory is `kebab-path-names` on the seven paths in the description.
- `.vscode/settings.json` is ignored, and it sets `window.title`, `timewarp.blurImagePath`,
  `peacock.color`, and `peacock.remoteColor`.
- All six assembly-named paths are still tracked.
- Build ends with "Build completed successfully!" and 0 errors. `git status` does not show those
  wwwroot files as deleted.

## Notes

Requested by Steven 2026-11-11 after running `ganda repo audit` on master: "create task and fix
them".
