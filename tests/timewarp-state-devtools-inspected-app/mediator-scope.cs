#region Purpose
// Assigns this fixture's actions and handlers to ClientPipeline.
#endregion

#region Design
// Hosts that call AddGeneratedMediator of ClientPipeline need assembly MediatorScope so legitimate client Sends are not TWM004. The scope marker type lives in TimeWarp.State.
#endregion

[assembly: MediatorScope(typeof(ClientPipeline))]
