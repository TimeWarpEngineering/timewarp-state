# Task 046: Migrate Mediator - Update Service Registration

## Description

- Rewrite the `EnsureMediator` method to use martinothamar/Mediator's configuration API
- Update pipeline behavior registration to use `MediatorOptions.PipelineBehaviors`

## Requirements

- Replace `RegisterServicesFromAssemblies()` with `options.Assemblies`
- Replace `AddOpenRequestPostProcessor()` with `options.PipelineBehaviors`
- Remove `RequestPreProcessorBehavior` registration (handled automatically)
- Configure appropriate `ServiceLifetime`

## ⚠️ Architecture decision + corrected approach

The task doc's "New pattern" (`options.Assemblies = timeWarpStateOptions.Assemblies.ToArray()`, `options.PipelineBehaviors = [...]` inside the library's `EnsureMediator`) is INVALID. martinothamar/Mediator's source generator parses `AddMediator(options => …)` at COMPILE TIME and requires compile-time-constant values — exactly what `MSG0007` rejected. A library cannot pass runtime assemblies, and a library-internal `AddMediator` can never see consumer handlers.

**Decision (maintainer, 2026-06-16): consumer calls `AddMediator`.** Verified working via a 2-project probe: a consumer's `AddMediator(o => o.Assemblies = [typeof(LibMarker)])` registers handlers from referenced assemblies, and pipeline behaviors registered as runtime open-generic DI (`AddScoped(typeof(IPipelineBehavior<,>), typeof(X<,>))`) ARE resolved by Mediator at runtime (no need for the compile-time `PipelineBehaviors` option). So only HANDLER registration moves to the consumer; the library keeps registering its own behaviors via DI.

Also discovered & fixed (constraint fallout): `IPipelineBehavior`/`MessagePre`/`MessagePostProcessor` constrain `TMessage : IMessage` (not `notnull`); changed `where T : notnull` → `where T : IMessage` on MyBehavior, MultiTimerPostProcessor, PostPipelineNotificationRequestPostProcessor.

### Implementation (DONE — core project compiles cleanly)
- [x] `service-collection-extensions.add-timewarp-state.cs`: removed `EnsureMediator` entirely; register the 3 library behaviors as open-generic `IPipelineBehavior<,>` in pipeline order — StateInitializationPreProcessor (pre) → StateTransactionBehavior → RenderSubscriptionsPostProcessor (post). Removed `RequestPreProcessorBehavior` (gone in Mediator).
- [x] `service-collection-extensions.log-timewarp-state-middleware.cs`: pre/post processors are now `IPipelineBehavior`, so it enumerates a single ordered `IPipelineBehavior<,>` list (removed the now-nonexistent `IRequestPreProcessor`/`IRequestPostProcessor` lookups).
- [x] `timewarp-state-plus/extensions/service-collection-extensions.cs`: removed manual `AddTransient<IRequestHandler<…>,Handler>` registrations (handlers now registered by the consumer's `AddMediator` via the Plus marker); kept `AddScoped<RouteState>()`.
- [x] `tests/test-app/test-app-client/program.cs`: added `AddMediator(o => { o.ServiceLifetime = Scoped; o.Assemblies = [typeof(Test.App.Client.AssemblyMarker), typeof(TimeWarp.State.AssemblyMarker), typeof(TimeWarp.State.Plus.AssemblyMarker)]; })`; converted the 3 pre/post-processor registrations to `IPipelineBehavior<,>`.

### Verification
- [x] `source/timewarp-state` builds with ZERO errors (was the goal of this task; MSG0007 gone)
- [ ] `-plus` / test-app / samples build — BLOCKED on task 047, see below

## ⛔ Handoff to task 047 (source generators) — new blocker discovered

Building `-plus`/test-app now fails with `CS0122` in the generated `Mediator.g.cs`: `'StartRequest'`/`'CommitRequest'` (and the 11 `internal ActionSet.Handler` classes) are inaccessible. Two coupled issues, both task-047:
1. **Generator flows transitively**: `timewarp-state.csproj:37` references `Mediator.SourceGenerator` with no `PrivateAssets`, so the generator runs in `-plus` and every consumer (it should only run in the consumer that calls `AddMediator`).
2. **Internal handlers/requests are cross-assembly-inaccessible**: in the consumer-AddMediator model, the consumer's generated code must reference the handler/request types it registers. The 13 `internal` types (StartRequest, CommitRequest, 11 ActionSet.Handler) must become `public` (or use another strategy). This is an API-surface change → needs maintainer confirmation in 047.

## Notes

**Current pattern:**
```csharp
private static void EnsureMediator(IServiceCollection serviceCollection, TimeWarpStateOptions timeWarpStateOptions)
{
  if (serviceCollection.HasRegistrationFor(typeof(IMediator))) return;

  serviceCollection
    .AddMediator(
      mediatorServiceConfiguration =>
        mediatorServiceConfiguration
          .RegisterServicesFromAssemblies(timeWarpStateOptions.Assemblies.ToArray())
          .AddOpenRequestPostProcessor(typeof(RenderSubscriptionsPostProcessor<,>))
    );
  serviceCollection.TryAddEnumerable(
    new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>), ServiceLifetime.Transient)
  );
}
```

**New pattern required:**
```csharp
private static void EnsureMediator(IServiceCollection serviceCollection, TimeWarpStateOptions timeWarpStateOptions)
{
  if (serviceCollection.HasRegistrationFor(typeof(IMediator))) return;

  serviceCollection.AddMediator(options =>
  {
    options.Assemblies = timeWarpStateOptions.Assemblies.ToArray();
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.PipelineBehaviors = 
    [
      typeof(StateInitializationPreProcessor<,>),
      typeof(StateTransactionBehavior<,>),
      typeof(RenderSubscriptionsPostProcessor<,>)
    ];
  });
}
```

**Important:** Pipeline behaviors are executed in the order specified in the array.

**Files to modify:**
1. `source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs`
2. `source/timewarp-state-plus/extensions/service-collection-extensions.cs`
3. `tests/test-app/test-app-client/program.cs`

## Implementation Notes

