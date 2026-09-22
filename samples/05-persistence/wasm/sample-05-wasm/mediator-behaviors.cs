#region Purpose
// Opts this host into ClientPipeline and the persistence save behavior.
#endregion

#region Design
// PersistentStatePostProcessor is not woven by TimeWarp.State.Plus. The host declares it.
// Order 520 matches the test host: core render subscriptions are 400, so this save runs on the
// way out after the handler and before subscribers re-render.
// MediatorScope is required so sample Sends are ClientPipeline members (TWM004).
#endregion

[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor<,>), order: 520, Scope = typeof(ClientPipeline))]
