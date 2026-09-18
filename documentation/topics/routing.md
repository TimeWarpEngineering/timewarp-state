---
uid: TimeWarp.State:Routing.md
title: Enable Routing
---

# Routing

TimeWarp.State.Plus includes `RouteState` and two layout/page components:

- `TwPageTitle` — records the page title onto the route stack
- `TwBreadcrumb` — renders that stack as a horizontal trail (`MaxLinks`, ellipsis, `RouteState.GoBack` on ancestors)

`TwBreadcrumb` ships isolated CSS under `tw-breadcrumb` classes. Hosts do **not** need Bootstrap (or any other CSS framework) for the trail to be readable. Keep the host `{ASSEMBLY}.styles.css` link so the Razor class library scoped-CSS bundle loads.

See the [Routing Tutorial](xref:TimeWarp.State:03-Routing.md) (Sample 03) and `source/timewarp-state-plus/features/routing/components/tw-breadcrumb.md`.
