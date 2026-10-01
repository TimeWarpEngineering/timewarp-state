#region Purpose
// CatalogActionAnalyzer: TWS0004 placement, TWS0005 description, TWS0006 duplicate name, TWS0007 plain sentence,
// TWS0008 DisplayName.
#endregion

// ReSharper disable InconsistentNaming
namespace CatalogActionAnalyzer_;

using static CatalogActionAnalyzerRunner;

public class Should_Not_Report
{
  public static async Task Given_Valid_Cataloged_Actions()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          [CatalogAction(Description = "Increment the counter.")]
          public sealed class Action : IAction { }
        }

        public static class ResetActionSet
        {
          [CatalogAction(Description = "Reset the counter", Name = "Counter.Clear", DisplayName = "Clear Counter", Visibility = ActionVisibility.Both, Permissions = new[] { "counter.write" })]
          public sealed class Action : IAction { }
        }

        public static class FetchActionSet
        {
          public sealed class Action : IAction { }
        }

        public static class LinkActionSet
        {
          [CatalogAction(Description = "Link an account.", DisplayName = null)]
          public sealed class Action : IAction { }
        }
      }
      """;

    await AnalyzerTestFactory.Create<CatalogActionAnalyzer>(TestCode).RunAsync();
  }
}

public class Should_Report_TWS0004
{
  public static async Task Given_Attribute_On_Top_Level_Class()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      [{|#0:CatalogAction(Description = "Do a thing.")|}]
      public sealed class Action { }
      """;

    await RunAsync(TestCode, new DiagnosticResult(CatalogActionAnalyzer.PlacementDiagnosticId, DiagnosticSeverity.Error).WithLocation(0).WithArguments("Action"));
  }

  public static async Task Given_Attribute_On_Non_Action_Nested_Class()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          [{|#0:CatalogAction(Description = "Handle it.")|}]
          public sealed class Handler { }
        }
      }
      """;

    await RunAsync
    (
      TestCode,
      new DiagnosticResult(CatalogActionAnalyzer.PlacementDiagnosticId, DiagnosticSeverity.Error)
        .WithLocation(0)
        .WithArguments("CounterState.IncrementActionSet.Handler")
    );
  }

  public static async Task Given_Action_Not_In_An_ActionSet()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public static class Outer
      {
        public static class Commands
        {
          [{|#0:CatalogAction(Description = "Do a thing.")|}]
          public sealed class Action { }
        }
      }
      """;

    await RunAsync
    (
      TestCode,
      new DiagnosticResult(CatalogActionAnalyzer.PlacementDiagnosticId, DiagnosticSeverity.Error)
        .WithLocation(0)
        .WithArguments("Outer.Commands.Action")
    );
  }

  public static async Task Given_Generic_State()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState<T> : State<CounterState<T>>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          [{|#0:CatalogAction(Description = "Increment the counter.")|}]
          public sealed class Action : IAction { }
        }
      }
      """;

    await RunAsync
    (
      TestCode,
      new DiagnosticResult(CatalogActionAnalyzer.PlacementDiagnosticId, DiagnosticSeverity.Error)
        .WithLocation(0)
        .WithArguments("CounterState<T>.IncrementActionSet.Action")
    );
  }

  public static async Task Given_Private_ActionSet()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        private static class IncrementActionSet
        {
          [{|#0:CatalogAction(Description = "Increment the counter.")|}]
          public sealed class Action : IAction { }
        }
      }
      """;

    await RunAsync
    (
      TestCode,
      new DiagnosticResult(CatalogActionAnalyzer.PlacementDiagnosticId, DiagnosticSeverity.Error)
        .WithLocation(0)
        .WithArguments("CounterState.IncrementActionSet.Action")
    );
  }
}

public class Should_Report_TWS0005
{
  public static async Task Given_Missing_Description()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          [{|#0:CatalogAction|}]
          public sealed class Action : IAction { }
        }
      }
      """;

    await RunAsync(TestCode, Description(0));
  }

  public static async Task Given_Whitespace_Description()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          [{|#0:CatalogAction(Description = "  ")|}]
          public sealed class Action : IAction { }
        }
      }
      """;

