# Give the State DevTools inspector the TimeWarp favicon used by timewarp.software

## Goal (implementer restatement)

The inspector tab shows a generic globe. `App.razor` never declares an icon, and the inspector ships no icon file, so the browser requests `/favicon.ico`, gets nothing, and draws the default.

This is done when:

- The tab uses the TimeWarp mark timewarp.software already uses: the 64×64 `assets/logo.png`, as `<link rel="icon" type="image/png">` in `App.razor`, with a relative href that resolves under `<base href="/">`.
- `assets/logo.svg` (the wide TIMEWARP ENTERPRISES wordmark) is checked at 16 px and 32 px. If it does not read as a tab icon, the page ships the PNG only, and Notes record that. No ICO, manifest, or apple-touch icon.
- `assets/logo.png` and `assets/logo.svg` stay the only committed originals. The inspector pulls the served file in with an MSBuild link. There is no second copy under `wwwroot`.
- The packed `TimeWarp.State.DevTools.App` package contains that file under `tools/`, and the inspector started from that package returns it with status 200 and `image/png`.
- A test in `tests/timewarp-state-devtools-tests` shows the host page `<head>` contains the icon link and a request for its href returns 200 with the right content type.
- `dev build` reports 0 warnings and 0 errors, the existing DevTools tests pass, and `ganda repo audit` is clean.

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

- [x] Wire `assets/logo.png` into the inspector's static web assets (link, no committed duplicate). SVG is not shipped; see Notes
- [x] Add the icon `<link>` element to `App.razor` `<head>`
- [x] Check the SVG at 16 px and 32 px and drop it
- [x] Confirm the PNG is in the packed `TimeWarp.State.DevTools.App` nupkg `tools/` content
- [x] Add a test: rendered host page head contains the icon link, and the icon URL returns 200 with `image/png`
- [x] Packaged-inspector check: `PackagedDevToolsApp_Should.StartFromAnOutOfTreeAppHost` fetches the icon URL from the packaged inspector
- [x] Visual check of the icon at 16 px and 32 px on light and dark backgrounds
- [x] `dev build` exits 0 with 0 errors. It reports 154 warnings, all pre-existing, none in this change
- [x] Existing `timewarp-state-devtools-tests` pass (99)

## Proof Gates

- `dev build`: 0 warnings, 0 errors.
- A test (`tests/timewarp-state-devtools-tests`) shows the host page `<head>` contains the icon link and a request for its href returns 200 with `image/png` (and `image/svg+xml` if kept).
- The packed `TimeWarp.State.DevTools.App` nupkg lists the icon files under `tools/`, and the same URL returns 200 from the packaged inspector started by the hosting package. If an AppHost is needed, this is run by the reviewer, not by a headless worker (see the worktree AppHost user-secrets note).
- Visual check note in `## Session`: the tab shows the TimeWarp logo instead of the globe (hard refresh or clear site data first, since browsers cache favicons).
- `ganda repo audit` clean.

## Results

The inspector tab icon is `assets/logo.png`, the same 64×64 TimeWarp mark timewarp.software uses. `App.razor` links it as `<link rel="icon" type="image/png" href="logo.png" />` after `<base href="/" />`. The project links that file into static web assets and points the asset's content root at `assets/`, so development and publish both serve the original. There is no second copy under `wwwroot`.

`assets/logo.svg` is not linked. It is the wide TIMEWARP ENTERPRISES wordmark. At 16 px and 32 px, on white and on near-black, it shrinks to a speck. The PNG stays readable at those sizes.

`InspectorFavicon_Should.ServeTheBrandPngFromTheHostPage` reads the host page head and requests `/logo.png`. `PackagedDevToolsApp_Should.StartFromAnOutOfTreeAppHost` checks the nupkg lists `tools/wwwroot/logo.png` with the original bytes, then starts the packaged inspector and requests the same URL.

### Review

The review ran one round at effort 1 with one `general` reviewer. Final counts: bug 0, suggestion 0, nit 1 (wontfix), open 0. Disposition: `accepted-exceptions`.

M1 is a nit. `PointDevToolsFaviconAtBrandFile` matches any static web asset whose path ends with `assets/logo.png`, including assets from referenced projects. Today it hits only the brand file. It was marked wontfix as latent only. Tighten it to the exact path if the target is touched again.

During review, the DevTools tests passed (99/99), `dotnet build` of the inspector had 0 errors with no warnings in changed files, and `ganda repo audit` passed. The proof gate asks for 0 warnings. It is not met repo-wide, because master already has warnings in files this change does not touch.

Paths:

- `review/review-framework.md`
- `review/round-1/general.md`
- `review/round-1/merged.md`
- `review/disposition.md`

### How to validate

Smoke: From a clean browser profile, run the inspector (`dotnet run` on `source/timewarp-state-devtools`, or the packaged dll from `TimeWarp.State.DevTools.App` `tools/` with that directory as the working directory). Open `/`. Hard-refresh once so a cached globe is not reused.

Expect: The document head contains `<link rel="icon" type="image/png" href="logo.png">`. `GET /logo.png` returns 200 with `Content-Type: image/png` and the bytes of `assets/logo.png`. The tab icon is the purple and blue infinity mark with the green swoosh, on both light and dark browser chrome. There is no SVG, ICO, or apple-touch icon link.

## Session

- Created: 2026-10-16
- Implementation: 2026-10-17
- Visual check: `dotnet run` on the inspector (Development, `http://127.0.0.1:5288/`). The head links `logo.png`. `GET /logo.png` was 200 `image/png` and matched `assets/logo.png`. Chromium painted that response at 16 px and 32 px on white and on `#1e1e1e`: the TimeWarp infinity mark, not a globe. The page itself loaded the State DevTools inspector. An X11 grab of this display was black, so the window chrome was not photographed. The profile was new, so no cached globe was in play.
- Implement oracle: implemented by implementer-grok (grok, model grok-4.7), session not reported, max-turns 500, Done — 2026-10-16T17:24:13Z
- Review oracle: round 1, effort 1, roster `general` (claude-opus-5-5, headless `ganda task work`, session id not exposed). Disposition `accepted-exceptions`, with 0 open and M1 nit wontfix. Recorded 2026-10-17.

## Notes

Reported by Steve 2026-10-16. Source of truth for the brand assets in this repo is `assets/logo.png` and `assets/logo.svg`; the site repo copies are identical as of 2026-10-16.

SVG check, 2026-10-17: `assets/logo.svg` is an Inkscape wordmark (viewBox 264.58×211.67, about 83 KB). Rendered at 16 px and 32 px on white and on `#1e1e1e`, the wordmark does not read as an icon. The 64×64 `logo.png` does. The inspector ships the PNG only. No ICO, manifest, or apple-touch icon.

`DefineStaticWebAssets` pins a linked file's content root at the project's `wwwroot`, so the dev manifest would look for `wwwroot/logo.png`. `PointDevToolsFaviconAtBrandFile` points that asset back at `assets/` before the manifests are written. Publish still copies the original into `tools/wwwroot/logo.png`.
