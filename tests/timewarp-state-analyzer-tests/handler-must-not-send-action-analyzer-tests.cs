#region Purpose
// Tests for HandlerMustNotSendActionAnalyzer: TWS0002 when a handler sends an action (directly or through a generated
// state method). Negatives, TWS0003, and multi-file/generated-file cases live in sibling files.
#endregion

#region Design
// Inline source compiled with AnalyzerTestFactory. The expected span is marked {|#0:...|} in the source and the
// result (handler name, called member) comes from HandlerSendSources, so each case only marks where the send is.
#endregion

// ReSharper disable InconsistentNaming
namespace HandlerMustNotSendActionAnalyzer_;

public class Should_Trigger_TWS0002
{
  public static async Task Given_SameState_GeneratedEntry()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public sealed class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          public sealed class Action : IAction { }

          internal sealed class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<CounterState>().{|#0:Fetch|}();
            }
          }
        }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }
        }

        public async Task Fetch(CancellationToken? externalCancellationToken = null)
        {
          await Sender.Send(new FetchActionSet.Action());
        }
      }
      """;

    await HandlerSendSources.RunSendAsync(TestCode, "Handler", "Fetch");
  }

  public static async Task Given_CrossState_GeneratedEntry()
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

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<ToastState>().{|#0:AddProblemDetails|}();
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunSendAsync(TestCode, "Handler", "AddProblemDetails");
  }

  public static async Task Given_SenderSend_OfIBaseAction()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      public interface IBaseAction : IAction { }

      public sealed class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          public sealed class Action : IBaseAction { }

          internal sealed class Handler : StateActionHandler<Action>
          {
            private readonly ISender Sender;

            public Handler(IStore store, ISender sender) : base(store)
            {
              Sender = sender;
            }

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Sender.{|#0:Send|}(new Action());
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunSendAsync(TestCode, "Handler", "CounterState.IncrementActionSet.Action");
  }

  public static async Task Given_HandleError_OnAppBase()
  {
    const string TestCode = HandlerSendSources.Usings +
      """
      internal abstract class BaseHandler<TAction> : StateActionHandler<TAction>
        where TAction : IAction
      {
        protected BaseHandler(IStore store) : base(store) { }

        protected virtual Task HandleError(CancellationToken cancellationToken) => Task.CompletedTask;
      }

      """ + HandlerSendSources.ToastState +
      """
      public sealed class WeatherState : State<WeatherState>
      {
        public override void Initialize() { }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }

          internal sealed class Handler : BaseHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await HandleError(cancellationToken);
            }

            protected override async Task HandleError(CancellationToken cancellationToken)
            {
              await Store.GetState<ToastState>().{|#0:AddProblemDetails|}();
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunSendAsync(TestCode, "Handler", "AddProblemDetails");
  }
}
