# TimeWarp.State.DevTools.App

Published TimeWarp DevTools inspector, packed the way Aspire packs its dashboard.

The package is framework-dependent and has no runtime identifier. `tools/TimeWarp.State.DevTools.dll` is the app. `TimeWarp.State.DevTools.Hosting` depends on this package and starts that dll with the AppHost's .NET host (`DOTNET_HOST_PATH`, the running `dotnet` muxer, `DOTNET_ROOT/dotnet`, then `dotnet`). Reference Hosting from the AppHost; do not reference this package from the inspected app.

`build/TimeWarp.State.DevTools.App.targets` and `buildTransitive/TimeWarp.State.DevTools.App.targets` set `TimeWarpStateDevToolsAppPath` to `tools/TimeWarp.State.DevTools.dll` and, on an Aspire AppHost, stamp that path as assembly metadata `timewarpstatedevtoolspath`.

The inspector project in this repository stays unpackable. This package is the publish output, produced in the same release as Hosting and at the same version.
