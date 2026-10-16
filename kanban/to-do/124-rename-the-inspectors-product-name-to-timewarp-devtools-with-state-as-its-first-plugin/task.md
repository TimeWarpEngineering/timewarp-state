# Rename the inspector's product name to TimeWarp DevTools, with State as its first plugin

## Description

Decision (Steve, 2026-10-16): the product is **TimeWarp DevTools**. State is only its first plugin. Future plugins come from the "DevTools shell and plugins" remainder of timewarp-architecture task 335. Today the product reads as "State DevTools" everywhere a user sees it:

- Header: eyebrow "TimeWarp", h1 "State DevTools", tagline "Current state, and the action that produced it."
- Browser tab title: "TimeWarp State DevTools"
- Aspire dashboard link text: "TimeWarp.State DevTools" (the resource itself is `timewarp-state-devtools`)

This task renames the product in display strings and the Aspire resource name. It does not rename packages, namespaces, or the `AddTimeWarpStateDevTools` API (see Open Questions).

### Occurrences found (read-only investigation, 2026-10-16)

(a) Display strings, change these:

- `source/timewarp-state-devtools/components/pages/Inspector.razor` line 283 `<PageTitle>TimeWarp State DevTools</PageTitle>` (the tab title; `App.razor` has no `<title>`, `<HeadOutlet />` supplies it).
- Same file, header markup lines 286-292: `<p class="eyebrow">TimeWarp</p>`, `<h1>State DevTools</h1>`, `<p class="lede">Current state, and the action that produced it.</p>`.
- `source/timewarp-state-devtools-hosting/add-timewarp-state-dev-tools.cs` line 51 `DashboardLinkText = "TimeWarp.State DevTools"` (Aspire dashboard link text), plus the exception message on line 174 ("TimeWarp.State DevTools inspector path is not set...") and the XML doc on line 142.
- Package descriptions: `source/timewarp-state-devtools-hosting/timewarp-state-devtools-hosting.csproj` line 4 and `source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj` line 4.
- Readmes: `source/timewarp-state-devtools-hosting/readme.md` (lines 3, 19), `source/timewarp-state-devtools-app/readme.md` (line 3), `source/timewarp-state-devtools-export/readme.md` (line 3), `source/timewarp-state-devtools-wasm/readme.md` (line 3), `source/timewarp-state-telemetry/readme.md` (line 114), `source/timewarp-state-devtools/readme.md`.
- Docs: `documentation/topics/devtools.md` (front matter `title:` line 3, line 8), `documentation/topics/telemetry.md` (line 40), `documentation/features/features.md` (line 21), `samples/03-routing/wasm/overview.md` (lines 51, 183), `samples/04-telemetry/overview.md` (line 32, also names the resource).
- Past release note `documentation/release-notes/release12.0.0-beta.14.md` line 13 mentions the old text. It is history: leave it.
- Tests asserting display strings: `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs` line 196 `body.ShouldContain("TimeWarp State DevTools")`; `tests/timewarp-state-devtools-tests/hosting-registration-tests.cs` lines 244 and 386 compare to `DashboardLinkText` (they follow the constant).

(b) Aspire resource name (public-ish contract):

- `TimeWarpStateDevToolsHosting.DefaultResourceName = "timewarp-state-devtools"` at `add-timewarp-state-dev-tools.cs` line 42. Used as the default `name` of `AddTimeWarpStateDevTools` (line 161) and `AddTimeWarpStateDevToolsProject` (line 228).
- Tests: `hosting-registration-tests.cs` lines 33 and 214 assert `Resource.Name == DefaultResourceName` (follow the constant).
- Downstream: timewarp-architecture tests reference the resource by name (its IngressSmoke `StateDevTools` test and `Publish_Should_HaveNoStateDevTools`). They break on the next package bump and must be updated there. Note that an unchanged `DashboardLinkText` is also matched by label in tests; check downstream for both.
- Docs naming the resource: `samples/04-telemetry/overview.md` line 32.

(c) Package IDs, namespaces, types (OUT of scope, listed for the record): `TimeWarp.State.DevTools.App`, `TimeWarp.State.DevTools.Hosting`, `TimeWarp.State.DevTools.Export`, `TimeWarp.State.DevTools.Wasm`, the namespace `TimeWarp.State.DevTools.Components`, the stylesheet link `TimeWarp.State.DevTools.styles.css` in `App.razor`, `TimeWarpStateDevToolsHosting`, `TimeWarpStateDevToolsHostingExtensions`, `TimeWarpStateDevToolsExport`, `TimeWarpStateDevToolsWasm`, `TimeWarpStateDevToolsClientConfig*`, `AddTimeWarpStateDevTools`, `AddTimeWarpStateDevToolsProject`, `WithTimeWarpStateDevTools`, `AddTimeWarpStateDevToolsExporter`, `AddTimeWarpStateDevToolsWasmExporter(Async)`, `MapTimeWarpStateDevToolsClientConfig`, env vars `TIMEWARP_STATE_DEVTOOLS_*`, the discovery path `/_timewarp/state-devtools`, the CORS policy name `TimeWarpStateDevToolsIngest`, the assembly metadata key `timewarpstatedevtoolspath`, and the project and folder names `timewarp-state-devtools*`.

