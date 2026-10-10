# Round 1 — general
**Date:** 2026-10-10
**Scope reviewed:** `review-findings.md` and `task.md` vs `origin/master`. Claims were checked against the tree at `b255a623` (High and Medium citations, plus the Low/Nit lines that were easy to falsify). Warning totals from the implementer's Release build were not re-run.

## Summary

The deliverable matches the brief. The diff outside `kanban/` is empty, the severity sections are present, and the High findings (package dependency, unconditional rollback, clone `default` for services, collection `Count()` check) match the code and the cached nuspec files. Two line ranges point past the end of the file, and the closing nit miscounts `TODO` comments. Those are defects in the findings file, not new product bugs.

## Issues

### Issue 1 — Severity: bug
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:60
- Description: M1 cites `source/timewarp-state/store/store.redux-dev-tools.cs:274-349`. That file has 119 lines. The seven `UnconditionalSuppressMessage` attributes are real; they sit at lines 43–71. M5 cites `source/timewarp-state/store/i-store.cs:379,387`. That file has 38 lines. `GetState(Type)` is line 27, `GetSemaphore` is line 29, and `StateInitializationTasks` is line 37. A reader who jumps to the cited lines does not land on the code. The behavioral claims at both sites are otherwise right.
- Suggestion: Replace the two ranges with the lines above.
- Status: open

### Issue 2 — Severity: nit
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:206
- Description: The nit says the four remaining `TODO` comments are all the persistence no-ops in M9. `source/` has five `TODO` comments. Four are those persistence no-ops (`persistent-state-post-processor.cs` lines 93 and 130, `persistence-service.cs` lines 107–108). The fifth is a file-layout note at `source/timewarp-state-policies/policies.action-policy.cs:15`.
- Suggestion: Say five `TODO` comments, and name the policies note separately from the four persistence no-ops.
- Status: open

### Issue 3 — Severity: nit
- File: kanban/to-do/102-full-codebase-review-by-claude-fable-review-only/review-findings.md:152
- Description: L1 cites `timewarp-state-options.cs:25` for `UseStateTransactionBehavior`. Line 25 is the `Assemblies` property. The flag is line 29.
- Suggestion: Cite line 29.
- Status: open
