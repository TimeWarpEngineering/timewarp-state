#region Purpose
// ActiveActionBehavior skips IInternalAction instead of throwing on a hard-coded type list.
#endregion

#region Design
// Constructs ActiveActionBehavior directly with a RecordingSender and NullLogger, and passes a next delegate that
// counts calls; no DI or store. Private [TrackAction] actions cover user and internal cases.
#endregion

// ReSharper disable UnusedType.Global
namespace ActiveActionBehavior_;

using Microsoft.Extensions.Logging.Abstractions;
using TimeWarp.Features.ActionTracking;
using TimeWarp.State.Plus.Features.Timers;

public class ActiveActionBehavior_Should
{
  public async Task Skip_Tracking_When_Action_Is_Internal()
  {
    RecordingSender recordingSender = new();
    ActiveActionBehavior<TimerState.ResetTimersOnActivityActionSet.Action, object> behavior = new
    (
      recordingSender,
      NullLogger<ActiveActionBehavior<TimerState.ResetTimersOnActivityActionSet.Action, object>>.Instance
    );

    int nextCalls = 0;
    object result = await behavior.Handle
    (
      new TimerState.ResetTimersOnActivityActionSet.Action(),
      (_, _) =>
      {
        nextCalls++;
        return new ValueTask<object>(new object());
      },
      CancellationToken.None
    );

    nextCalls.ShouldBe(1);
    result.ShouldNotBeNull();
    recordingSender.Sent.ShouldBeEmpty();
  }

  public async Task Skip_Tracking_When_Tracked_Action_Is_Also_Internal()
  {
    RecordingSender recordingSender = new();
    ActiveActionBehavior<TrackedInternalAction, object> behavior = new
    (
      recordingSender,
      NullLogger<ActiveActionBehavior<TrackedInternalAction, object>>.Instance
    );

    int nextCalls = 0;
    await behavior.Handle
    (
      new TrackedInternalAction(),
      (_, _) =>
      {
        nextCalls++;
        return new ValueTask<object>(new object());
      },
      CancellationToken.None
    );

    nextCalls.ShouldBe(1);
    recordingSender.Sent.ShouldBeEmpty();
  }

  public async Task Track_User_Action_With_Start_Then_Complete()
  {
    RecordingSender recordingSender = new();
    ActiveActionBehavior<TrackedUserAction, object> behavior = new
    (
      recordingSender,
      NullLogger<ActiveActionBehavior<TrackedUserAction, object>>.Instance
    );

    int nextCalls = 0;
    await behavior.Handle
    (
      new TrackedUserAction(),
      (_, _) =>
      {
        nextCalls++;
        recordingSender.Sent.Count.ShouldBe(1);
        recordingSender.Sent[0].ShouldBeOfType<ActionTrackingState.StartProcessingActionSet.Action>();
        return new ValueTask<object>(new object());
      },
      CancellationToken.None
    );

    nextCalls.ShouldBe(1);
    recordingSender.Sent.Count.ShouldBe(2);
    recordingSender.Sent[1].ShouldBeOfType<ActionTrackingState.CompleteProcessingActionSet.Action>();
  }

  public async Task Complete_Tracking_With_None_When_The_Handler_Is_Cancelled()
  {
    RecordingSender recordingSender = new();
    ActiveActionBehavior<TrackedUserAction, object> behavior = new
    (
      recordingSender,
      NullLogger<ActiveActionBehavior<TrackedUserAction, object>>.Instance
    );

    using CancellationTokenSource cancellationTokenSource = new();
    await Should.ThrowAsync<OperationCanceledException>
    (
      async () => await behavior.Handle
      (
        new TrackedUserAction(),
        (_, _) =>
        {
          cancellationTokenSource.Cancel();
          throw new OperationCanceledException(cancellationTokenSource.Token);
        },
        cancellationTokenSource.Token
      )
    );

    recordingSender.Sent.Count.ShouldBe(2);
    recordingSender.Sent[1].ShouldBeOfType<ActionTrackingState.CompleteProcessingActionSet.Action>();
    recordingSender.Tokens[1].ShouldBe(CancellationToken.None);
    recordingSender.Tokens[0].CanBeCanceled.ShouldBeTrue();
  }

  [TrackAction]
  private sealed class TrackedUserAction : IAction;

  [TrackAction]
  private sealed class TrackedInternalAction : IInternalAction;

  private sealed class RecordingSender : ISender<ClientPipeline>
  {
    public List<object> Sent { get; } = [];
    public List<CancellationToken> Tokens { get; } = [];

    public ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
      Sent.Add(request);
      Tokens.Add(cancellationToken);
      return ValueTask.FromResult(default(TResponse)!);
    }

    public ValueTask Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
      Sent.Add(request!);
      Tokens.Add(cancellationToken);
      return ValueTask.CompletedTask;
    }

    public ValueTask<object?> Send(object request, CancellationToken cancellationToken = default)
    {
      Sent.Add(request);
      Tokens.Add(cancellationToken);
      return ValueTask.FromResult<object?>(null);
    }


  }
}
