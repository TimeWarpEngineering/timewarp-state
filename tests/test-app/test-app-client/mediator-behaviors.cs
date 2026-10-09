#region Purpose
// Puts every handler in this assembly on ClientPipeline and declares the test app's extra pipeline behaviors.
#endregion

#region Design
// Behaviors are fixed at compile time by assembly attributes: pre notification 500, post notification 510, persistence
// 520, active action tracking 530, event stream 540. Lower order is outermost, so they run inside the library's own
// behaviors (100-400).
#endregion

// Every action/handler in this app is a ClientPipeline member; behaviors are woven only into
// that pipeline.
[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PrePipelineNotificationRequestPreProcessor<,>), order: 500, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PostPipelineNotificationRequestPostProcessor<,>), order: 510, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor<,>), order: 520, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ActiveActionBehavior<,>), order: 530, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(EventStreamBehavior<,>), order: 540, Scope = typeof(ClientPipeline))]
