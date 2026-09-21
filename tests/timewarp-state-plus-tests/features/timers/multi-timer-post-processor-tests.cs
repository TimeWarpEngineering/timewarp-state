#region Purpose
// Recursion: MultiTimerPostProcessor skips IInternalAction so ResetTimersOnActivity is sent once.
#endregion

// ReSharper disable UnusedType.Global
namespace MultiTimerPostProcessor_;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TimeWarp.Features.ActionTracking;
using TimeWarp.State.Plus.Features.Timers;

public class MultiTimerPostProcessor_Should
{
  public async Task Dispatch_Reset_Once_For_User_Request_And_Skip_On_Internal()
  {
    using ProcessorHarness processorHarness = new();

    await processorHarness.UserProcessor.Handle
    (
      new UserRequest(),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    processorHarness.Sender.Sent.Count.ShouldBe(1);
    processorHarness.Sender.Sent[0].ShouldBeOfType<TimerState.ResetTimersOnActivityActionSet.Action>();

    await processorHarness.ResetProcessor.Handle
    (
      (TimerState.ResetTimersOnActivityActionSet.Action)processorHarness.Sender.Sent[0],
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    processorHarness.Sender.Sent.Count.ShouldBe(1);
  }

  public async Task Skip_Reset_For_Start_And_Complete_Processing()
  {
    using ProcessorHarness processorHarness = new();
    UserRequest userRequest = new();

    await processorHarness.StartProcessor.Handle
    (
      new ActionTrackingState.StartProcessingActionSet.Action(userRequest),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    await processorHarness.CompleteProcessor.Handle
    (
      new ActionTrackingState.CompleteProcessingActionSet.Action(userRequest),
      _ => Task.FromResult(new object()),
      CancellationToken.None
    );

    processorHarness.Sender.Sent.ShouldBeEmpty();
  }

  public async Task Terminate_When_Nested_Dispatch_Runs_The_Same_Processor()
  {
    using ProcessorHarness processorHarness = new();
    processorHarness.Sender.OnSend = async (request, cancellationToken) =>
    {
      if (request is TimerState.ResetTimersOnActivityActionSet.Action resetAction)
      {
        await processorHarness.ResetProcessor.Handle
        (
          resetAction,
          _ => Task.FromResult(new object()),
          cancellationToken
        );
      }
    };

    using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(2));
    await processorHarness.UserProcessor.Handle
    (
      new UserRequest(),
      _ => Task.FromResult(new object()),
      timeout.Token
    );

    processorHarness.Sender.Sent.Count.ShouldBe(1);
    processorHarness.Sender.Sent[0].ShouldBeOfType<TimerState.ResetTimersOnActivityActionSet.Action>();
  }

  private sealed class UserRequest : IAction;

  private sealed class ProcessorHarness : IDisposable
  {
    public RecordingSender Sender { get; }
    public TimerState TimerState { get; }
    public MultiTimerPostProcessor<UserRequest, object> UserProcessor { get; }
    public MultiTimerPostProcessor<TimerState.ResetTimersOnActivityActionSet.Action, object> ResetProcessor { get; }
    public MultiTimerPostProcessor<ActionTrackingState.StartProcessingActionSet.Action, object> StartProcessor { get; }
    public MultiTimerPostProcessor<ActionTrackingState.CompleteProcessingActionSet.Action, object> CompleteProcessor { get; }

    public ProcessorHarness()
    {
      Sender = new();
      TimerState = new
      (
        Options.Create(new MultiTimerOptions()),
        NullLogger<TimerState>.Instance,
        A.Fake<IPublisher<ClientPipeline>>()
      )
      {
        Sender = Sender
      };

      UserProcessor = new(NullLogger<MultiTimerPostProcessor<UserRequest, object>>.Instance, TimerState);
      ResetProcessor = new
      (
        NullLogger<MultiTimerPostProcessor<TimerState.ResetTimersOnActivityActionSet.Action, object>>.Instance,
        TimerState
      );
      StartProcessor = new
      (
        NullLogger<MultiTimerPostProcessor<ActionTrackingState.StartProcessingActionSet.Action, object>>.Instance,
        TimerState
      );
      CompleteProcessor = new
      (
        NullLogger<MultiTimerPostProcessor<ActionTrackingState.CompleteProcessingActionSet.Action, object>>.Instance,
        TimerState
      );
    }

    public void Dispose() => TimerState.Dispose();
  }

  private sealed class RecordingSender : ISender<ClientPipeline>
  {
    public List<object> Sent { get; } = [];
    public Func<object, CancellationToken, Task>? OnSend { get; set; }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
      return RecordAndReturn<TResponse>(request, cancellationToken);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
      return Record(request, cancellationToken);
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
      await Record(request, cancellationToken);
      return null;
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

    private async Task Record(object request, CancellationToken cancellationToken)
    {
      Sent.Add(request);
      if (OnSend is not null)
      {
        await OnSend(request, cancellationToken);
      }
    }

    private async Task<TResponse> RecordAndReturn<TResponse>(object request, CancellationToken cancellationToken)
    {
      await Record(request, cancellationToken);
      return default!;
    }
  }
}
