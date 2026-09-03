#region Purpose
// Assigns this sample's actions and handlers to ClientPipeline.
#endregion

#region Design
// Hosts that call AddGeneratedMediator<ClientPipeline> need assembly MediatorScope so
// legitimate client Sends are not TWM004. Empty sealed ClientPipeline is the scope marker.
#endregion

using TimeWarp.Mediator;
using TimeWarp.State;

[assembly: MediatorScope(typeof(ClientPipeline))]
