#region Purpose
// Tests for the AllowActionSend opt-out: TWS0002 suppression and the TWS0003 info diagnostic.
#endregion

#region Design
// Observed analyzer behavior, pinned here: TWS0003 reports on every AllowActionSend attribute applied to a handler
// type or a method of a handler, whether or not the handler actually sends (it is a visibility diagnostic, not an
// "unnecessary suppression" check). A method-level attribute suppresses TWS0002 only for sends inside that method.
// The TWS0003 span is the attribute syntax, marked {|#0:...|}.
#endregion

// ReSharper disable InconsistentNaming
namespace HandlerMustNotSendActionAnalyzer_;

public class Should_Trigger_TWS0003
{
  public static async Task Given_AllowActionSend_OnHandlerType()
  {
    const string TestCode = HandlerSendSources.Usings + HandlerSendSources.ToastState +
      """
      public sealed class WeatherState : State<WeatherState>
      {
        public override void Initialize() { }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }

          [{|#0:AllowActionSend("grandfather HandleError toast")|}]
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

    await HandlerSendSources.RunAsync(
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(TestCode),
      HandlerSendSources.AllowResult("Handler", "grandfather HandleError toast"));
  }

  public static async Task Given_AllowActionSend_OnHandlerMethod_SuppressesTWS0002()
  {
    const string TestCode = HandlerSendSources.Usings + HandlerSendSources.ToastState +
      """
      public sealed class WeatherState : State<WeatherState>
      {
        public override void Initialize() { }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }

          internal sealed class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            [{|#0:AllowActionSend("toast on failure")|}]
            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<ToastState>().AddProblemDetails();
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunAsync(
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(TestCode),
      HandlerSendSources.AllowResult("Handler", "toast on failure"));
  }

  public static async Task Given_AllowActionSend_OnHandlerType_WithoutAnySend()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public sealed class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          public sealed class Action : IAction { }

          [{|#0:AllowActionSend("stale exemption")|}]
          internal sealed class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            public override ValueTask Handle(Action action, CancellationToken cancellationToken) => default;
          }
        }
      }
      """;

    await HandlerSendSources.RunAsync(
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(TestCode),
      HandlerSendSources.AllowResult("Handler", "stale exemption"));
  }

  public static async Task Given_AllowActionSend_OnMethod_DoesNotCover_OtherMethods()
  {
    const string TestCode = HandlerSendSources.Usings + HandlerSendSources.ToastState +
      """
      public sealed class WeatherState : State<WeatherState>
      {
        public override void Initialize() { }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }

          internal sealed class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            [{|#0:AllowActionSend("toast on failure")|}]
            private async Task Allowed() => await Store.GetState<ToastState>().AddProblemDetails();

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<ToastState>().{|#1:AddProblemDetails|}();
            }
          }
        }
      }
      """;

    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(TestCode);

    await HandlerSendSources.RunAsync(
      analyzerTest,
      HandlerSendSources.AllowResult("Handler", "toast on failure"),
      new DiagnosticResult("TWS0002", DiagnosticSeverity.Warning).WithLocation(1).WithArguments("Handler", "AddProblemDetails"));
  }
}
