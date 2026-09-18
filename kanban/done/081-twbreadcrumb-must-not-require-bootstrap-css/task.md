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

- [x] Isolated CSS (or equivalent) on `TwBreadcrumb`; no Bootstrap class contract
- [x] Sample 03 still demonstrates the trail; crumbs do not require `bootstrap.min.css`
- [x] Docs / sample 03 tutorial mention no Bootstrap requirement
- [x] Verify: render Sample 03 without Bootstrap CSS on the crumb — trail still readable and GoBack works
- [x] Implementation review (effort 1, general) under `review/`; disposition `clean`

## Session

- Created: ganda session 303480 (2026-09-02)
- Cockpit: grok `01a03d38-9611-7620-aae5-848e15dafa94` (timewarp-flow)
- Implementer: grok `01a0b23f-46e3-7662-947a-5c365a4ab5c4` (2026-09-18)
- Review oracle: grok `01a0b252-7c1c-72b0-b1b6-c87bf38de48b` (2026-09-18)
- Reviewer (general, round 1): grok `01a0b255-cc87-7651-aea9-7d3f7373274f` (2026-09-18)

## Notes

Architecture task **207** (timewarp-architecture) will put `TwBreadcrumb` + `TwPageTitle` in
`TimeWarpPage`. That host has Fluent UI v5 and must not load Bootstrap. This task is the library
fix so that consumption does not leak a Bootstrap CSS contract.

Implementation review (effort 1, general): `review/` — round 1 found no issues; disposition `clean`.

Related files:

- `source/timewarp-state-plus/features/routing/components/TwBreadcrumb.razor`
- `source/timewarp-state-plus/features/routing/components/TwPageTitle.razor`
- `samples/03-routing/wasm/sample-03-wasm/layout/MainLayout.razor`
- `samples/03-routing/wasm/sample-03-wasm/wwwroot/index.html`

## Results

`TwBreadcrumb` now owns `tw-breadcrumb` classes and ships `TwBreadcrumb.razor.css` (Blazor CSS
isolation). Bootstrap class names (`breadcrumb`, `breadcrumb-item`, `active`, `text-muted`) are
gone. `aria-label="breadcrumb"` and `aria-current="page"` remain. Behavior is unchanged: `MaxLinks`,
ellipsis, current page as text, ancestors call `RouteState.GoBack`. TimeWarp.State.Plus still does
not include Bootstrap.

Hosts load the trail via the usual `{ASSEMBLY}.styles.css` import of the RCL scoped-CSS bundle.
Optional `--tw-breadcrumb-divider` (default `/`) on a parent customizes the separator.

**Sample 03 styles link:** `wwwroot/index.html` pointed at `Sample03Wasm.styles.css` (404). The
generated bundle is `sample-03-wasm.styles.css` (assembly name from the kebab-case csproj). That
link is what imports `_content/TimeWarp.State.Plus/TimeWarp.State.Plus.*.bundle.scp.css`. Without
the fix, isolated CSS never loaded in the sample, so the trail still depended on Bootstrap.

### Files changed

- `source/timewarp-state-plus/features/routing/components/TwBreadcrumb.razor`
- `source/timewarp-state-plus/features/routing/components/TwBreadcrumb.razor.css` (new)
- `source/timewarp-state-plus/features/routing/components/tw-breadcrumb.md` (new)
- `source/timewarp-state-plus/readme.md`
- `samples/03-routing/wasm/sample-03-wasm/wwwroot/index.html`
- `samples/03-routing/wasm/overview.md`
- `documentation/topics/routing.md`
- `documentation/topics/toc.yml`
- `tests/timewarp-state-plus-tests/features/routing/tw-breadcrumb-style-tests.cs` (new)

### Key decisions

- Isolation (`*.razor.css`) rather than `wwwroot` CSS: native HTML root, automatic host import.
- No Bootstrap in Plus (confirmed absent from the packed nupkg).
- Sample 03 keeps `bootstrap.min.css` for starter chrome; the trail does not use it.
- Sample 03 stays on PackageReference (other samples); ProjectReference of Plus into the sample
  fails because the sample's `IncrementCount` action methods come from the package's source
  generator, not a project analyzer reference.

