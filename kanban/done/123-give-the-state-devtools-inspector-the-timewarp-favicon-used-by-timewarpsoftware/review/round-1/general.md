# Round 1 — general
**Date:** 2026-10-17
**Scope reviewed:** 04ead89d (`App.razor`, `timewarp-state-devtools.csproj`, `inspector-favicon-tests.cs`, `packaged-devtools-app-tests.cs`) and the task.md Results in 75705f36

## Summary

The change adds `<link rel="icon" type="image/png" href="logo.png" />` after `<base href="/" />` in `App.razor`. It links `assets/logo.png` into the inspector's static web assets as `wwwroot\logo.png`, and a small target points that asset's `ContentRoot` back at `assets/`. No second copy is committed. The SVG wordmark is left out, which the task allows. Two tests cover the head link, the served bytes, the nupkg entry, and the packaged inspector. Risk is low. There is one robustness nit in how the MSBuild target picks its asset.

## Verification

- `dotnet test tests/timewarp-state-devtools-tests`: 99 passed, 0 failed. This includes `InspectorFavicon_Should.ServeTheBrandPngFromTheHostPage` and `PackagedDevToolsApp_Should.StartFromAnOutOfTreeAppHost`.
- `obj/{Debug,Release}/net11.0/staticwebassets.build.json`: exactly one logo asset. Identity is `<repo>/assets/logo.png`, ContentRoot is `<repo>/assets/`, RelativePath is `logo#[.{fingerprint}]?.png`, and it is `Discovered`/`Primary`. No other asset matched the target's condition.
- `source/timewarp-state-devtools/wwwroot/` has no `logo.png`. `assets/logo.png` and `assets/logo.svg` are still the only originals.
- The packaged test asserts `tools/wwwroot/logo.png` is a single entry with the original bytes. So `CopyToPublishDirectory` plus static-web-asset publish does not produce a duplicate.
- `dotnet build source/timewarp-state-devtools`: 0 errors. Every warning is in a file this change does not touch (TW0007 in analyzer and core, BL0010/BL0016 in blazor, NU1510). The task's "0 warnings" proof gate is not met repo-wide, but those warnings were already on master, and the implementer recorded that in the checklist.
- `ganda repo audit`: passes, with one advisory `kebab-path-names` warning on paths this change does not touch.
- Goal restatement vs task: they match. The restatement treats the SVG as conditional, and so does the task. The restatement keeps the "0 warnings" gate from the task. No misread.

Reviewer note: a `--no-incremental` inspection build deleted the committed TypeScript output under `source/timewarp-state-blazor/wwwroot/{js,types}`. An incremental build of that project regenerated it, and `git status` matched HEAD again. This is how the repo already behaves, not something this change introduced.

## Issues

### Issue 1 — Severity: nit
- File: source/timewarp-state-devtools/timewarp-state-devtools.csproj:40
- Description: `PointDevToolsFaviconAtBrandFile` rewrites `ContentRoot` on every `StaticWebAsset` whose Identity ends with `assets/logo.png`. The target runs after `ResolveProjectStaticWebAssets`, so referenced-project assets are in that item list too. If a referenced RCL ever ships `wwwroot/assets/logo.png` (a common layout), that asset's ContentRoot would be pointed at the repo `assets/` folder, and its dev-manifest entry would break. No asset matches today.
- Suggestion: Match the exact source path, for example `Condition="'%(FullPath)' == '$(_BrandFaviconDirectory)logo.png'"`, or add `'%(SourceType)' == 'Discovered'`.
- Status: open
