# Normalize file encoding to UTF-8 without BOM

## Description

TimeWarp source files are UTF-8 without a byte order mark. ganda task 451 (timewarp-ganda PR #330)
added `ganda repo normalize-encoding` and the `editorconfig-charset` audit check. This repo has
violations of both.

A dry run of `ganda repo normalize-encoding` on master (2026-10-24) scanned 1391 tracked files and
would change 121. It skipped 19 binary files and reported 0 needs-review and 0 errors. Most of the
changes are `.razor`, `.cs`, `.csproj` and `.js` files under `source/` and `tests/`.

`ganda repo audit` on master fails 4 checks:

- `editorconfig-charset` (Error): `.editorconfig` line 35 has `charset = "utf-8"`. Remove the quotes. Audit does not auto-fix a quoted value.
- `kanban-porcelain-gitignore` (Error): root `.gitignore` is missing `mission.heartbeat`.
- `nuru` (Error): `TimeWarp.Nuru` 3.0.0 < 4.0.0-beta.5 in `Directory.Packages.props`.
- `kebab-path-names` (Warning): 7 paths, mostly Blazor `*.lib.module.js` / MSBuild `.targets` names that the conventions allow. Leave them unless audit `--fix` changes them.

Per policy, fix every audit failure in this task and commit whatever `ganda repo audit --fix` changes.

## Checklist

- [ ] `ganda repo normalize-encoding --apply` from the task worktree; a second run reports `changed: 0`.
- [ ] Spot-check the diff: BOM / encoding only, no content changes (`git diff --stat` and `git diff -w` should show no text edits beyond the BOM).
- [ ] `.editorconfig`: `charset = utf-8` (unquoted) under `[*]`.
- [ ] `ganda repo audit --fix`, then fix what remains, until `ganda repo audit` reports 0 failed (the kebab warning may stay).
- [ ] Build and tests pass after the Nuru bump (`./bin/dev build`, `./bin/dev test` or the repo's equivalent).

## Session

- Created: 252075b7-65c4-482c-b43d-14987934160a (2026-10-24)

## Notes

Commit the encoding normalization separately from the audit fixes so the BOM-only diff stays easy to review.
