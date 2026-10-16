# Give the State DevTools inspector the TimeWarp favicon used by timewarp.software

## Description

The DevTools inspector tab shows a generic globe icon next to "TimeWarp State DevTools". Steve reported this on 2026-10-16. The host page has no icon link and the inspector's `wwwroot` has no icon file, so browsers fall back to `/favicon.ico` (404) and show the default.

Findings (read-only investigation, 2026-10-16):

- timewarp.software (`curl -sL https://timewarp.software`) declares exactly one icon: `<link rel="icon" href="/assets/logos/logo.png" type="image/png" />`. There is no SVG icon link, no `.ico`, no `apple-touch-icon`, and no web manifest. `/favicon.ico` returns 404. The nav brand image is `/assets/logos/timewarp-enterprises.svg` (200, `image/svg+xml`). `/assets/logos/logo.png` is 200, `image/png`.
- Site source repo: `/home/steve/worktrees/github.com/TimeWarpEngineering/timewarp-software/master`. Originals: `public/assets/logos/logo.png` (64x64 RGBA PNG, the favicon) and `public/assets/logos/timewarp-enterprises.svg` (Inkscape SVG, viewBox 264.58x211.67, the logo). Added in that repo by task 009 (commit 4ae159ad), copied from timewarp.enterprises.
- This repo already holds the same originals. `assets/logo.png` is byte-identical to the site `logo.png` (`cmp` clean) and `assets/logo.svg` is byte-identical to the site `timewarp-enterprises.svg`. This is the shared brand-asset location. Use it. Do not copy from a scraped URL.
- Host page: `source/timewarp-state-devtools/components/App.razor` (`<head>` has `<base>`, `tokens.css`, `app.css`, the scoped styles link, `theme.js`, `<HeadOutlet />`; no icon). Static assets: `source/timewarp-state-devtools/wwwroot/` (`app.css`, `theme.js`, `tokens.css`). Project: `source/timewarp-state-devtools/timewarp-state-devtools.csproj` (Sdk.Web, published into the `TimeWarp.State.DevTools.App` package under `tools/` by `source/timewarp-state-devtools-app/timewarp-state-devtools-app.csproj`, started by `source/timewarp-state-devtools-hosting`).
- Samples already use a 32x32 `wwwroot/favicon.png` (for example `samples/00-state-action-handler/wasm/sample-00-wasm/wwwroot/favicon.png`). That is a per-sample copy, not the brand source.

## Requirements

- The inspector shows the TimeWarp favicon in the browser tab, matching timewarp.software: the 64x64 `logo.png` as `<link rel="icon" type="image/png">`.
- Add an SVG icon (`<link rel="icon" type="image/svg+xml">`) from `assets/logo.svg`, with the PNG as fallback. The site has no SVG, ICO, or apple-touch-icon, so do not invent ICO or apple-touch variants. If the SVG renders poorly at 16 px (it is a 24 KB Inkscape file), drop the SVG link and ship the PNG only, and record that in Notes.
- Single source of truth: `assets/logo.png` and `assets/logo.svg` stay the only originals. The inspector's `wwwroot` gets them by an MSBuild link (a `<Content Include="..\..\assets\logo.png" Link="wwwroot\..." />` or `StaticWebAssetBasePath` item), not a second committed copy. If a link does not work with static web assets, add a build check that the copy equals the original (when two things must agree, generate one from the other or check it).
- Icon links go in the `<head>` of `App.razor` with relative hrefs that work under `<base href="/">`.
- The assets ship in the packed `TimeWarp.State.DevTools.App` package under `tools/` (publish output `wwwroot/`, or the static web assets manifest layout the app already uses) and are served when the AppHost runs the packaged inspector, not only under `dotnet run`.
- No new packages. No Tailwind. No `<style>` elements.

## Out of Scope

- Changing the timewarp.software favicon or adding an ICO, manifest, or apple-touch-icon there.
- Redesigning or recolouring the logo, or a dark-mode icon variant.
- Favicons for the samples or the test apps.

## Checklist

- [ ] Wire `assets/logo.png` and `assets/logo.svg` into the inspector's static web assets (link, no committed duplicate)
- [ ] Add the icon `<link>` elements to `App.razor` `<head>`
- [ ] Check the SVG at 16 px and 32 px, keep or drop it
- [ ] Confirm the files are in the packed `TimeWarp.State.DevTools.App` nupkg `tools/` content
- [ ] Add a test: rendered host page head contains the icon link(s), and the icon URL returns 200 with the right content type
- [ ] Packaged-inspector check: run the packaged inspector through the AppHost path and fetch the icon URL
- [ ] Visual check of the browser tab, light and dark browser chrome
- [ ] `dev build` is 0 warnings and 0 errors
- [ ] Existing `timewarp-state-devtools-tests` pass

## Proof Gates

- `dev build`: 0 warnings, 0 errors.
- A test (`tests/timewarp-state-devtools-tests`) shows the host page `<head>` contains the icon link and a request for its href returns 200 with `image/png` (and `image/svg+xml` if kept).
- The packed `TimeWarp.State.DevTools.App` nupkg lists the icon files under `tools/`, and the same URL returns 200 from the packaged inspector started by the hosting package. If an AppHost is needed, this is run by the reviewer, not by a headless worker (see the worktree AppHost user-secrets note).
- Visual check note in `## Session`: the tab shows the TimeWarp logo instead of the globe (hard refresh or clear site data first, since browsers cache favicons).
- `ganda repo audit` clean.

## Session

- Created: 2026-10-16

## Notes

Reported by Steve 2026-10-16. Source of truth for the brand assets in this repo is `assets/logo.png` and `assets/logo.svg`; the site repo copies are identical as of 2026-10-16.
