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

## Checklist

- [ ] `ganda repo audit --fix` run and its changes committed
- [ ] Coverage artifact uploaded by CI
- [ ] Six assembly-named paths exempted with reason + task id (not renamed)
- [ ] Backlog 110 slug fixed via kanban CLI, or reported as not possible
- [ ] Window-icon configuration complete
- [ ] `./bin/dev build` clean; tracked wwwroot JS intact
- [ ] Final `ganda repo audit` result recorded

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-11)

## Notes

Requested by Steven 2026-11-11 after running `ganda repo audit` on master: "create task and fix
them".
