#region Purpose
// Shared source snippets and run helpers for the HandlerMustNotSendActionAnalyzer tests.
#endregion

#region Design
// Spans come from markup, never line/column numbers: wrap the expected location in {|#0:...|} in the source and
// RunAsync binds it to a TWS0002 or TWS0003 result via WithLocation(0), so whitespace edits cannot break a test.
// Each snippet const ends with a blank line, so snippets concatenate with plain +.
#endregion

namespace HandlerMustNotSendActionAnalyzer_;

internal static class HandlerSendSources
{
  public const string Usings =
    """
    using System.Threading;
    using System.Threading.Tasks;
    using TimeWarp.Mediator;
    using TimeWarp.State;

    """;

  // A state whose generated-style entry method (AddProblemDetails) sends the nested action.
  public const string ToastState =
    """
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

    """;

  public static Task RunSendAsync(string testCode, string handlerName, string actionName) =>
    RunAsync(AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(testCode), SendResult(handlerName, actionName));

  public static Task RunNoDiagnosticAsync(string testCode) =>
    AnalyzerTestFactory.Create<HandlerMustNotSendActionAnalyzer>(testCode).RunAsync();

  public static DiagnosticResult SendResult(string handlerName, string actionName) =>
    new DiagnosticResult("TWS0002", DiagnosticSeverity.Warning).WithLocation(0).WithArguments(handlerName, actionName);

  public static DiagnosticResult AllowResult(string handlerName, string reason) =>
    new DiagnosticResult("TWS0003", DiagnosticSeverity.Info).WithLocation(0).WithArguments(handlerName, reason);

  public static Task RunAsync
  (
    CSharpAnalyzerTest<HandlerMustNotSendActionAnalyzer, FixieVerifier> analyzerTest,
    params DiagnosticResult[] expected
  )
  {
    analyzerTest.ExpectedDiagnostics.AddRange(expected);
    return analyzerTest.RunAsync();
  }
}
