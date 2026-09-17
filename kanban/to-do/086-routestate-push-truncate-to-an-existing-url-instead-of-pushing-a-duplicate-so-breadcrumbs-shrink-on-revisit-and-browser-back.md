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

- [ ] Truncate-to-existing-URL on push + tests
- [ ] `dev build` 0/0; library tests green; `ganda repo audit` clean
- [ ] Results and How to validate (architecture: Home → Settings → Profile → Settings shows
      "Home / Settings", not a four-deep trail)

## Session

- Created: cockpit (2026-09-16); brief written 2026-09-17
- Claude Code cockpit session: https://claude.ai/code/session_01KPZXyAmA6Vk99W1yUQUn1N

## Notes

- Files: `source/timewarp-state-plus/features/routing/route-state/route-state.push-route-info.cs`,
  `route-state.go-back.cs`, `components/TwBreadcrumb.razor`, `components/TwPageTitle.razor`;
  tests near the GoBack clamp tests (task 059).
- Consumers: timewarp-architecture `web-spa/components/TimeWarpPage.razor`; COPIC
  `Web.Spa/Components/Pages/CopicPage/{CopicPage.razor,MainContentArea/BreadCrumbs.razor}`.

## Results

_Pending._

### How to validate

_Pending._
