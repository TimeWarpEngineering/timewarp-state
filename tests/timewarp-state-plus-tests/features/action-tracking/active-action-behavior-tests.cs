#region Purpose
// ActiveActionBehavior skips IInternalAction instead of throwing on a hard-coded type list.
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
      _ =>
      {
        nextCalls++;
        return Task.FromResult(new object());
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
      _ =>
      {
        nextCalls++;
        return Task.FromResult(new object());
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
      _ =>
      {
        nextCalls++;
        recordingSender.Sent.Count.ShouldBe(1);
        recordingSender.Sent[0].ShouldBeOfType<ActionTrackingState.StartProcessingActionSet.Action>();
        return Task.FromResult(new object());
      },
      CancellationToken.None
    );

    nextCalls.ShouldBe(1);
    recordingSender.Sent.Count.ShouldBe(2);
    recordingSender.Sent[1].ShouldBeOfType<ActionTrackingState.CompleteProcessingActionSet.Action>();
  }

  [TrackAction]
  private sealed class TrackedUserAction : IAction;

  [TrackAction]
  private sealed class TrackedInternalAction : IInternalAction;

  private sealed class RecordingSender : ISender<ClientPipeline>
  {
    public List<object> Sent { get; } = [];

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
      Sent.Add(request);
      return Task.FromResult(default(TResponse)!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
      Sent.Add(request!);
      return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
      Sent.Add(request);
      return Task.FromResult<object?>(null);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>
    (
      IStreamRequest<TResponse> request,
      CancellationToken cancellationToken = default
    )
    {
      throw new NotSupportedException();
    }

    public IAsyncEnumerable<object?> CreateStream
    (
      object request,
      CancellationToken cancellationToken = default
    )
    {
      throw new NotSupportedException();
    }
  }
}
