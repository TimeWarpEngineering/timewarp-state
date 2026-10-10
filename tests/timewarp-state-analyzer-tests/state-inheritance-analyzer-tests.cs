#region Purpose
// Tests for StateInheritanceAnalyzer: State<T>'s type argument must be the declaring class, except an abstract
// self-constrained intermediate.
#endregion

#region Design
// A foreign State<T> with a wrong type argument must not report; TimeWarp.State's must. An abstract class whose
// type argument is its own self-constrained type parameter is allowed. A concrete class with that shape still
// reports. Uses the pinned Net110 references.
#endregion

// ReSharper disable InconsistentNaming
namespace StateInheritanceAnalyzer_;

public class Should_Not_Trigger_StateInheritanceRules
{
  public static async Task Given_ForeignState_WithWrongTypeArg()
  {
    const string TestCode =
      """
      namespace OtherLib
      {
        public abstract class State<T> { }
      }

      public class OtherForeignState : OtherLib.State<OtherForeignState>
      {
      }

      public class ForeignState : OtherLib.State<OtherForeignState>
      {
      }
      """;

    CSharpAnalyzerTest<StateInheritanceAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}

public class Should_Trigger_TWS0010
{
  public static async Task Given_TimeWarpState_WithWrongTypeArg()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public sealed class OtherState : State<OtherState>
      {
        public override void Initialize() { }
      }

      public sealed class WrongState : State<OtherState>
      {
        public override void Initialize() { }
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TWS0010", DiagnosticSeverity.Error)
      .WithSpan(8, 21, 8, 31);

    CSharpAnalyzerTest<StateInheritanceAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}

public class Should_Allow_Abstract_Self_Constrained_Intermediate
{
  public static async Task Given_Abstract_State_Whose_Type_Argument_Is_Its_Own_Type_Parameter()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public abstract class IntermediateState<TState> : State<TState>
        where TState : IntermediateState<TState>
      {
      }

      public sealed class ConcreteState : IntermediateState<ConcreteState>
      {
        public override void Initialize() { }
      }
      """;

    CSharpAnalyzerTest<StateInheritanceAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}

public class Should_Still_Trigger_TWS0010_For_Concrete_Generic
{
  public static async Task Given_Concrete_Generic_With_Self_Constraint()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public class ConcreteGenericState<TState> : State<TState>
        where TState : ConcreteGenericState<TState>
      {
        public override void Initialize() { }
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TWS0010", DiagnosticSeverity.Error)
      .WithSpan(3, 14, 3, 34);
    DiagnosticResult sealedDiagnostic = new DiagnosticResult("TWS0011", DiagnosticSeverity.Warning)
      .WithSpan(3, 14, 3, 34);

    CSharpAnalyzerTest<StateInheritanceAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
    analyzerTest.ExpectedDiagnostics.Add(sealedDiagnostic);

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }

  public static async Task Given_Abstract_Type_Parameter_Not_Constrained_To_Itself()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public abstract class OpenState<TState> : State<TState>
        where TState : State<TState>
      {
      }
      """;

    DiagnosticResult expectedDiagnostic = new DiagnosticResult("TWS0010", DiagnosticSeverity.Error)
      .WithSpan(3, 23, 3, 32);

    CSharpAnalyzerTest<StateInheritanceAnalyzer, FixieVerifier> analyzerTest = new()
    {
      TestCode = TestCode,
      ReferenceAssemblies = AnalyzerTestFactory.Net110
    };

    analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);

    const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));

    const string MediatorAssemblyPath = @"TimeWarp.Mediator.Contracts.dll";
    analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(MediatorAssemblyPath));

    await analyzerTest.RunAsync();
  }
}
