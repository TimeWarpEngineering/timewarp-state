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

- [ ] `samples/00-state-action-handler/server/sample-00-server` (Blazor Server; markers: app + TimeWarp.State)
- [ ] `samples/00-state-action-handler/wasm/sample-00-wasm` (WASM; markers: app + TimeWarp.State)
- [ ] `samples/00-state-action-handler/auto/sample-00-auto/sample-00-auto-client` (InteractiveAuto client; markers: app + TimeWarp.State)
- [ ] `samples/01-redux-dev-tools/wasm/sample-01-wasm` (markers: app + TimeWarp.State)
- [ ] `samples/02-action-tracking/wasm/sample-02-wasm` (uses action-tracking → markers include TimeWarp.State.Plus)
- [ ] `samples/03-routing/wasm/sample-03-wasm` (uses routing → markers include TimeWarp.State.Plus)
- [ ] Confirm each sample's `AssemblyMarker` type/namespace for the `typeof(...)` marker (create one if missing)
- [ ] **Verify: full-solution `dotnet build` is green** (samples were the last red projects)
- [ ] Move sample bullets out of task 049 (done when this task was created)

## Notes

Discovered during task 049. No sample uses `[PersistentState]`, so the persistence runtime gap (task 075) does not affect them. Server-only samples could use `ServiceLifetime.Singleton`; Scoped is safe everywhere and matches Blazor circuit semantics.
