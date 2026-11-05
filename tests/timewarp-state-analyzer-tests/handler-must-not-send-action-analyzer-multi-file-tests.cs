#region Purpose
// HandlerMustNotSendActionAnalyzer behavior across several source files: partial classes and generated (.g.cs) files.
#endregion

#region Design
// .g.cs decision: the analyzer calls ConfigureGeneratedCodeAnalysis(None), so any code in a file named *.g.cs is
// skipped entirely. A user handler calling a generated entry method (defined in .g.cs) IS reported, because the
// invocation sits in user code. A handler whose own body lives in a .g.cs file is NOT reported (and a send inside a
// generated wrapper is not a handler body). These tests pin both halves so the special-casing is deliberate.
// Sources go through the multi-source AnalyzerTestFactory.Create overload; spans are {|#0:...|} markup.
#endregion

// ReSharper disable InconsistentNaming
namespace HandlerMustNotSendActionAnalyzer_;

public class Should_Analyze_Across_Files
{
  public static async Task Given_PartialClass_SplitAcrossFiles()
  {
    const string StateSource = HandlerSendSources.Usings +
      """
      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }
        }

        public async Task Fetch(CancellationToken? externalCancellationToken = null)
        {
          await Sender.Send(new FetchActionSet.Action());
        }

        public static partial class IncrementActionSet
        {
          public sealed class Action : IAction { }

          internal sealed partial class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }
          }
        }
      }
      """;

    const string HandlerSource = HandlerSendSources.Usings +
      """
      public sealed partial class CounterState
      {
        public static partial class IncrementActionSet
        {
          internal sealed partial class Handler
          {
            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<CounterState>().{|#0:Fetch|}();
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunAsync(
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(("State.cs", StateSource), ("Handler.cs", HandlerSource)),
      HandlerSendSources.SendResult("Handler", "Fetch"));
  }
}

public class Should_Respect_GeneratedFiles
{
  private const string GeneratedFetchMethod =
    """
    #nullable enable
    using System.Threading;
    using System.Threading.Tasks;
    using TimeWarp.Mediator;

    public partial class CounterState
    {
      public async Task Fetch(CancellationToken? externalCancellationToken = null)
      {
        await Sender.Send(new FetchActionSet.Action());
      }
    }
    """;

  public static async Task Given_UserHandler_CallsGeneratedEntry_Reports()
  {
    const string UserSource = HandlerSendSources.Usings +
      """
      public sealed partial class CounterState : State<CounterState>
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
              await Store.GetState<CounterState>().{|#0:Fetch|}();
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunAsync(
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>
      (
        ("CounterState.cs", UserSource),
        ("CounterState.FetchActionSet_Method.g.cs", GeneratedFetchMethod)
      ),
      HandlerSendSources.SendResult("Handler", "Fetch"));
  }

  public static async Task Given_HandlerBody_InGeneratedFile_IsNotReported()
  {
    const string UserSource = HandlerSendSources.Usings + HandlerSendSources.ToastState +
      """
      public sealed partial class WeatherState : State<WeatherState>
      {
        public override void Initialize() { }

        public static partial class FetchActionSet
        {
          public sealed class Action : IAction { }

          internal sealed partial class Handler : StateActionHandler<Action>
          {
            public Handler(IStore store) : base(store) { }
          }
        }
      }
      """;

    const string GeneratedHandlerBody = HandlerSendSources.Usings +
      """
      public sealed partial class WeatherState
      {
        public static partial class FetchActionSet
        {
          internal sealed partial class Handler
          {
            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<ToastState>().AddProblemDetails();
            }
          }
        }
      }
      """;

    await HandlerSendSources.RunAsync(
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>
      (
        ("WeatherState.cs", UserSource),
        ("WeatherState.FetchActionSet_Handler.g.cs", GeneratedHandlerBody)
      ));
  }
}
