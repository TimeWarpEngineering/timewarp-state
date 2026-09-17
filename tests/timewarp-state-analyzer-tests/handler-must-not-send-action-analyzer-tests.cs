// ReSharper disable InconsistentNaming
namespace HandlerMustNotSendActionAnalyzer_;

using TimeWarp.State.Analyzer.Tests;

public class Should_Trigger_TW0002
{
  public static async Task Given_SameState_GeneratedEntry()
  {
    const string TestCode =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

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
              await Store.GetState<CounterState>().Fetch();
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

    await RunSendDiagnosticAsync(TestCode, "Handler", "Fetch", 20, 46, 20, 51);
  }

  public static async Task Given_CrossState_GeneratedEntry()
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

    await RunSendDiagnosticAsync(TestCode, "Handler", "AddProblemDetails", 35, 44, 35, 61);
  }

  public static async Task Given_SenderSend_OfIBaseAction()
  {
    const string TestCode =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

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
              await Sender.Send(new Action());
            }
          }
        }
      }
      """;

    await RunSendDiagnosticAsync(TestCode, "Handler", "CounterState.IncrementActionSet.Action", 27, 22, 27, 26);
  }

  public static async Task Given_HandleError_OnAppBase()
  {
    const string TestCode =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

      internal abstract class BaseHandler<TAction> : StateActionHandler<TAction>
        where TAction : IAction
      {
        protected BaseHandler(IStore store) : base(store) { }

        protected virtual Task HandleError(CancellationToken cancellationToken) => Task.CompletedTask;
      }

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

          internal sealed class Handler : BaseHandler<Action>
          {
            public Handler(IStore store) : base(store) { }

            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await HandleError(cancellationToken);
            }

            protected override async Task HandleError(CancellationToken cancellationToken)
            {
              await Store.GetState<ToastState>().AddProblemDetails();
            }
          }
        }
      }
      """;

    await RunSendDiagnosticAsync(TestCode, "Handler", "AddProblemDetails", 48, 44, 48, 61);
  }

  public static async Task Given_PartialClass_SplitAcrossFiles()
  {
    const string StateSource =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

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

    const string HandlerSource =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.State;

      public sealed partial class CounterState
      {
        public static partial class IncrementActionSet
        {
          internal sealed partial class Handler
          {
            public override async ValueTask Handle(Action action, CancellationToken cancellationToken)
            {
              await Store.GetState<CounterState>().Fetch();
            }
          }
        }
      }
      """;

    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest = new()
    {
      ReferenceAssemblies = ReferenceAssemblies.Net.Net100
    };
    AnalyzerTestFactory.AddLibraryReferences(analyzerTest);
    analyzerTest.TestState.Sources.Add(StateSource);
    analyzerTest.TestState.Sources.Add(HandlerSource);
    analyzerTest.ExpectedDiagnostics.Add(
      new DiagnosticResult("TW0002", DiagnosticSeverity.Warning)
        .WithSpan("/0/Test1.cs", 13, 46, 13, 51)
        .WithArguments("Handler", "Fetch"));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_GeneratedEntry_InGeneratedFile()
  {
    const string UserSource =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;
      using TimeWarp.State;

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
              await Store.GetState<CounterState>().Fetch();
            }
          }
        }
      }
      """;

    const string GeneratedSource =
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

    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest = new()
    {
      ReferenceAssemblies = ReferenceAssemblies.Net.Net100
    };
    AnalyzerTestFactory.AddLibraryReferences(analyzerTest);
    analyzerTest.TestState.Sources.Add(UserSource);
    analyzerTest.TestState.Sources.Add(("CounterState.FetchActionSet_Method.g.cs", GeneratedSource));
    analyzerTest.ExpectedDiagnostics.Add(
      new DiagnosticResult("TW0002", DiagnosticSeverity.Warning)
        .WithSpan(20, 46, 20, 51)
        .WithArguments("Handler", "Fetch"));

    await analyzerTest.RunAsync();
  }

  private static async Task RunSendDiagnosticAsync
  (
    string testCode,
    string handlerName,
    string actionName,
    int startLine,
    int startColumn,
    int endLine,
    int endColumn
  )
  {
    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(testCode);

    analyzerTest.ExpectedDiagnostics.Add(
      new DiagnosticResult("TW0002", DiagnosticSeverity.Warning)
        .WithSpan(startLine, startColumn, endLine, endColumn)
        .WithArguments(handlerName, actionName));

    await analyzerTest.RunAsync();
  }
}

public class Should_Not_Trigger_TW0002
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

public class Should_Trigger_TW0003
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
      new DiagnosticResult("TW0003", DiagnosticSeverity.Info)
        .WithSpan(29, 6, 29, 54)
        .WithArguments("Handler", "grandfather HandleError toast"));

    await analyzerTest.RunAsync();
  }
}