### Test outcomes

- `dotnet build source/timewarp-state-plus/timewarp-state-plus.csproj`: succeeded, 0 warnings.
- Packed `TimeWarp.State.Plus.12.0.0-beta.4.nupkg` contains
  `staticwebassets/TimeWarp.State.Plus.*.bundle.scp.css` with `.tw-breadcrumb`; no Bootstrap files.
- `dotnet fixie timewarp-state-plus-tests`: **16 passed, 1 skipped** (including the new style
  contract test and existing GoBack tests).
- Isolated-CSS fixture (nupkg bundle + component markup, **no Bootstrap stylesheet**):
  `display: flex`, `list-style: none`, divider `"/"`, trail `... / Home / Counter / Weather`.

Live Sample 03 WASM restored from the local Debug nupkg did not reach the trail: Blazor DI failed
with `LoadPersistentStateRequestHandler` missing `IPersistenceService`. That registration is
outside this CSS change. GoBack is covered by plus-tests; the click handler is unchanged.

### How to validate

**Smoke**

```bash
dotnet build source/timewarp-state-plus/timewarp-state-plus.csproj
# expect: Build succeeded, 0 errors. nupkg written to artifacts/packages/

python3 - <<'PY'
import zipfile, glob
p = glob.glob("artifacts/packages/TimeWarp.State.Plus.*.nupkg")[0]
z = zipfile.ZipFile(p)
names = z.namelist()
assert not any("bootstrap" in n.lower() for n in names)
css = [n for n in names if n.endswith(".bundle.scp.css")][0]
text = z.read(css).decode()
assert ".tw-breadcrumb" in text and "display: flex" in text
print("ok", css)
PY
# expect: ok staticwebassets/TimeWarp.State.Plus.*.bundle.scp.css

# Optional visual: copy the bundle + TwBreadcrumb markup into a page that does NOT
# link bootstrap.min.css. Expect a horizontal trail with / separators, not a numbered <ol>.
```

**Expect**

- `TwBreadcrumb.razor` has `class="tw-breadcrumb"` / `tw-breadcrumb__item`; no `breadcrumb-item` or
  `text-muted`. `aria-label="breadcrumb"` and `aria-current="page"` remain.
- Plus nupkg has the scoped-CSS bundle and **no** `bootstrap.min.css`.
- Sample 03 `index.html` links `sample-03-wasm.styles.css` (that file `@import`s the Plus bundle).
  `bootstrap.min.css` may stay for sidebar/buttons.
- Docs (`tw-breadcrumb.md`, Sample 03 tutorial, Plus readme, `documentation/topics/routing.md`)
  state hosts do not need Bootstrap.

**Automated**

```bash
dotnet fixie timewarp-state-plus-tests
# expect: 16 passed, 1 skipped (SkipExample)
```

**Not in scope:** Fluent UI host wiring (architecture task 207). Version bump / NuGet cut. Sample 03
still PackageReferences published `TimeWarp.State.Plus` until the next pack; run the library and
nupkg checks above to prove this change, not `dotnet run` of the sample against nuget.org beta.4.

### Review disposition

**Outcome:** `clean` (0 open findings; no `wontfix`)
**Effort:** 1 (general only)
**Rounds:** 1
**Roster:** general (`review/round-1/general.md`)

| Severity | open | fixed | wontfix |
|----------|------|-------|---------|
| bug | 0 | 0 | 0 |
| suggestion | 0 | 0 | 0 |
| nit | 0 | 0 | 0 |

Round 1 found no issues. No fix loop. No escalations.

**Review paths**

- `kanban/in-progress/081-twbreadcrumb-must-not-require-bootstrap-css/review/review-framework.md`
- `kanban/in-progress/081-twbreadcrumb-must-not-require-bootstrap-css/review/round-1/general.md`
- `kanban/in-progress/081-twbreadcrumb-must-not-require-bootstrap-css/review/round-1/merged.md`
- `kanban/in-progress/081-twbreadcrumb-must-not-require-bootstrap-css/review/disposition.md`
