# Migrate samples to martinothamar/Mediator

## Description

The 6 sample consumer apps don't build after the Mediator migration. Each needs the same consumer-side wiring test-app already got (tasks 046/049): the consumer owns `AddMediator(...)` because Mediator's source generator registers handlers at compile time.

Split out of task 049 so the samples have their own task → own commit.

The per-handler `ValueTask<Unit>` return fix is already applied in the migration commit (the sync handlers in these samples used `return Unit.Value;` in a non-async method → already corrected to `new ValueTask<Unit>(Unit.Value)`).

## Per-sample changes (same pattern as test-app)

For each app's `.csproj`:
```xml
<PackageReference Include="Mediator.SourceGenerator" PrivateAssets="all"
    IncludeAssets="runtime; build; native; contentfiles; analyzers" />
```

For each app's `program.cs` (before/around `AddTimeWarpState`):
```csharp
serviceCollection.AddMediator(options =>
{
  options.ServiceLifetime = ServiceLifetime.Scoped;     // Singleton for server-only samples is fine too
  options.GenerateTypesAsInternal = true;               // sample actions stay internal -> no CS0051
  options.Assemblies = [ /* sample marker, TimeWarp.State marker, + Plus marker if used */ ];
});
```

The `Assemblies` list must include compile-time `typeof(...)` markers for every assembly with handlers the sample uses: the sample's own assembly + `TimeWarp.State` always + `TimeWarp.State.Plus` if it uses routing/timers/theme/action-tracking.

## Checklist

- [x] `samples/00-state-action-handler/server/sample-00-server` — builds 0 errors
- [x] `samples/00-state-action-handler/wasm/sample-00-wasm` — builds 0 errors
- [x] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto-client` — builds 0 errors
- [x] `samples/01-redux-dev-tools/wasm/sample-01-wasm` — builds 0 errors
- [x] `samples/02-action-tracking/wasm/sample-02-wasm` (markers include TimeWarp.State.Plus) — builds 0 errors
- [x] `samples/03-routing/wasm/sample-03-wasm` (markers include TimeWarp.State.Plus) — builds 0 errors
- [x] Marker type: used `typeof(Program)` (in each sample's assembly) rather than creating an AssemblyMarker
- [x] **Full-solution `dotnet build timewarp-state.slnx` is GREEN — 0 errors**
- [x] Sample bullets removed from task 049

## What each sample needed (3 edits per app)

1. `.csproj`: add `Mediator.SourceGenerator` (PrivateAssets=all, analyzer assets) — each consumer owns the generator.
2. `program.cs`: `AddMediator(o => { o.ServiceLifetime = Scoped; o.GenerateTypesAsInternal = true; o.Assemblies = [typeof(Program), TimeWarp.State marker, (+Plus marker)] })`. `GenerateTypesAsInternal` keeps each sample's internal action types from tripping CS0051.
3. `global-usings.cs`: add `global using Mediator;` (handlers now reference `Mediator.Unit`).

## Notes

- Samples reference `TimeWarp.State`/`Plus` as **NuGet packages** from the local feed (`artifacts/packages`), version `12.0.0-beta.3`. The cached copy in `~/.nuget/packages` was a stale intermediate (still listed a `Mediator.SourceGenerator` dependency and predated making handlers public), which caused the earlier CS0122/CS0051 errors. Resolution: repack the libraries (`dotnet build` the 3 source projects → GeneratePackageOnBuild) and clear the cached `12.0.0-beta.3` extraction so restore pulls the fresh package. A clean checkout doesn't hit this; it was a local-cache artifact of building intermediate states during the migration.
- **Heads-up for the repo:** same-version local-feed repacks are fragile (the cache shadows them). Before release (task 051) consider bumping the beta version so consumers/samples reliably pick up changes, or switch samples to project references during active development (sample-03 already has commented-out project references).
- No sample uses `[PersistentState]`, so the persistence runtime gap (task 075) does not affect them.
