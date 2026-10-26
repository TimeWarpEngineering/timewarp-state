# Scripts

Runfiles under `scripts/` build, test, pack, and clean this repo. `dev test` and `dev e2e` delegate to `scripts/test.cs` and `scripts/e2e.cs`. CI uses `dev workflow`.

## Local NuGet cache

Root `nuget.config` does not list `artifacts/packages`. A missing folder does not fail restore. `dev pack` writes that folder and opens each `.nupkg` to check its layout. Sample 00 auto is the only `PackageReference` consumer of those packages. `verify-samples` builds the other samples as project references, packs, deletes each packable id's version folder under `artifacts/nuget-packages`, then restores sample 00 auto with `--force` and a config file that maps `TimeWarp.State` and `TimeWarp.State.*` to that folder. A same-version extract would otherwise win over the nupkg just packed. `RestorePackagesPath` names the isolated folder and wins over `NUGET_PACKAGES`. The other samples do not use the feed.

Restores in this repo use `artifacts/nuget-packages` and do not write the packed version into `~/.nuget/packages`:

- `nuget.config` sets `globalPackagesFolder` to `artifacts/nuget-packages` (relative to the config file).
- `Directory.Build.props` sets `RestorePackagesPath` to the same folder. That property wins over `NUGET_PACKAGES` for project restore, build, pack, and test.
- `scripts/package.cs`, `build.cs`, `test.cs`, `e2e.cs`, and `clean.cs`, and the dev CLI, set `NUGET_PACKAGES` to that folder before they start child `dotnet` processes.

A local pack stays in `artifacts/packages`. A restore extracts it under `artifacts/nuget-packages`. The release version string can match a package published to nuget.org. Other repositories on the machine still resolve that version from their own cache and from nuget.org.

`dev pack` and `scripts/package.cs` refuse to pack when a shared-cache `.nupkg.metadata` for a package id and version being packed has a `source` that is not an `http://` or `https://` URL. The message names the version folder. Delete that folder. The shared cache is `~/.nuget/packages`, and an inherited `NUGET_PACKAGES` when that path is not `artifacts/nuget-packages`. The guard does not treat `artifacts/nuget-packages` as poison: a local-feed extract there is the isolated lane.

`clean` sets `NUGET_PACKAGES` and then runs `dotnet nuget locals all --clear`, so the global-packages folder it clears is `artifacts/nuget-packages`. It does not delete `~/.nuget/packages`.

## How to debug analyzer

https://peterlesliemorris.com/debugging-my-published-rolsyn-source-generator-nuget-package/
