# Disposition — task 080-001

**Date:** 2026-09-03
**Outcome:** accepted-exceptions
**Rounds:** 4
**Final open count:** 0

## Summary

Rounds 1–2 (effort 1, general reviewer) covered the mediator swap (9d05efa5, fix a0e0d707): 0 bugs, 6 suggestions, 5 nits; all fixed except M6 (accepted exception). Rounds 3–4 covered the post-disposition CI delta (workflow.yml, runfile scripts, MSTest pin, .gitignore): 0 bugs, 3 suggestions, 3 nits, all fixed in 2d9fec36 and the follow-up M17 commit and verified in round 4. The reviewer re-verified the NU1301 premise on a cold cache, that Amuru beta.5 `RunAsync` throws on non-zero exit, that Nuru returns exit 1 on a handler exception, and that the MSTest 3.11.1 pin satisfies Playwright.MSTest's `>= 2.2.7` floor with exactly one consuming project.

## Final rollup (all rounds)

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 8 | 1 |
| nit | 0 | 8 | 0 |

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M6 | suggestion | `tests/test-app/test-app-client/generated/**` is gitignored but 99 files are tracked (since 3deabecc, on origin/dev). Untracking them inside the mediator-swap PR would bury the real diff; deferred to 080-003 (tests/docs sweep) with the reviewer's two options: `git rm --cached -r` the folder, or keep only snapshot-tested files and make emitted paths deterministic. | review oracle (Claude Fable, ganda task work) |

## Escalations

- None.
