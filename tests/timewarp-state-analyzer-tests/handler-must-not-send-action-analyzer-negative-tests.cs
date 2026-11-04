#region Purpose
// Cases that must not report TWS0002, and the TWS0003 opt-out.
#endregion

#region Design
// Same AnalyzerTestFactory harness as the TWS0002 cases. Split out so each file stays under the line cap.
#endregion

// ReSharper disable InconsistentNaming
namespace HandlerMustNotSendActionAnalyzer_;

using TimeWarp.State.Analyzer.Tests;

public class Should_Not_Trigger_TWS0002
{
  public static async Task Given_Publish_OfINotification()
  {
    const string TestCode =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

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

    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(TestCode);

    await analyzerTest.RunAsync();
  }

  public static async Task Given_OwnStateMutation()
  {
    const string TestCode =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

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

    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(TestCode);

    await analyzerTest.RunAsync();
  }
}

public class Should_Trigger_TWS0003
{
  public static async Task Given_AllowActionSend_OnHandlerType()
  {
    const string TestCode =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed class ToastState : State<ToastState>
      {
        public override void Initialize() { }

        public static class AddProblemDetailsActionSet
        {
          public sealed class Action : IAction { }
        }

        public async Task AddProblemDetails(CancellationToken? externalCancellationToken = null)
        {
          await Sender.Send(new AddProblemDetailsActionSet.Action());
        }
      }

      public sealed class WeatherState : State<WeatherState>
      {
        public override void Initialize() { }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }

          [AllowActionSend("grandfather HandleError toast")]
          internal sealed class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<ToastState>().AddProblemDetails();
            }
          }
        }
      }
      """;

    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(TestCode);

    analyzerTest.ExpectedDiagnostics.Add(
      new DiagnosticResult("TWS0003", DiagnosticSeverity.Info)
        .WithSpan(29, 6, 29, 54)
        .WithArguments("Handler", "grandfather HandleError toast"));

    await analyzerTest.RunAsync();
  }
}
