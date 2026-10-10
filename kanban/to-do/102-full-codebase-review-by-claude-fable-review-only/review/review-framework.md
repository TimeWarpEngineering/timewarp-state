# Review framework — task 102

**Date:** 2026-10-10
**Host task:** kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/
**Diff scope:** branch `task/102-full-codebase-review-by-claude-fable-review-only` vs `origin/master` (three-dot). Two kitchen files: `review-findings.md` and `task.md`. No product, test, doc, sample, build, or CI file is in the diff.
**Plan / brief:** Review-only whole-repo findings. The deliverable is `review-findings.md` (overall assessment, severity groups, real paths). This review checks that deliverable against the tree at commit `b255a623` (product parent `60041165`). It does not change product code or file follow-up tasks.
**Effort:** 2 (general only)
**Reviewer roster:** general
**Session IDs:** Grok 4.7 session `01a124d4-f378-7342-814a-14800fafbba6` (2026-10-10), acting as the general reviewer

## Budget (by-diff)

- Lines changed: 268
- Effort: 2
- TCB hits: none
- Roster axes: general
- Turn cap: 120 (--max-turns; cursor uncapped)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
- Product defects described in `review-findings.md` stay in that document. This task does not patch them.
