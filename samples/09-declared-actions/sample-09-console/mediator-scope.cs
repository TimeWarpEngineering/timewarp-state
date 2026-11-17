#region Purpose
// Assigns this sample's actions and handlers to ClientPipeline.
#endregion

#region Design
// Hosts that call AddGeneratedMediator<ClientPipeline> need assembly MediatorScope so legitimate client Sends are not
// TWM004. A declared request takes its scope from the nearest existing containing type, then this attribute.
#endregion

[assembly: MediatorScope(typeof(ClientPipeline))]
