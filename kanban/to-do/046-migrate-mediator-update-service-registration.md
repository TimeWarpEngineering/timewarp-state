# Task 046: Migrate Mediator - Update Service Registration

## Description

- Rewrite the `EnsureMediator` method to use martinothamar/Mediator's configuration API
- Update pipeline behavior registration to use `MediatorOptions.PipelineBehaviors`

## Requirements

- Replace `RegisterServicesFromAssemblies()` with `options.Assemblies`
- Replace `AddOpenRequestPostProcessor()` with `options.PipelineBehaviors`
- Remove `RequestPreProcessorBehavior` registration (handled automatically)
- Configure appropriate `ServiceLifetime`

## Checklist

### Implementation
- [ ] Update `source/timewarp-state/extensions/service-collection-extensions.add-timewarp-state.cs`:
  - [ ] Rewrite `EnsureMediator` method with new API
  - [ ] Configure `options.Assemblies`
  - [ ] Configure `options.PipelineBehaviors` with ordered list
  - [ ] Set `options.ServiceLifetime` (recommend Scoped for Blazor)
  - [ ] Remove `RequestPreProcessorBehavior` TryAddEnumerable call
- [ ] Update `source/timewarp-state-plus/extensions/service-collection-extensions.cs`:
  - [ ] Update any handler registrations if needed
- [ ] Update `tests/test-app/test-app-client/program.cs`:
  - [ ] Update pipeline behavior registrations to work with new API

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

