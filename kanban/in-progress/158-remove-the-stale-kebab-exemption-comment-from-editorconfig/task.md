# Remove the stale kebab exemption comment from .editorconfig

## Description

Task 154 left this note under `[ganda.audit]` in `.editorconfig`:

```
# Blazor JS initializers ({AssemblyName}.lib.module.js), their map / d.ts / ts sources,
# and build/{PackageId}.targets must keep the assembly or package id (task 154).
# kebab-path-names.prune matches directory basenames only. There is no per-path
# file exemption, so these six names stay visible as a warning instead of turning
# the check off. Backlog 110's doubled hyphen is a separate kitchen the kanban CLI
# cannot rename.
kebab-path-names.severity = warning
```

It is no longer true. timewarp-ganda **654** made the check accept Blazor `.lib.module.*` names
and NuGet `build/{PackageId}.targets` built in, and added a `kebab-path-names.allow` key.
`ganda repo audit` on master (2026-11-16) no longer flags any of the six files. Backlog 110 was
renamed with `ganda kanban rename` the same day, so the last sentence is stale too.

## Requirements

1. Delete the six comment lines. Do not add a `kebab-path-names.allow` entry; none is needed.
2. Decide `kebab-path-names.severity = warning`: if `warning` is the check's default, remove the
   line (it only existed to carry the comment); if the default is stricter and the repo passes at
   the default, remove it as well; keep it only if removing it makes the audit fail, and then say
   why in a one-line comment with this task id. Find the default from the check rather than guess.
3. Leave the `kebab-path-names.prune = PULL_REQUEST_TEMPLATE` line and its comment alone.
4. Run `ganda repo audit --fix` first and commit what it changes (audit fixes are policy).
5. Gate: `ganda repo audit` passes with `kebab-path-names` passing (0 non-kebab paths). Record
   the final audit summary in Results. No build or test run is needed for a comment-only change
   unless the audit fix touches code.

## Goal (implementer restatement)

Drop the six-line task-154 comment under `[ganda.audit]` in `.editorconfig`; ganda 654 made it
false. The `kebab-path-names.severity = warning` line only carried that comment: the check's
default (per `ganda repo audit --list-checks`) is Error, so remove the line if the repo passes at
Error. Keep the `PULL_REQUEST_TEMPLATE` prune line. Done when `ganda repo audit --fix` changes are
committed and `ganda repo audit` passes with `kebab-path-names` green at its default severity.

## Checklist

- [x] `ganda repo audit --fix` run and committed (it changed nothing)
- [x] Stale comment removed
- [x] `severity` line removed (default is Error; repo passes at it)
- [x] `ganda repo audit`: `kebab-path-names` passes

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-16)
- Implement oracle (claude, 2026-11-16): removed the stale comment and the severity override.
  The first `./bin/dev test` failed on "Run dotnet tool restore" for fixie (stale
  `~/.dotnet/toolResolverCache`). After clearing that cache, the rerun passed. No repo change was
  needed.

## Results

- `.editorconfig` `[ganda.audit]`: deleted the six-line task-154 comment and
  `kebab-path-names.severity = warning`. `ganda repo audit --list-checks` lists the
  `kebab-path-names` default as **Error**. The repo passes at that default, so the line was
  removed (Requirement 2). No `kebab-path-names.allow` entry was added. The
  `PULL_REQUEST_TEMPLATE` prune line and its comment stay unchanged.
- `ganda repo audit --fix` (run first) changed no files, so there was nothing to commit from it.

### Gates

| Command | Result |
|---------|--------|
| `./bin/dev build` | pass: Build succeeded, 0 warnings, 0 errors |
| `./bin/dev test` | pass after a tool-cache clear: 90, 174, 153 (1 skipped), 40 (1 skipped), 21, 124, 4, 67 (1 skipped), 7 (1 skipped) passed, 0 failed |
| `ganda repo audit` | pass: "Repository passes all audit checks." `kebab-path-names` PASS, "All path basenames are kebab-case" |

### How to validate

- Smoke: `grep -n 'kebab-path-names' .editorconfig` then `ganda repo audit`
- Expect: only the `kebab-path-names.prune = PULL_REQUEST_TEMPLATE` line remains, and there is no
  `severity` line. The audit prints `kebab-path-names` PASS "All path basenames are kebab-case" and
  "Repository passes all audit checks."

## Notes

Requested by Steven 2026-11-16 after confirming ganda 654 cleared the six paths.
