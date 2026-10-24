# Normalize file encoding to UTF-8 without BOM

## Goal (implementer restatement)

Tracked text files in this repo should be UTF-8 with no byte order mark, and a second
`ganda repo normalize-encoding` run should change nothing. `.editorconfig` must declare
`charset = utf-8` without quotes. Root `.gitignore` must ignore `mission.heartbeat`.
`TimeWarp.Nuru` must meet the audit floor (4.0.0-beta.5 or newer). `ganda repo audit`
must report zero failed checks; the kebab-path warning may remain when those paths are
allowed Blazor or MSBuild names. The solution must still build and tests must pass after
the Nuru bump. Encoding normalization is its own commit, separate from the audit fixes.

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

- [x] `ganda repo normalize-encoding --apply` from the task worktree; a second run reports `changed: 0`.
- [x] Spot-check the diff: BOM / encoding only, no content changes (`git diff --stat` and `git diff -w` should show no text edits beyond the BOM).
- [x] `.editorconfig`: `charset = utf-8` (unquoted) under `[*]`.
- [x] `ganda repo audit --fix`, then fix what remains, until `ganda repo audit` reports 0 failed (the kebab warning may stay).
- [x] Build and tests pass after the Nuru bump (`./bin/dev build`, `./bin/dev test` or the repo's equivalent).

## Session

- Created: 252075b7-65c4-482c-b43d-14987934160a (2026-10-24)
- Implementation: 01a16e64-e4be-73c1-8f63-93a99cb67768 (2026-10-24). Encoding commit is BOM-only (121 files). Audit `--fix` pinned TimeWarp.Nuru and TimeWarp.Nuru.DevCli to 4.0.0-beta.5 and rewrote `Directory.Packages.props` with a UTF-8 BOM; a follow-up normalize stripped that mark. `kanban-landed` still warns that task 098 has no usable worktree; `--fix` does not clear it.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-24T15:07:00Z
- Review oracle: Claude Opus 5.5 (claude-opus-5-5), ganda task work headless, session id not reported, effort 2, roster general, round 1, disposition clean — 2026-10-24
- Review oracle: review by reviewer-claude (claude, model claude-opus-5-5), session not reported, max-turns 120 — 2026-10-24T15:09:41Z

## Results

- `ganda repo normalize-encoding --apply` rewrote 121 tracked files. Every one started with a UTF-8 BOM and no other byte difference after the mark was removed (0 UTF-16 files). A second apply reported `changed: 0` (scanned 1393, skipped 19 binary, needs-review 0, errors 0). `git diff` is one line per file because the BOM sits on the first line; `git diff -w` still shows that line. A byte compare after stripping the BOM found no text changes.
- `.editorconfig` `[*]` has unquoted `charset = utf-8`. The quoted `charset = "utf-8"` under the source-file brace glob was removed so the resolved value for `probe.cs` is unquoted.
- Root `.gitignore` ignores `mission.heartbeat`.
- `ganda repo audit --fix` pinned `TimeWarp.Nuru` and `TimeWarp.Nuru.DevCli` from 3.0.0 to 4.0.0-beta.5. The props writer put a UTF-8 BOM back on `Directory.Packages.props`. A follow-up normalize changed that one file, and the next dry-run reported `changed: 0`. The committed props diff is the two version pins.
- `./bin/dev build` exited 0 (0 errors, 162 pre-existing warnings). `./bin/dev test` exited 0 in about 7.5 minutes. Suite counts: analyzer 44, source generator 90, state 120 passed (1 skipped), plus 40 passed (1 skipped), telemetry 21, devtools 101, devtools isolation 4, client integration 64 passed (1 skipped), architecture 7 passed (1 skipped).
- `ganda repo audit` after `--fix` exited 0: Passed 31, Failed 2, both warnings. `kebab-path-names` lists 7 paths the task said to leave: 6 Blazor `*.lib.module.*` / MSBuild `.targets` names plus the `kanban/backlog/110-…--predefined-state-for-any-site/` folder (double hyphen); `--fix` changed none of them (corrected by review M1). `kanban-landed` warns that task 098 has no usable worktree. No Error checks remain. The dev-cli self-install inside `--fix` compiled against Nuru 4.0.0-beta.5.

### How to validate

Smoke: `ganda repo normalize-encoding` from this worktree (dry-run).
Expect: `changed: 0`, `errors: 0`, `needs-review: 0`.

Smoke: `ganda repo audit`
Expect: exit 0. `editorconfig-charset`, `kanban-porcelain-gitignore`, and `nuru` pass. `kebab-path-names` may warn on the 6 Blazor and MSBuild paths and the kanban 110 folder. `kanban-landed` may warn that task 098 has no worktree.

Smoke: `./bin/dev build` then `./bin/dev test`
Expect: both exit 0.

### Review disposition

- Rounds: 1 (effort 2, roster: general; review oracle inline).
- Final counts: bug 0, suggestion 0, nit 1 (fixed). Open 0, wontfix 0.
- Disposition: **clean**. M1 (nit) said Results called all 7 kebab paths Blazor/MSBuild names. The reviewer corrected that wording in the kitchen. No product change was needed.
- Reviewer re-verified the following. Every product file except the three config files differs only by its BOM, and no tracked file still has a BOM. The normalize dry-run reports `changed: 0` and `ganda repo audit` exits 0 with warnings only. `scripts/clean.cs` and `scripts/build.cs` compile against Nuru 4.0.0-beta.5. The full test suite was not re-run; the implementer's run is the evidence for it.
- Artifacts: `review/review-framework.md`, `review/round-1/general.md`, `review/round-1/merged.md`, `review/disposition.md`.

## Learnings

- Restatement misread (minor): the restatement says the kebab warning "may remain when those paths are allowed Blazor or MSBuild names". The task said to "leave them unless audit `--fix` changes them" and described them as "mostly" Blazor/MSBuild. One of the 7 is a kanban backlog folder with a double hyphen. The narrower restatement then carried into an inaccurate Results line (review M1). why: review oracle round 1, 2026-10-24
- `ganda repo audit --fix` (the props writer) writes `Directory.Packages.props` back with a UTF-8 BOM. Any later Nuru pin will bring the BOM back until ganda fixes that writer. Run `ganda repo normalize-encoding` after `--fix`. why: review oracle round 1, 2026-10-24

## Review

- Round 1: Done

## Notes

Commit the encoding normalization separately from the audit fixes so the BOM-only diff stays easy to review.
