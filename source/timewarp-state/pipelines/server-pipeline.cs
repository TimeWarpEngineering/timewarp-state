#region Purpose
// Marker type that names the server request pipeline, separate from the Blazor store.
#endregion

#region Design
// Empty sealed marker for ISender/IPublisher/AddGeneratedMediator and MediatorScope. Server API
// handlers and requests use this scope so they never pass through ClientPipeline store behaviors.
// Cross-scope Send of a ClientPipeline action is the TWM004 compile error.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Marker type that names the server pipeline: request handlers that run on the server (API endpoints,
/// background processing) and must not pass through the TimeWarp.State store behaviors.
/// </summary>
/// <remarks>
/// Assign server handlers, requests or a whole assembly with <c>[MediatorScope(typeof(ServerPipeline))]</c>,
/// register the pipeline with the generated <c>AddGeneratedMediator&lt;ServerPipeline&gt;()</c> and inject
/// <c>ISender&lt;ServerPipeline&gt;</c> / <c>IPublisher&lt;ServerPipeline&gt;</c>. The Blazor store pipeline
/// is <see cref="ClientPipeline"/>.
/// </remarks>
public sealed class ServerPipeline;
