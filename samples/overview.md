# TimeWarp.State Samples

This directory contains sample projects that demonstrate various features and capabilities of TimeWarp.State. Most samples are Blazor hosts. The console sample uses the core package only.

Sample 00 auto (`samples/00-state-action-handler/auto`) is the only `PackageReference` consumer. `dev verify-samples` packs, drops each packable id's same-version extract from `artifacts/nuget-packages`, then restores that host and its WebAssembly client from `artifacts/packages`, so one consumer proves the packed server, WebAssembly, analyzer, and source-generator bits. It is not in `timewarp-state.slnx`, because that version is not on nuget.org until release. The other samples `ProjectReference` the source projects, the same way `tests/test-app/test-app-client` does, so inner-loop builds skip the feed. The DevTools inspector test stays on the pack lane: `tests/timewarp-state-devtools-tests/packaged-devtools-app-tests.cs` starts the inspector from the NuGet package. `dev pack` also opens each `.nupkg` and checks its layout.

## Available Samples

### [00-StateActionHandler](00-state-action-handler/overview.md)
Demonstrates the fundamental StateActionHandler pattern, which implements unidirectional data flow in TimeWarp.State. This sample showcases:
- State management through immutable states
- Action-based state modifications
- Handler implementation for processing actions
- Examples in multiple Blazor render modes (Auto, Server, WebAssembly)

### [02-ActionTracking](02-action-tracking/wasm/overview.md)
Illustrates action tracking functionality:
- Monitoring action execution
- Performance tracking
- Action state management
- WebAssembly implementation example

### [03-Routing](03-routing/overview.md)
Demonstrates TimeWarp.State.Plus's comprehensive routing features:
- Stack-based navigation management
- Breadcrumb navigation support
- Route history tracking
- Thread-safe route updates
- Page title synchronization
- WebAssembly implementation example

### [04-Telemetry](04-telemetry/overview.md)
Observes TimeWarp.State actions in the Aspire dashboard via OpenTelemetry:
- One span per dispatched action (type names, duration, success/failure)
- .NET 11 framework ActivitySources for HTTP, Blazor, circuits, and SignalR, with the action span under `HandleEvent`
- Blazor Server `ActivitySource` path
- OTLP export when the AppHost (or `OTEL_EXPORTER_OTLP_ENDPOINT`) is set
- Snapshots remain opt-in and off in the sample

### [05-Persistence](05-persistence/overview.md)
Stores two states in the browser with `[PersistentState]`:
- Session storage for a tab-scoped draft (`DraftNoteState`)
- Local storage for accent and density (`DisplayPreferencesState`)
- Startup load through `LoadPersistentStateRequest`
- FullName write key, simple-name fallback on load (no migration framework)

### [06-Render control](06-render-control/overview.md)
Shows when `TimeWarpStateComponent` paints:
- `RendererInfo`, `IsPreRendering`, and `AssignedRenderMode` on a WebAssembly host
- `RenderReason` for event, parameter, subscription, and `ReRender`
- `CheckComplexParameterChanged` so a new object with the same text does not repaint
- `RegisterRenderTrigger` so a noisy field does not repaint a card
- `CaptureRenderCaller` on for the caller column, and off in production hosts

### [07-Console](07-console/overview.md)
Dispatches one action with `TimeWarp.State` and no Blazor package:
- `AddGeneratedMediator<ClientPipeline>` and `AddTimeWarpState`
- Prints `Count=5` and exits 0

## Getting Started

Each sample project contains its own README with detailed information about:
- Core concepts demonstrated
- Implementation details
- Key benefits
- Best practices
- Specific considerations for different Blazor render modes

Choose a sample based on the feature you want to explore and refer to its README for detailed guidance.

## Sample Structure

Each sample may include implementations in different Blazor render modes:
- **Auto**: Interactive Auto render mode
- **Server**: Interactive Server render mode
- **Wasm**: Interactive WebAssembly render mode

This helps demonstrate how TimeWarp.State can be used effectively across different Blazor hosting models.

## Best Practices

When exploring these samples, consider:
1. Reading the sample's README first to understand its purpose and concepts
2. Examining the implementation in your preferred render mode
3. Following the demonstrated patterns in your own applications
4. Using the samples as reference for implementing similar features
5. Understanding the specific considerations for each render mode
