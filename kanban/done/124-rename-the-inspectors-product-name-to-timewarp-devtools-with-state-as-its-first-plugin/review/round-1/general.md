# Round 1 — general
**Date:** 2026-10-17
**Scope reviewed:** commit `b05ababd` (26 files, +146/-56) and the task 124 Results commit `8707b408`.

## Summary

The change renames the inspector to TimeWarp DevTools everywhere a user sees it: `<PageTitle>`, the header `h1`, the Error page header, `DashboardLinkText`, the exception message, XML docs, package descriptions, readmes, topics, features, the two sample overviews, and the root readme. `DefaultResourceName` is now `timewarp-devtools`. The header has a `nav aria-label="Plugins"` row with one `State` label, and the existing tagline sits beside it. The CSS is scoped and there is no `<style>`. There is no plugin registry, route, or second plugin. Risk is low. The only behavior change is the default resource name, and the release note covers that break.

## Verification

- Goal restatement vs task: it matches. Every requirement in the task maps to a Done-when bullet, and the out-of-scope items (package ids, namespaces, API, env vars) are kept. No misread.
- `grep -rn "State DevTools" source samples documentation tests readme.md` (excluding `obj/` and `bin/`): the only hit is `documentation/release-notes/release12.0.0-beta.14.md`, which is history and stays as written.
- `git grep` for the old resource-name literal outside kanban and release notes: the only hits are folder paths (`source/timewarp-state-devtools/...`), which are out of scope.
- No code depends on the resource name. Inspected apps find the inspector through `TIMEWARP_STATE_DEVTOOLS_ENDPOINT`, which comes from `devtools.GetEndpoint(UiEndpointName)`, and through `/_timewarp/state-devtools`. There is no `services__<name>__` key.
- The `name` override still works. The new `UseACallerSuppliedResourceName` test covers both `AddTimeWarpStateDevTools` and `AddTimeWarpStateDevToolsProject`.
- `InspectorProductName_Should.RenderTheProductNameAndTheStatePlugin` asserts the prerendered `<title>`, the brand `h1`, the Plugins nav, the `State` label, and the tagline.
- `dotnet test tests/timewarp-state-devtools-tests`: 101 passed, 0 failed. `dotnet test tests/timewarp-state-devtools-isolation-tests`: 4 passed.
- `./bin/dev check-version`: source is `12.0.0-beta.16` and NuGet is `12.0.0-beta.15`, so it is safe to release. `<Version>` and `TimeWarpStateVersion` moved together.
- The release note and its toc entry name both breaks and the timewarp-architecture tests to update (IngressSmoke `StateDevTools`, `Publish_Should_HaveNoStateDevTools`, and label matches).
- Header styling: the `.plugin` border uses `rgba(255, 255, 255, 0.55)`, the same pattern as the existing `.theme` border. `--twe-header` and `--twe-header-ink` are not overridden in the light theme, so white on purple holds in both themes.

## Issues

<!-- None. -->
