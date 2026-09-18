# Round 1 — general
**Date:** 2026-09-18
**Scope reviewed:** branch `task/081-twbreadcrumb-must-not-require-bootstrap-css` vs `origin/master`

## Summary

`TwBreadcrumb` no longer depends on Bootstrap’s breadcrumb class contract: markup uses `tw-breadcrumb*` classes, and `TwBreadcrumb.razor.css` ships a horizontal trail via Blazor CSS isolation (scoped bundle present in the Plus nupkg; no Bootstrap files or deps). Behavior (`MaxLinks`, ellipsis, current page as text, ancestor `RouteState.GoBack`) is unchanged; `aria-label="breadcrumb"` and `aria-current="page"` remain. Sample 03 links `sample-03-wasm.styles.css` (which `@import`s the Plus scp bundle) while keeping `bootstrap.min.css` only for starter chrome; docs state hosts do not need Bootstrap. Risk is low; `dotnet fixie timewarp-state-plus-tests` reported 16 passed, 1 skipped.

## Issues

No issues found.