    await RunAsync(TestCode, Description(0));
  }

  private static DiagnosticResult Description(int location) =>
    new DiagnosticResult(CatalogActionAnalyzer.DescriptionDiagnosticId, DiagnosticSeverity.Error)
      .WithLocation(location)
      .WithArguments("CounterState.IncrementActionSet.Action");
}

public class Should_Report_TWS0006
{
  public static async Task Given_Explicit_Name_Colliding_With_Default_Name()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          [{|#0:CatalogAction(Description = "Increment the counter.")|}]
          public sealed class Action : IAction { }
        }

        public static class BumpActionSet
        {
          [{|#1:CatalogAction(Description = "Bump the counter.", Name = "Counter.Increment")|}]
          public sealed class Action : IAction { }
        }
      }
      """;

    await RunAsync
    (
      TestCode,
      new DiagnosticResult(CatalogActionAnalyzer.DuplicateNameDiagnosticId, DiagnosticSeverity.Error).WithLocation(0).WithArguments("Counter.Increment"),
      new DiagnosticResult(CatalogActionAnalyzer.DuplicateNameDiagnosticId, DiagnosticSeverity.Error).WithLocation(1).WithArguments("Counter.Increment")
    );
  }
}

public class Should_Report_TWS0007
{
  public static async Task Given_Two_Sentences()
  {
    const string TestCode =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }

        public static class IncrementActionSet
        {
          [{|#0:CatalogAction(Description = "Increment the counter. Then render it.")|}]
          public sealed class Action : IAction { }
        }
      }
      """;

    await RunAsync
    (
      TestCode,
      new DiagnosticResult(CatalogActionAnalyzer.PlainSentenceDiagnosticId, DiagnosticSeverity.Warning)
        .WithLocation(0)
        .WithArguments("CounterState.IncrementActionSet.Action")
    );
  }
}

public class Should_Report_TWS0008
{
  public static async Task Given_Empty_DisplayName()
  {
    await RunAsync(Source("\"\""), DisplayName(0));
  }

  public static async Task Given_Whitespace_DisplayName()
  {
    await RunAsync(Source("\"  \""), DisplayName(0));
  }

  public static async Task Given_Constant_Field_DisplayName_Not_Reported()
  {
    await RunAsync(Source("Labels.Increment"));
  }

  public static async Task Given_Non_Constant_DisplayName_Compiler_Rejects_It()
  {
    await RunAsync
    (
      Source("{|#1:Labels.Dynamic|}"),
      DiagnosticResult.CompilerError("CS0182").WithLocation(1)
    );
  }

  private static string Source(string displayName) =>
    $$"""
    using TimeWarp.Mediator;
    using TimeWarp.State;

    public static class Labels
    {
      public const string Increment = "Increment";
      public static string Dynamic => "Increment";
    }

    public sealed partial class CounterState : State<CounterState>
    {
      public override void Initialize() { }

      public static class IncrementActionSet
      {
        [{|#0:CatalogAction(Description = "Increment the counter.", DisplayName = {{displayName}})|}]
        public sealed class Action : IAction { }
      }
    }
    """;

  private static DiagnosticResult DisplayName(int location) =>
    new DiagnosticResult(CatalogActionAnalyzer.DisplayNameDiagnosticId, DiagnosticSeverity.Error)
      .WithLocation(location)
      .WithArguments("CounterState.IncrementActionSet.Action");
}

internal static class CatalogActionAnalyzerRunner
{
  public static async Task RunAsync(string testCode, params DiagnosticResult[] expected)
  {
    CSharpAnalyzerTest<CatalogActionAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<CatalogActionAnalyzer>(testCode);
    analyzerTest.ExpectedDiagnostics.AddRange(expected);
    await analyzerTest.RunAsync();
  }
}
