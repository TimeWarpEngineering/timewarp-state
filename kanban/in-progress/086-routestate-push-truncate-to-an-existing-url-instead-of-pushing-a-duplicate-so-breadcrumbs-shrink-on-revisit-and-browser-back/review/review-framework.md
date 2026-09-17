# Review framework — task 086

**Date:** 2026-09-17
**Host task:** kanban/in-progress/086-routestate-push-truncate-to-an-existing-url-instead-of-pushing-a-duplicate-so-breadcrumbs-shrink-on-revisit-and-browser-back/
**Diff scope:** branch `task/086-routestate-push-truncate-to-an-existing-url-instea` vs `origin/master` (product: `9f47e471`; kitchen Results: `b67696b7`)
**Plan / brief:** `PushRouteInfoActionSet.Handler` must scan the whole `RouteStack` for the incoming URL; if found, truncate down to that entry (pop everything above it, including the match) and push an updated title instead of duplicating. Same-URL-on-top keeps update-in-place. Keep GoBack and the task-059 clamp unchanged. Required tests: A→B→C→A ⇒ `[A]`; A→B→C→B ⇒ `[B, A]`; A twice ⇒ one entry, title updated. Optional stack-depth cap: implementer declined (unique-URL growth is a real trail; `TwBreadcrumb.MaxLinks` already limits display).
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0afd6-7398-71a2-8caf-6468ddfde474` (2026-09-17)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`
