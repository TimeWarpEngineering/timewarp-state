# Contributing Overview

Open a [discussion](https://github.com/TimeWarpEngineering/timewarp-state/discussions) before starting work.

## Documentation links

DocFX builds `documentation/docfx.json`. Paths are case-sensitive. Folders and markdown files are kebab-case. Blazor `.razor` files keep the component type name (`App.razor`, `Counter.razor`, `MainLayout.razor`).

| Kind | Form |
| --- | --- |
| Article in this docset | Relative markdown link to a file listed in `docfx.json` content, for example `[Render control](../topics/render-control.md)` |
| Topic cross-reference | `xref:` plus the target file's YAML `uid`, for example `[Routing](xref:TimeWarp.State:Routing.md)` |
| TOC entry | `topicUid` set to that same `uid`, or `href` to a directory that contains `toc.yml` (`topics/`, `release-notes/`, `migrations/`) |
| Include | `[!include[Name](partials/badges.md)]` with the real relative path |
| Code from a sample | `[!code-csharp[Name](../../samples/00-state-action-handler/wasm/sample-00-wasm/program.cs)]` — the file must exist on disk |
| Source file outside the docset | Absolute GitHub URL (`blob` for a file, `tree` for a directory). A relative link to `.cs`, `.razor`, or a project folder is an invalid file link |
| Sample article | Link to `overview.md` or `readme.md`, not a bare directory and not a renamed PascalCase folder |

A relative link to a file that is not in the DocFX content or resource set is an invalid file link. A `topicUid` or `xref` that does not match a `uid` is an invalid cross-reference.

## Local package feed

Pack writes `artifacts/packages`. Root `nuget.config` does not list that folder, so a fresh restore does not need it to exist. Sample 00 auto is the packaged consumer: `verify-samples` packs, then restores that sample from the folder. The other samples project-reference source, so they do not depend on feed order or a cold package cache. Restores in this repo use the isolated folder `artifacts/nuget-packages`, so a local pack is not stored under `~/.nuget/packages` at the release version. `dev pack` fails when that shared-cache entry's `.nupkg.metadata` `source` is a local path. `scripts/overview.md` records the folder layout and the guard.
