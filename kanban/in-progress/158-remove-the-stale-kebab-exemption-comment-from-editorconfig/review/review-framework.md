# Review framework — task 158

**Date:** 2026-11-16
**Host task:** kanban/in-progress/158-remove-the-stale-kebab-exemption-comment-from-editorconfig/
**Diff scope:** branch `task/158-remove-the-stale-kebab-exemption-comment-from` vs `master`. Product change is `.editorconfig` (7 lines deleted). The kanban `task.md` move is kitchen only.
**Plan / brief:** Remove the stale task-154 kebab exemption comment and `kebab-path-names.severity = warning` under `[ganda.audit]`. Keep `kebab-path-names.prune = PULL_REQUEST_TEMPLATE`. Do not add a `kebab-path-names.allow` entry. The check default is Error; drop the override because the recorded audit passes at that default.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** reviewer-grok 01a1e395-8b31-7f51-9068-024912843c7a (2026-11-16); implementer-claude 6c20b5d4-66d2-4f32-88eb-c9b6eb3e00de

## Budget (by-diff)

- Lines changed: 140
- Effort: 1
- TCB hits: none
- Roster axes: general
- Turn cap: 80 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims by reading the diff and the kitchen `## Results` and `### How to validate` record
- Do not re-run a full build, a full test run, an audit, or the hooks; those belong to the implementer's recorded results, the host audit node, and CI
- Exception: one targeted command may check one suspected bug; the finding names the command and the reason. A full build, full test run, or audit is never that command
- A missing or thin recorded result is a finding, not a reason to run the gates
- Prior rounds are immutable; new work goes in `round-(N+1)/`
