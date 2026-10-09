#region Purpose
// Declares TimeWarp.State.Blazor's mediator membership and the Blazor-only ClientPipeline behaviors.
#endregion

#region Design
// ReduxDevTools (order 100) and RenderSubscriptions (order 400) are woven from this assembly so
// TimeWarp.State has no Blazor reference. StateInitialization (200) and StateTransaction (300)
// stay in TimeWarp.State. Order is compared across assemblies. AssemblyMarker locates this assembly.
#endregion

// Referencing this package joins these behaviors to the host's generated mediator.
// A console host that references only TimeWarp.State does not weave them.
[assembly: MediatorAssembly]
[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ReduxDevToolsBehavior<,>), order: 100, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(RenderSubscriptionsPostProcessor<,>), order: 400, Scope = typeof(ClientPipeline))]

namespace TimeWarp.State.Blazor;

/// <summary>
/// Marker type for the TimeWarp.State.Blazor assembly.
/// </summary>
public sealed class AssemblyMarker;
