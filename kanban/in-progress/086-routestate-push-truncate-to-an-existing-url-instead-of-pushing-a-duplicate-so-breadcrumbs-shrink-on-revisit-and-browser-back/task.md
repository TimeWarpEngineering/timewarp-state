# RouteState push: truncate to an existing URL instead of pushing a duplicate so breadcrumbs shrink on revisit and browser Back

## Description

Documented root cause (cockpit investigation 2026-09-17, read-only across timewarp-state,
timewarp-architecture, and COPIC). **Open; not scheduled.** Steve: "document it, we come back to
it later or another agent can do it."

Symptom: in timewarp-architecture the `TwBreadcrumb` trail only grows — e.g.
"Settings / Authentication / Profile / Settings" — and never shrinks when the user goes back via
a sidebar link or the browser Back button. COPIC looks fine only because its flows return via the
crumb link, which is the one path that truncates.

Root cause (library, both apps affected):

- `RouteState` is a `Stack<RouteInfo>` (newest on top) —
  `source/timewarp-state-plus/features/routing/route-state/route-state.cs:8`.
- `PushRouteInfoActionSet.Handler` (`route-state.push-route-info.cs:30-39`) compares the incoming
  URL only against the **top** (`TryPeek`); on a match it updates the title in place, otherwise
  it pushes — it never scans deeper for an earlier occurrence of the same URL, so revisiting an
  older page pushes a duplicate instead of truncating back to it.
- The only pop is `GoBackActionSet.Handler` (`route-state.go-back.cs:32-49`), and the only caller
  of GoBack is the breadcrumb's own click handler (`TwBreadcrumb.razor:28-31`; COPIC
  `BreadCrumbs.razor:5-8`). Nav-menu links and browser Back/Forward never call it; they just
  render, and both apps push on every render from the page shell (`TimeWarpPage.razor:37-44`;
  COPIC `CopicPage.razor:18-22`). Neither app subscribes to `NavigationManager.LocationChanged`.
- `MaxLinks` (`TwBreadcrumb.razor:17-19`) is a display-only `Take(N)` over an unbounded stack, so
  growth is invisible until duplicates rotate into view.
- Not a version regression: routing source is identical between COPIC's pinned
  `11.0.0-beta.83` and architecture's `12.0.0-beta.1`; task 059 only clamped the GoBack pop count.

## Requirements (when picked up)

- In `PushRouteInfoActionSet.Handler`: scan the whole stack for an entry with the same URL; if
  found, truncate down to that entry (pop everything above it) and update its title, instead of
  pushing a duplicate. Same-URL-on-top keeps today's update-in-place behaviour.
- Keep GoBack semantics and the task-059 clamp unchanged.
- Tests next to the existing GoBack clamp tests: (1) push A, B, C, then A → stack is exactly
  [A]; (2) push A, B, C, then B (browser-back-to-B without GoBack) → stack is [B, A], not
  [B, C, B, A]; (3) push A twice → one entry, title updated.
- Consider an optional cap on stack depth (e.g. 50) as belt-and-braces; document the decision.
- Consumers (architecture, COPIC) need only a package bump.

## Checklist

- [x] Truncate-to-existing-URL on push + tests
- [x] `dev build` 0/0; library tests green; `ganda repo audit` clean
- [x] Results and How to validate (architecture: Home → Settings → Profile → Settings shows
      "Home / Settings", not a four-deep trail)
- [x] Implementation review disposition (same task id)

## Session

- Created: cockpit (2026-09-16); brief written 2026-09-17
- Claude Code cockpit session: https://claude.ai/code/session_01KPZXyAmA6Vk99W1yUQUn1N
- Implementer: Grok (2026-09-17) — truncate-on-push + plus tests
- Review oracle: Grok session `01a0afd6-7398-71a2-8caf-6468ddfde474` (2026-09-17) — effort 1, general only

## Notes

- Files: `source/timewarp-state-plus/features/routing/route-state/route-state.push-route-info.cs`,
  `route-state.go-back.cs`, `components/TwBreadcrumb.razor`, `components/TwPageTitle.razor`;
  tests near the GoBack clamp tests (task 059).
