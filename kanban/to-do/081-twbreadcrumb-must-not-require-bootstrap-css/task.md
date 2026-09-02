# TwBreadcrumb must not require Bootstrap CSS

## Description

`TwBreadcrumb` in TimeWarp.State.Plus is a RouteState history trail. It is **not** a Bootstrap package
reference — Plus has no Bootstrap NuGet/CSS in the nupkg. It **does** copy Bootstrap's breadcrumb
**class contract**, so the trail only looks like a breadcrumb when the host already loads
`bootstrap.min.css`.

Markup today (`source/timewarp-state-plus/features/routing/components/TwBreadcrumb.razor`):

- `<nav aria-label="breadcrumb">`
- `<ol class="breadcrumb">`
- `<li class="breadcrumb-item">` / `breadcrumb-item active`
- ellipsis uses `text-muted`

There is no `TwBreadcrumb.razor.css`. Sample 03 (the documented consumer) loads
`css/bootstrap/bootstrap.min.css` in `wwwroot/index.html` and puts `<TwBreadcrumb MaxLinks=3 />`
in `MainLayout`. Architecture (Fluent UI v5, no Bootstrap) cannot consume this without either
shipping Bootstrap or getting an unstyled `<ol>`.

Decouple the component from Bootstrap class names. Ship isolated CSS so the trail is readable in
any host.

## Requirements

- `TwBreadcrumb` must render a usable horizontal trail **without** Bootstrap CSS in the host.
- Do **not** add Bootstrap (package, CDN, or `bootstrap.min.css`) to TimeWarp.State.Plus.
- Replace Bootstrap class names (`breadcrumb`, `breadcrumb-item`, `active`, `text-muted`) with
  component-owned classes (e.g. `tw-breadcrumb` / CSS isolation). Keep `aria-label="breadcrumb"`
  and `aria-current="page"` on the current item.
- Preserve behavior: `MaxLinks`, ellipsis, current page as text, ancestors call `RouteState.GoBack`.
- Sample 03 trail must still look like a breadcrumb when Bootstrap is **not** what styles it
  (host template may keep Bootstrap for the rest of the Blazor starter chrome; the trail must not
  depend on that).
- Document that hosts do not need Bootstrap for `TwBreadcrumb`.

## Checklist

- [ ] Isolated CSS (or equivalent) on `TwBreadcrumb`; no Bootstrap class contract
- [ ] Sample 03 still demonstrates the trail; crumbs do not require `bootstrap.min.css`
- [ ] Docs / sample 03 tutorial mention no Bootstrap requirement
- [ ] Verify: render Sample 03 without Bootstrap CSS on the crumb — trail still readable and GoBack works

## Session

- Created: ganda session 303480 (2026-09-02)
- Cockpit: grok `01a03d38-9611-7620-aae5-848e15dafa94` (timewarp-flow)

## Notes

Architecture task **207** (timewarp-architecture) will put `TwBreadcrumb` + `TwPageTitle` in
`TimeWarpPage`. That host has Fluent UI v5 and must not load Bootstrap. This task is the library
fix so that consumption does not leak a Bootstrap CSS contract.

Related files:

- `source/timewarp-state-plus/features/routing/components/TwBreadcrumb.razor`
- `source/timewarp-state-plus/features/routing/components/TwPageTitle.razor`
- `samples/03-routing/wasm/sample-03-wasm/layout/MainLayout.razor`
- `samples/03-routing/wasm/sample-03-wasm/wwwroot/index.html`
