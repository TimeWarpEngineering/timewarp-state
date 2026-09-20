#region Purpose
// Declares ClientPipeline membership and the ActiveActionBehavior for this sample.
#endregion

#region Design
// Hosts that call AddGeneratedMediator<ClientPipeline> need assembly MediatorScope so
// legitimate client Sends are not TWM004. ActiveActionBehavior is client-scoped only.
#endregion

[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ActiveActionBehavior<,>), order: 500, Scope = typeof(ClientPipeline))]

