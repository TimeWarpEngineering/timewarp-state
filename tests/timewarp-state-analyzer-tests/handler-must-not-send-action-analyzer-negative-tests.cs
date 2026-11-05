#region Purpose
// Cases that must not report TWS0002.
#endregion

#region Design
// Same HandlerSendSources harness as the TWS0002 cases. Each test compiles clean and expects zero diagnostics.
#endregion

// ReSharper disable InconsistentNaming
namespace HandlerMustNotSendActionAnalyzer_;

public class Should_Not_Trigger_TWS0002
{
  public static async Task Given_Publish_OfINotification()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public sealed class Happened : INotification { }

      public sealed class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          public sealed class Action : IAction { }

          internal sealed class Handler : StateActionHandler<Action>
          {
            private readonly IPublisher Publisher;

            public Handler(IStore store, IPublisher publisher) : base(store)
            {
              Publisher = publisher;
            }

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Publisher.Publish(new Happened());
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunNoDiagnosticAsync(TestCode);
  }

  public static async Task Given_OwnStateMutation()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public sealed class CounterState : State<CounterState>
      {
        public int Count { get; private set; }

        public override void Initialize() { }

        public static class IncrementActionSet
        {
          public sealed class Action : IAction
          {
            public int Amount { get; init; }
          }

          internal sealed class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            public override ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              CounterState counterState = Store.GetState<CounterState>();
              counterState.Count += action.Amount;
              return default;
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunNoDiagnosticAsync(TestCode);
  }

  public static async Task Given_SenderSend_OfNonActionRequest()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public sealed class CountQuery : IRequest<int> { }

      public sealed class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          public sealed class Action : IAction { }

          internal sealed class Handler : StateActionHandler<Action>
          {
            private readonly ISender Sender;

            public Handler(IStore store, ISender sender) : base(store)
            {
              Sender = sender;
            }

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              int count = await Sender.Send(new CountQuery());
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunNoDiagnosticAsync(TestCode);
  }

  public static async Task Given_ActionSend_OutsideAnyHandler()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public sealed class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          public sealed class Action : IAction { }
        }
      }

      public sealed class CounterService
      {
        private readonly ISender Sender;
        private readonly IStore Store;

        public CounterService(ISender sender, IStore store)
        {
          Sender = sender;
          Store = store;
        }

        public async Task Run()
        {
          await Sender.Send(new CounterState.IncrementActionSet.Action());
        }
      }
      """;

    await HandlerSendSources.RunNoDiagnosticAsync(TestCode);
  }

  public static async Task Given_HandlerCalls_OrdinaryStateMethod()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public sealed class CounterState : State<CounterState>
      {
        public int Count { get; private set; }

        public override void Initialize() { }

        // Hand-written: no nested ResetActionSet, so it is not an ActionSet entry method.
        public void Reset() => Count = 0;

        public static class IncrementActionSet
        {
          public sealed class Action : IAction { }

          internal sealed class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            public override ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              Store.GetState<CounterState>().Reset();
              return default;
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunNoDiagnosticAsync(TestCode);
  }
}
