using TimeWarp.Features.ReduxDevTools;
using TimeWarp.Features.RenderSubscriptions;
using TimeWarp.Features.StateTransactions;
using TimeWarp.Mediator;
using TimeWarp.State;

// This assembly is a member of the TimeWarp.Mediator compile-time graph: the consuming host's
// generator links the handlers below (ReduxDevTools Commit/Start) and weaves these behaviors.
//
// Everything in this assembly belongs to the ClientPipeline (the Blazor store pipeline): the
// assembly-level MediatorScope assigns every request and handler here to ISender<ClientPipeline>,
// and each behavior is declared with Scope = typeof(ClientPipeline) so it is woven only into that
// pipeline. Server handlers (ServerPipeline) never run these behaviors, and no behavior filters on
// `request is IAction` at runtime: membership is decided at compile time.
//
// Order is the pipeline order (lower = outermost), matching the previous DI registration order:
// ReduxDevTools -> StateInitialization -> StateTransaction -> RenderSubscriptions -> handler.
// Hosts declare their own client behaviors with order >= 500 and Scope = typeof(ClientPipeline)
// so they run inside the State pipeline.
[assembly: MediatorAssembly]
[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ReduxDevToolsBehavior<,>), order: 100, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(StateInitializationPreProcessor<,>), order: 200, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(StateTransactionBehavior<,>), order: 300, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(RenderSubscriptionsPostProcessor<,>), order: 400, Scope = typeof(ClientPipeline))]

namespace TimeWarp.State;

/// <summary>
/// Serves as a marker for the assembly, facilitating easy identification and reflection-based operations.
/// </summary>
/// <remarks>
/// This class is intended to be used as a reference point within the assembly for scenarios such as assembly scanning,
/// where a stable, known type is required to locate the assembly at runtime. The class is sealed to indicate it is not
/// designed for inheritance or extension, reinforcing its role as a simple marker.
/// </remarks>
public sealed class AssemblyMarker;
