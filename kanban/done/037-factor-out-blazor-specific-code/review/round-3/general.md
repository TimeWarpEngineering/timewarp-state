# Round 3 — general
**Date:** 2026-10-10
**Scope reviewed:** delta d0af155c..2a0725cf (post-disposition fix for PR #625 e2e failure; version back to 12.0.0-beta.10)

## Summary

The delta sets `StaticWebAssetBasePath` to `_content/TimeWarp.State` and renames the initializer to `TimeWarp.State.Blazor.lib.module.{ts,js,d.ts,js.map}`. Before the fix, assets were served from `/TimeWarp.State/` and the initializer did not match Blazor's `{PackageId}.lib.module.js` filter. It also moves the version and docs from 13.0.0-beta.1 back to 12.0.0-beta.10. A Release build of `timewarp-state-blazor` gives a `staticwebassets.build.json` with `BasePath` `_content/TimeWarp.State`. The renamed initializer has the `JSLibraryModule` trait and is in the `JSModuleManifest`. The repo no longer references `timewarp.state.lib.module` or `13.0.0-beta`, outside kanban history. The migration toc lists only 12.0.0-beta.10, and the release notes and migration guide cover 037, 100, 101, 099 and 622 as the task Notes require. Core `timewarp-state.csproj` declares no static web assets, so there is no base-path collision.

## Issues

None.
