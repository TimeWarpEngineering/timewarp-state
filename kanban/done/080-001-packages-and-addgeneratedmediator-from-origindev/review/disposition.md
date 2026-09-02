# Disposition — task 080-001

**Date:** 2026-09-03
**Outcome:** accepted-exceptions
**Rounds:** 2
**Final open count:** 0

## Summary

Effort 1 (general reviewer, Claude Opus) over the mediator swap (9d05efa5) found 0 bugs, 6 suggestions and 4 nits; the reviewer independently re-derived the behavior pipeline order, confirmed each rewritten behavior is semantically equivalent to its MessagePre/PostProcessor predecessor, and reproduced the Results test counts. Nine findings were fixed on this task (a0e0d707) and verified in round 2, which added one comment-wording nit (M11, fixed). One suggestion (M6) is an accepted exception.

## Exception log (if accepted-exceptions)

| ID | Severity | Rationale | Decided by |
|----|----------|-----------|------------|
| M6 | suggestion | `tests/test-app/test-app-client/generated/**` is gitignored but 99 files are tracked (since 3deabecc, on origin/dev). Untracking them inside the mediator-swap PR would bury the real diff; deferred to 080-003 (tests/docs sweep) with the reviewer's two options: `git rm --cached -r` the folder, or keep only snapshot-tested files and make emitted paths deterministic. | review oracle (Claude Fable, ganda task work) |

## Escalations

- None.