## Requirements

- Display strings: "State DevTools", "TimeWarp State DevTools" and "TimeWarp.State DevTools" become "TimeWarp DevTools" in every item under (a) above, except history (past release notes).
- The browser tab title (`<PageTitle>`) is exactly "TimeWarp DevTools".
- Header: the brand reads "TimeWarp DevTools". The existing inspector view is labelled as the "State" plugin (a section or tab label), with the existing tagline "Current state, and the action that produced it." kept under or beside it. The implementer designs a minimal header that leaves room for future plugin tabs. Do not build a plugin system, a plugin registry, routing per plugin, or a second plugin. One label is enough. Follow `tw-blazor-css-strategy` (scoped CSS, no `<style>`).
- Aspire resource name: `DefaultResourceName` becomes `"timewarp-devtools"` (kebab, no product word "state"). `DashboardLinkText` becomes `"TimeWarp DevTools"`. Record the exact values in Notes when done.
- Keep the `name` parameter override working so callers can still pass their own resource name.
- Update `samples/04-telemetry/overview.md` and the docs and readmes under (a) to the new resource name and display text.
- Breaking change: add a release note (new `documentation/release-notes/` file for the next version, following the existing pattern, and the toc) that states the default resource name changed from `timewarp-state-devtools` to `timewarp-devtools` and the dashboard text changed. State that timewarp-architecture must update its tests (IngressSmoke `StateDevTools` test, `Publish_Should_HaveNoStateDevTools`, and any label match) when it takes the next bump.
- Version: run `dev check-version` per the PR policy (this is a breaking change to a shipped default; bump `<Version>` and platform pins together if the check requires it).
- Do not touch package IDs, namespaces, or the `AddTimeWarpStateDevTools` family of APIs.

## Out of Scope

- Renaming package IDs, namespaces, folder/project names, env vars, or the `AddTimeWarpStateDevTools` / `WithTimeWarpStateDevTools` API surface to plugin-neutral names (see Open Questions).
- Building the DevTools shell or plugin system (timewarp-architecture task 335 remainder).
- Editing past release notes.
- Task 122 (action-log labels) and task 123 (favicon) work, except for coordination below.

## Open Questions for Steve

- Should package IDs, namespaces, and the API (`AddTimeWarpStateDevTools`, `WithTimeWarpStateDevTools`, `TimeWarp.State.DevTools.*`) move to plugin-neutral names such as `TimeWarp.DevTools.*`, with State as a plugin package? This is an architecture decision tied to the plugin shell (task 335). Not decided here. The implementer must not guess.

## Coordination

Tasks 122 (header and layout work in `Inspector.razor` and its scoped CSS), 123 (favicon in `App.razor`, and its text still says "TimeWarp State DevTools"), and this task touch the same inspector files. Run 122, 123, and 124 as one serial stack (`ganda task work` work-set into one feature branch), never in parallel. Suggested order: 122, 123, 124, so the header labels from 122 are renamed once. 123's description is history and stays as written.

## Checklist

- [x] Change the header brand and add the "State" section label with the tagline (`Inspector.razor` and scoped CSS)
- [x] Change `<PageTitle>` to "TimeWarp DevTools"
- [x] Change `DefaultResourceName` to `timewarp-devtools` and `DashboardLinkText` to `TimeWarp DevTools`; update the exception message and XML docs
- [x] Update package descriptions in the hosting and app csproj files
- [x] Update readmes, `documentation/topics/devtools.md`, `telemetry.md`, `features.md`, and the two sample overviews
- [x] Update tests: packaged-app test `ShouldContain("TimeWarp DevTools")`; hosting tests follow the constants
- [x] Add a check that the rendered `<title>` and the header brand both read "TimeWarp DevTools"
- [x] Add the breaking-change release note and toc entry
- [x] `dev check-version`, and bump if required
- [x] `dev build` is 0 warnings and 0 errors
- [x] `timewarp-state-devtools-tests` and `timewarp-state-devtools-isolation-tests` pass
- [x] `ganda repo audit` clean

## Proof Gates

