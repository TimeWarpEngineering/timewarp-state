# Round 1 — general
**Date:** 2026-10-10
**Scope reviewed:** `samples/08-notifications/readme.md`, task 028 kanban files, vs `origin/master`

## Summary

The change adds one markdown README that points at TimeWarp Architecture's `NotificationState`
and explains the pattern. No code, so runtime risk is nil; the risk is wrong links or wrong
claims. Verification against timewarp-architecture `origin/master` @ `0500982b7e5d7632aa3442c42cf29259696b1136`:

- Every GitHub `blob`/`tree` link path in the README exists (`git cat-file -e`, 0 missing).
- The repo-relative link `../../source/timewarp-state/features/pipeline/exception-notification.cs` exists; `StateTransactionBehavior` publishes `ExceptionNotification`; `TWS0002` is the handler-must-not-send-action rule in this repo.
- Behavioral claims match the source: `MaxVisible = 3`, `SuccessAutoDismissInterval` 6 s, dedupe key `(Intent, Title, Body)` with same-id refresh, body-equals-title dropped, `FromProblem` mapping, `ReportProblem` ignores 499, `RemoveExpired` uses `<= now` with caller-supplied clock, `NavigationListener` hooks `LocationChanged` and is injected in `Routes.razor`, `MessageBars` uses `AllowDismiss="false"` + Dismiss button + `ShowAll` view state + `NextExpiry` scheduling, exception handler uses `Exception.Message` as title, passkey snippet matches, TWA0025 is WASM-only and resolves only Error/Success, StyleGuide opt-out reason is quoted verbatim, Counter handler only logs on `PostPipelineNotification`.
- Diff contains only the README and kanban files, as the brief requires.

`samples/overview.md` does not list sample 08. The brief explicitly limits the PR to the README
and kanban files, and the task Results record this decision, so it is not raised as a finding.

## Issues

None.
