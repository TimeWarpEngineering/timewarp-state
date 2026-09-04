#region Purpose
// Marker type that names the TimeWarp.State Blazor store pipeline.
#endregion

#region Design
// Empty sealed marker for ISender/IPublisher/AddGeneratedMediator and MediatorScope/MediatorBehavior.
// Every state action, its handler, and the State pipeline behaviors belong here. Blazor client code
// (WebAssembly or interactive server) dispatches through ISender<ClientPipeline>. Server handlers
// belong to ServerPipeline and never run these behaviors; a typed cross-scope Send is TWM004.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Marker type that names the TimeWarp.State store pipeline.
/// </summary>
/// <remarks>
/// <para>
/// Every state action, its handler and the State pipeline behaviors (ReduxDevTools, state initialization,
/// state transaction, render subscriptions and the opt-in Plus behaviors) belong to this pipeline.
/// Blazor client code, whether it runs in WebAssembly or in an interactive server circuit, dispatches
/// through <c>ISender&lt;ClientPipeline&gt;</c> / <c>IPublisher&lt;ClientPipeline&gt;</c>, and the host
/// registers it with the generated <c>AddGeneratedMediator&lt;ClientPipeline&gt;()</c>.
/// </para>
/// <para>
/// Server-side handlers (API endpoints, background work) belong to <see cref="ServerPipeline"/> and never
/// run the State behaviors. A typed <c>Send</c> of a client action through <c>ISender&lt;ServerPipeline&gt;</c>
/// (or the reverse) is the TimeWarp.Mediator compile error TWM004.
/// </para>
/// </remarks>
public sealed class ClientPipeline;
