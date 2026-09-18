# Review framework — task 081

**Date:** 2026-09-18
**Host task:** kanban/in-progress/081-twbreadcrumb-must-not-require-bootstrap-css/
**Diff scope:** branch `task/081-twbreadcrumb-must-not-require-bootstrap-css` vs `origin/master` (commit `e6f563a4`)
**Plan / brief:** Decouple `TwBreadcrumb` from Bootstrap class names; ship isolated CSS so the trail is readable in any host (Fluent UI included). Preserve `MaxLinks`, ellipsis, current page as text, ancestor `RouteState.GoBack`. Document that hosts do not need Bootstrap.
**Effort:** 1 (general only)
**Reviewer roster:** general
**Session IDs:** grok review oracle `01a0b252-7c1c-72b0-b1b6-c87bf38de48b` (2026-09-18)

## Ground rules

- Reviewers are read-only on product code; they write only under `review/round-N/`
- Severity: bug | suggestion | nit — Status starts as open
- Do not invent issues to fill space; zero issues is a valid outcome
- Address the diff and surrounding call sites; re-verify falsifiable claims against the repo
- Prior rounds are immutable; new work goes in `round-(N+1)/`

## Product files in scope

- `source/timewarp-state-plus/features/routing/components/TwBreadcrumb.razor`
- `source/timewarp-state-plus/features/routing/components/TwBreadcrumb.razor.css` (new)
- `source/timewarp-state-plus/features/routing/components/tw-breadcrumb.md` (new)
- `source/timewarp-state-plus/readme.md`
- `samples/03-routing/wasm/sample-03-wasm/wwwroot/index.html`
- `samples/03-routing/wasm/overview.md`
- `documentation/topics/routing.md`
- `documentation/topics/toc.yml`
- `tests/timewarp-state-plus-tests/features/routing/tw-breadcrumb-style-tests.cs` (new)

## Requirements to check

- Trail is usable without Bootstrap CSS in the host
- No Bootstrap package/CDN/`bootstrap.min.css` added to TimeWarp.State.Plus
- Bootstrap class names replaced; `aria-label="breadcrumb"` and `aria-current="page"` kept
- Behavior preserved (`MaxLinks`, ellipsis, current page as text, ancestors `GoBack`)
- Sample 03 trail does not depend on `bootstrap.min.css` for styling
- Docs state hosts do not need Bootstrap
