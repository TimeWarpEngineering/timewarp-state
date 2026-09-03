using TimeWarp.Mediator;
using TimeWarp.State.Plus;
using TimeWarp.Features.ActionTracking;
using Test.App.Client.Features.EventStream;
using Test.App.Client.Pipeline.NotificationPreProcessor;
using Test.App.Client.Pipeline.NotificationPostProcessor;

[assembly: MediatorBehavior(typeof(PrePipelineNotificationRequestPreProcessor<,>), order: 500)]
[assembly: MediatorBehavior(typeof(PostPipelineNotificationRequestPostProcessor<,>), order: 510)]
[assembly: MediatorBehavior(typeof(PersistentStatePostProcessor<,>), order: 520)]
[assembly: MediatorBehavior(typeof(ActiveActionBehavior<,>), order: 530)]
[assembly: MediatorBehavior(typeof(EventStreamBehavior<,>), order: 540)]