- Consumers: timewarp-architecture `web-spa/components/TimeWarpPage.razor`; COPIC
  `Web.Spa/Components/Pages/CopicPage/{CopicPage.razor,MainContentArea/BreadCrumbs.razor}`.
- Stack-depth cap: **not added**. Truncate-on-revisit is the unbounded-duplicate fix.
  Unique-URL growth is a real trail; `TwBreadcrumb.MaxLinks` already limits display.
  Dropping oldest would hide Home. Revisit if a consumer reports unique-URL blowup.
- Review kitchen: `review/review-framework.md`, `review/round-1/`, `review/disposition.md`.

## Results

`PushRouteInfoActionSet.Handler` now scans the whole `RouteStack` for the incoming
`NavigationManager.Uri`. If that URL is already present (including on top), it pops
everything above the match and replaces the match with an updated title. A new URL
still pushes. `GoBackActionSet` and the task-059 clamp are unchanged.

**Files**

- `source/timewarp-state-plus/features/routing/route-state/route-state.push-route-info.cs`
  — `TruncateToOrPush`; Purpose/Design regions record the no-cap decision
- `tests/timewarp-state-plus-tests/features/routing/push-route-info-tests.cs`
  — A→B→C→A ⇒ `[A]`; A→B→C→B ⇒ `[B, A]`; A twice ⇒ one entry, title updated;
  new URLs still append

**Decisions**

- No stack-depth cap (see Notes).
- Consumers (architecture, COPIC) need only a TimeWarp.State.Plus package bump
  after this ships; no app-side code change.

**Tests**

- `dotnet fixie timewarp-state-plus-tests`: 15 passed, 1 skipped (includes the four
  new PushRouteInfo cases and the GoBack clamp tests)
- `dotnet run --file ./scripts/test.cs`: analyzer 10; state suite; plus 15/1;
  client integration 42/1; architecture 7/1 — all green
- `dotnet run --file ./tools/dev-cli/dev.cs -- build`: 0 errors (47 pre-existing
  RS0030/NU1510 in test-app / e2e, none in Plus)
- `ganda repo audit`: exit 0; 3 advisory warnings (generated kebab paths,
  memsearch scaffold, vscode peacock) — not introduced here

### How to validate

**Automated**

```bash
dotnet tool restore
dotnet fixie timewarp-state-plus-tests
```

**Expect:** `15 passed, 1 skipped`. The four `PushRouteInfo_.PushRouteInfo_Should.*`
cases pass: revisit A after A/B/C leaves `[A]`; revisit B leaves `[B, A]` not
`[B, C, B, A]`; pushing A twice keeps one entry with the new title.

**Smoke (library)**

```bash
dotnet run --file ./scripts/test.cs
```

**Expect:** every suite step exits 0 (analyzer, state, plus, client integration,
architecture).

**Smoke (consumer, after TimeWarp.State.Plus package bump)**

In timewarp-architecture: Home → Settings → Profile → Settings (sidebar or
equivalent, not the crumb link).

**Expect:** breadcrumb reads `Home / Settings`, not a four-deep trail such as
`Home / Settings / Profile / Settings`. Browser Back to Profile then to Settings
should shrink the same way. Crumb-link GoBack still works.

**Not in scope:** bumping architecture or COPIC in this repo; stack-depth cap.

### Review disposition

- Body: tw-implementation-review, effort 1, roster `general` (grok-4.5 subagent, read-only); 1 round on `9f47e471` + kitchen `b67696b7` vs `origin/master`.
- Round 1: 0 bug, 0 suggestion, 0 nit. Merge pass confirmed `TruncateToOrPush` LIFO scan, required A/B/C truncate cases, same-URL title update, new-URL append, and unchanged GoBack task-059 clamp. `dotnet fixie timewarp-state-plus-tests`: 15 passed, 1 skipped.
- Final: 0 open; 0 fixed; 0 wontfix.
- **Disposition: clean** (`review/disposition.md`; framework `review/review-framework.md`; last ledger `review/round-1/merged.md`).
