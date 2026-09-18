# TwBreadcrumb Component

## Overview

`TwBreadcrumb` renders `RouteState` history as a horizontal trail. Ancestors call `RouteState.GoBack`; the current page is text with `aria-current="page"`.

## Styling

The component ships **isolated CSS** (`TwBreadcrumb.razor.css`) under component-owned classes (`tw-breadcrumb`, `tw-breadcrumb__item`, `tw-breadcrumb__item--current`, `tw-breadcrumb__ellipsis`).

Hosts **do not need Bootstrap** (or any other CSS framework) for the trail to be readable. TimeWarp.State.Plus does not include Bootstrap.

Keep the host's `{ASSEMBLY}.styles.css` link (Blazor's default) so the Razor class library scoped-CSS bundle loads. Optional: set `--tw-breadcrumb-divider` on a parent to change the separator (default `/`).

## Usage

```razor
<TwBreadcrumb MaxLinks="3" />
```

- `MaxLinks` — when greater than 0, show the most recent N entries and an ellipsis if the stack is longer. `0` or negative shows the full stack.
- Place the component in a layout so it updates with navigation.
- Pair with `TwPageTitle` on pages so crumbs have titles.
