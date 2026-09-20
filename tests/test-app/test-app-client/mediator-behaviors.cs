// Every action/handler in this app is a ClientPipeline member; behaviors are woven only into
// that pipeline.
[assembly: MediatorScope(typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PrePipelineNotificationRequestPreProcessor<,>), order: 500, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PostPipelineNotificationRequestPostProcessor<,>), order: 510, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor<,>), order: 520, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(ActiveActionBehavior<,>), order: 530, Scope = typeof(ClientPipeline))]
[assembly: MediatorBehavior(typeof(EventStreamBehavior<,>), order: 540, Scope = typeof(ClientPipeline))]