- `dev build`: 0 warnings, 0 errors.
- Tests updated for the new strings and the new default resource name, and passing.
- A test shows the rendered page `<title>` is "TimeWarp DevTools" and the header brand (the `h1` or equivalent) reads "TimeWarp DevTools", with a "State" section label present.
- `grep -rn "State DevTools" source samples documentation --include=* ` finds only history (past release notes) and no display string.
- Release note states the resource-name break and the downstream timewarp-architecture test updates.
- `ganda repo audit` clean. Any AppHost run is done by the reviewer, not a headless worker (worktree AppHost user-secrets note).

## Results

The inspector product is TimeWarp DevTools. The header `h1` is that name. State is one label in a plugin row, with the tagline beside it. The browser `<title>` is TimeWarp DevTools. The error page uses the same product name.

`TimeWarpStateDevToolsHosting.DefaultResourceName` is `timewarp-devtools`. `DashboardLinkText` is `TimeWarp DevTools`. `AddTimeWarpStateDevTools` and `AddTimeWarpStateDevToolsProject` still take a caller-supplied `name`.

`12.0.0-beta.15` is already on NuGet, so `<Version>` and `TimeWarpStateVersion` are `12.0.0-beta.16`. `documentation/release-notes/release12.0.0-beta.16.md` records the resource-name break, the dashboard-text change, and the timewarp-architecture tests to update (IngressSmoke `StateDevTools`, `Publish_Should_HaveNoStateDevTools`, and any label match). Package ids, namespaces, and the `AddTimeWarpStateDevTools` API are unchanged.

`./bin/dev build` exited 0 with 0 errors and 154 warnings. Those warnings are the pre-existing NuGet advisories, TW0007 hints, RS0030 `Console` bans, and nullable warnings. None are in the files this task edited. `ganda repo audit` exited 0. The kebab-path advisory is the existing Blazor module filenames, the DevTools targets file, and task 110's folder.

### How to validate

Smoke: From the repo root, `dotnet test tests/timewarp-state-devtools-tests/timewarp-state-devtools-tests.csproj` and `dotnet test tests/timewarp-state-devtools-isolation-tests/timewarp-state-devtools-isolation-tests.csproj`. Then `./bin/dev check-version` and `grep -rn "State DevTools" source samples documentation --include='*'`.

Expect: DevTools tests: 101 passed, 0 failed. Isolation tests: 4 passed. `check-version` prints source `12.0.0-beta.16`, one prerelease increment ahead of NuGet `12.0.0-beta.15`, and "Version in source is new — safe to release." `InspectorProductName_Should.RenderTheProductNameAndTheStatePlugin` reads `<title>` and the brand `h1` as `TimeWarp DevTools`, with a State plugin label and the tagline. On a tree without `obj/`, the grep hits only `documentation/release-notes/release12.0.0-beta.14.md`. A built nuspec joins the separate tags `TimeWarp.State` and `DevTools` with a space; those tags are not the product name.

## Session

- Created: 1393397 (2026-10-16)
- Implementation: grok (2026-10-17)

## Goal (implementer restatement)

The inspector is the product **TimeWarp DevTools**. State is a label on the view that already exists, not the product name. A person opening the page, the browser tab, the Aspire dashboard link, or the docs should read TimeWarp DevTools. The action-log tagline stays with the State label. Package ids, namespaces, folder names, env vars, and `AddTimeWarpStateDevTools` stay as they are.

Done when:

- The rendered page title and the header brand are exactly `TimeWarp DevTools`, and a State section label sits with the existing tagline. The header is one label in a row that can hold more labels later. There is no plugin registry, route, or second plugin.
- The default Aspire resource name is `timewarp-devtools` and the dashboard link text is `TimeWarp DevTools`. A caller can still pass a different resource name.
- Display copy in the listed readmes, package descriptions, topics, features page, and the two sample overviews uses TimeWarp DevTools. The telemetry sample names the resource `timewarp-devtools`. Past release notes stay unchanged.
- A release note for the version this ships in records the resource-name and dashboard-text break, and names the timewarp-architecture tests that must change on the next bump (`IngressSmoke` `StateDevTools`, `Publish_Should_HaveNoStateDevTools`, and any label match).
- `dev check-version` passes. `<Version>` and `TimeWarpStateVersion` move together only if that check requires a bump.
- `dev build` is clean, the DevTools and isolation test projects pass, and `ganda repo audit` is clean.

## Notes

Decision recorded from Steve, 2026-10-16. No in-repo collision with the proposed id.

- `TimeWarpStateDevToolsHosting.DefaultResourceName` = `timewarp-devtools`
- `TimeWarpStateDevToolsHosting.DashboardLinkText` = `TimeWarp DevTools`
