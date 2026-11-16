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

## Checklist

- [ ] `ganda repo audit --fix` run and committed
- [ ] Stale comment removed
- [ ] `severity` line removed or justified
- [ ] `ganda repo audit`: `kebab-path-names` passes

## Session

- Created: 31bc0748-8dda-4ee8-912e-8d546f2d6990 (2026-11-16)

## Notes

Requested by Steven 2026-11-16 after confirming ganda 654 cleared the six paths.
