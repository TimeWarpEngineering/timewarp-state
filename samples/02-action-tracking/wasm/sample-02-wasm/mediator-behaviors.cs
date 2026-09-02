using TimeWarp.Mediator;
using TimeWarp.Features.ActionTracking;

[assembly: MediatorBehavior(typeof(ActiveActionBehavior<,>), order: 500)]
