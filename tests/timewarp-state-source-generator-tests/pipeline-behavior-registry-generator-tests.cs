#region Purpose
// PipelineBehaviorRegistrySourceGenerator records nameable open-generic IPipelineBehavior definitions.
#endregion

#region Design
// The driver runs only that generator. A concrete behavior emits one Register call. Abstract, file-local, and
// unrelated types emit no file.
#endregion

namespace PipelineBehaviorRegistryGenerator_;

public class Should_Record_Open_Generic_Behavior
{
  public static void Given_ProbeBehavior()
  {
    const string Source =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      public sealed class ProbeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
      {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) =>
          next(cancellationToken);
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) =
      StateCloneGeneratorTestDriver.Run(Source, new PipelineBehaviorRegistrySourceGenerator());
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("PipelineBehaviorRegistry.Register(typeof(global::ProbeBehavior<,>), typeof(global::TimeWarp.Mediator.IPipelineBehavior<,>))");
    outputCompilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
      .Select(diagnostic => diagnostic.ToString())
      .ShouldBeEmpty();
  }
}

public class Should_Skip_Behaviors_Generated_Code_Cannot_Name
{
  public static void Given_Abstract_Behavior()
  {
    const string Source =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      public abstract class AbstractBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
      {
        public abstract Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.Run(Source, new PipelineBehaviorRegistrySourceGenerator());
    runResult.Diagnostics.ShouldBeEmpty();
    StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldBeNull();
  }

  public static void Given_FileLocal_Behavior()
  {
    const string Source =
      """
      using System.Threading;
      using System.Threading.Tasks;
      using TimeWarp.Mediator;

      file sealed class LocalBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
      {
        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) =>
          next(cancellationToken);
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.Run(Source, new PipelineBehaviorRegistrySourceGenerator());
    runResult.Diagnostics.ShouldBeEmpty();
    StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldBeNull();
  }

  public static void Given_Unrelated_Class()
  {
    const string Source =
      """
      public sealed class Holder
      {
        public int Value { get; set; }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation _) =
      StateCloneGeneratorTestDriver.Run(Source, new PipelineBehaviorRegistrySourceGenerator());
    runResult.Diagnostics.ShouldBeEmpty();
    StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldBeNull();
  }
}
