#region Purpose
// Tests for StateJsonSerializableAnalyzer (TWS0013) and StateJsonSerializableCodeFixProvider: a state that persistence or
// telemetry snapshots serialize must be listed by a JsonSerializerContext, and the fix writes the missing line.
#endregion

#region Design
// The in-memory compilation runs no System.Text.Json generator, so a context's generated half is missing.
// CompilerDiagnostics.None keeps those compiler errors out of the assertions; only TWS0013 is compared.
// Telemetry is detected by type name, so the tests declare a stand-in TimeWarpStateTelemetryOptions instead of
// referencing TimeWarp.State.Telemetry.dll.
#endregion

// ReSharper disable InconsistentNaming
namespace StateJsonSerializableAnalyzer_;

internal static class Tws0013
{
  internal const string TelemetryStandIn =
    """

    namespace TimeWarp.State.Telemetry
    {
      public sealed class TimeWarpStateTelemetryOptions { }
    }
    """;

  internal static CSharpAnalyzerTest<StateJsonSerializableAnalyzer, FixieVerifier> Analyzer(string testCode)
  {
    CSharpAnalyzerTest<StateJsonSerializableAnalyzer, FixieVerifier> analyzerTest =
      AnalyzerTestFactory.Create<StateJsonSerializableAnalyzer>(testCode);
    analyzerTest.CompilerDiagnostics = CompilerDiagnostics.None;
    return analyzerTest;
  }

  internal static CSharpCodeFixTest<StateJsonSerializableAnalyzer, StateJsonSerializableCodeFixProvider, FixieVerifier> CodeFix()
  {
    CSharpCodeFixTest<StateJsonSerializableAnalyzer, StateJsonSerializableCodeFixProvider, FixieVerifier> codeFixTest = new()
    {
      ReferenceAssemblies = AnalyzerTestFactory.Net110,
      CompilerDiagnostics = CompilerDiagnostics.None
    };

    // The code action's formatter reads indentation from .editorconfig, as it does in a real project.
    codeFixTest.TestState.AnalyzerConfigFiles.Add(("/.editorconfig", "root = true\n\n[*.cs]\nindent_style = space\nindent_size = 2\n"));
    codeFixTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile("TimeWarp.State.dll"));
    codeFixTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile("TimeWarp.Mediator.dll"));
    codeFixTest.FixedState.AdditionalReferences.Add(MetadataReference.CreateFromFile("TimeWarp.State.dll"));
    codeFixTest.FixedState.AdditionalReferences.Add(MetadataReference.CreateFromFile("TimeWarp.Mediator.dll"));
    return codeFixTest;
  }

  internal static DiagnosticResult Expected(int line, int column, string stateName, string reason, string path = "") =>
    new DiagnosticResult(StateJsonSerializableAnalyzer.DiagnosticId, DiagnosticSeverity.Warning)
      .WithSpan(path, line, column, line, column + stateName.Length)
      .WithArguments(stateName, reason);
}

public class Should_Trigger_TWS0013
{
  public static async Task Given_PersistentState_NotListed_By_Existing_Context()
  {
    const string TestCode =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.Features.Persistence;
      using TimeWarp.State;

      [PersistentState(PersistentStateMethod.SessionStorage)]
      public sealed partial class CartState : State<CartState>
      {
        public override void Initialize() { }
      }

      public sealed class OtherType { }

      [JsonSerializable(typeof(OtherType))]
      internal sealed partial class AppJsonContext : JsonSerializerContext;
      """;

    CSharpAnalyzerTest<StateJsonSerializableAnalyzer, FixieVerifier> analyzerTest = Tws0013.Analyzer(TestCode);
    analyzerTest.ExpectedDiagnostics.Add(Tws0013.Expected(6, 29, "CartState", "persistence"));
    await analyzerTest.RunAsync();
  }

  public static async Task Given_Telemetry_Referenced_And_No_Context()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }
      }
      """ + Tws0013.TelemetryStandIn;

    CSharpAnalyzerTest<StateJsonSerializableAnalyzer, FixieVerifier> analyzerTest = Tws0013.Analyzer(TestCode);
    analyzerTest.ExpectedDiagnostics.Add(Tws0013.Expected(3, 29, "CounterState", "telemetry snapshots"));
    await analyzerTest.RunAsync();
  }

  public static async Task Given_PersistentState_And_Telemetry_Not_Listed()
  {
    const string TestCode =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.Features.Persistence;
      using TimeWarp.State;

      [PersistentState(PersistentStateMethod.LocalStorage)]
      public sealed partial class CartState : State<CartState>
      {
        public override void Initialize() { }
      }

      internal sealed partial class AppJsonContext : JsonSerializerContext;
      """ + Tws0013.TelemetryStandIn;

    CSharpAnalyzerTest<StateJsonSerializableAnalyzer, FixieVerifier> analyzerTest = Tws0013.Analyzer(TestCode);
    analyzerTest.ExpectedDiagnostics.Add(Tws0013.Expected(6, 29, "CartState", "persistence and telemetry snapshots"));
    await analyzerTest.RunAsync();
  }

  public static async Task Given_State_Derived_From_Intermediate_Base()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public abstract class BaseState<T> : State<T> where T : BaseState<T> { }

      public sealed class ProfileState : BaseState<ProfileState>
      {
        public override void Initialize() { }
      }
      """ + Tws0013.TelemetryStandIn;

    CSharpAnalyzerTest<StateJsonSerializableAnalyzer, FixieVerifier> analyzerTest = Tws0013.Analyzer(TestCode);
    analyzerTest.ExpectedDiagnostics.Add(Tws0013.Expected(5, 21, "ProfileState", "telemetry snapshots"));
    await analyzerTest.RunAsync();
  }
}

public class Should_Not_Trigger_TWS0013
{
  public static async Task Given_PersistentState_Listed_By_Context()
  {
    const string TestCode =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.Features.Persistence;
      using TimeWarp.State;

      namespace App.Features
      {
        [PersistentState(PersistentStateMethod.SessionStorage)]
        public sealed partial class CartState : State<CartState>
        {
          public override void Initialize() { }
        }
      }

      namespace App
      {
        public static class Outer
        {
          [JsonSerializable(typeof(App.Features.CartState))]
          internal sealed partial class NestedJsonContext : JsonSerializerContext;
        }
      }
      """ + Tws0013.TelemetryStandIn;

    await Tws0013.Analyzer(TestCode).RunAsync();
  }

  public static async Task Given_PersistentState_And_No_Context()
  {
    // Persistence falls back to the reflection resolver when the project has no context.
    const string TestCode =
      """
      using TimeWarp.Features.Persistence;
      using TimeWarp.State;

      [PersistentState(PersistentStateMethod.SessionStorage)]
      public sealed partial class CartState : State<CartState>
      {
        public override void Initialize() { }
      }
      """;

    await Tws0013.Analyzer(TestCode).RunAsync();
  }

  public static async Task Given_Plain_State_Without_Telemetry()
  {
    const string TestCode =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }
      }

      internal sealed partial class AppJsonContext : JsonSerializerContext;
      """;

    await Tws0013.Analyzer(TestCode).RunAsync();
  }

  public static async Task Given_Abstract_Generic_Or_Foreign_State()
  {
    const string TestCode =
      """
      using TimeWarp.State;

      public abstract class AbstractState : State<AbstractState> { }

      public class GenericState<T> : State<GenericState<T>>
      {
        public override void Initialize() { }
      }

      namespace OtherLib
      {
        public abstract class State<T> { }
        public sealed class ForeignState : State<ForeignState> { }
      }
      """ + Tws0013.TelemetryStandIn;

    await Tws0013.Analyzer(TestCode).RunAsync();
  }
}

public class Should_Fix_TWS0013
{
  public static async Task Given_Context_With_Attributes_Appends_Line()
  {
    const string Source =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.Features.Persistence;
      using TimeWarp.State;

      namespace App;

      [PersistentState(PersistentStateMethod.SessionStorage)]
      public sealed partial class CartState : State<CartState>
      {
        public override void Initialize() { }
      }

      public sealed class OtherType { }

      [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
      [JsonSerializable(typeof(OtherType))]
      internal sealed partial class AppJsonContext : JsonSerializerContext;
      """;

    const string Fixed =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.Features.Persistence;
      using TimeWarp.State;

      namespace App;

      [PersistentState(PersistentStateMethod.SessionStorage)]
      public sealed partial class CartState : State<CartState>
      {
        public override void Initialize() { }
      }

      public sealed class OtherType { }

      [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
      [JsonSerializable(typeof(OtherType))]
      [JsonSerializable(typeof(CartState))]
      internal sealed partial class AppJsonContext : JsonSerializerContext;
      """;

    CSharpCodeFixTest<StateJsonSerializableAnalyzer, StateJsonSerializableCodeFixProvider, FixieVerifier> codeFixTest = Tws0013.CodeFix();
    codeFixTest.TestCode = Source;
    codeFixTest.FixedCode = Fixed;
    codeFixTest.ExpectedDiagnostics.Add(Tws0013.Expected(8, 29, "CartState", "persistence"));
    await codeFixTest.RunAsync();
  }

  public static async Task Given_Context_In_Another_File_And_Namespace_Without_Attributes()
  {
    const string StateSource =
      """
      using TimeWarp.Features.Persistence;
      using TimeWarp.State;

      namespace App.Features.Cart
      {
        [PersistentState(PersistentStateMethod.LocalStorage)]
        public sealed partial class CartState : State<CartState>
        {
          public override void Initialize() { }
        }
      }
      """;

    const string ContextSource =
      """
      using System.Text.Json.Serialization;

      namespace App
      {
        /// <summary>App JSON.</summary>
        internal sealed partial class AppJsonContext : JsonSerializerContext
        {
        }
      }
      """;

    const string FixedContextSource =
      """
      using System.Text.Json.Serialization;

      namespace App
      {
        /// <summary>App JSON.</summary>
        [JsonSerializable(typeof(Features.Cart.CartState))]
        internal sealed partial class AppJsonContext : JsonSerializerContext
        {
        }
      }
      """;

    CSharpCodeFixTest<StateJsonSerializableAnalyzer, StateJsonSerializableCodeFixProvider, FixieVerifier> codeFixTest = Tws0013.CodeFix();
    codeFixTest.TestState.Sources.Add(("/0/cart-state.cs", StateSource));
    codeFixTest.TestState.Sources.Add(("/0/app-json-context.cs", ContextSource));
    codeFixTest.FixedState.Sources.Add(("/0/cart-state.cs", StateSource));
    codeFixTest.FixedState.Sources.Add(("/0/app-json-context.cs", FixedContextSource));
    codeFixTest.ExpectedDiagnostics.Add(Tws0013.Expected(7, 31, "CartState", "persistence", "/0/cart-state.cs"));
    await codeFixTest.RunAsync();
  }

  public static async Task Given_Two_Contexts_Offers_One_Action_Per_Context()
  {
    const string Source =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }
      }

      internal sealed partial class BContext : JsonSerializerContext;

      internal sealed partial class AContext : JsonSerializerContext;
      """ + Tws0013.TelemetryStandIn;

    const string Fixed =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.State;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }
      }

      [JsonSerializable(typeof(CounterState))]
      internal sealed partial class BContext : JsonSerializerContext;

      internal sealed partial class AContext : JsonSerializerContext;
      """ + Tws0013.TelemetryStandIn;

    CSharpCodeFixTest<StateJsonSerializableAnalyzer, StateJsonSerializableCodeFixProvider, FixieVerifier> codeFixTest = Tws0013.CodeFix();
    codeFixTest.TestCode = Source;
    codeFixTest.FixedCode = Fixed;
    codeFixTest.CodeActionIndex = 1; // actions are sorted by context name: AContext, BContext
    codeFixTest.CodeActionEquivalenceKey = "AddAttributeAsyncBContext";
    codeFixTest.ExpectedDiagnostics.Add(Tws0013.Expected(4, 29, "CounterState", "telemetry snapshots"));
    await codeFixTest.RunAsync();
  }

  public static async Task Given_No_Context_Creates_One()
  {
    const string Source =
      """
      using TimeWarp.State;

      namespace App.Features;

      public sealed partial class CounterState : State<CounterState>
      {
        public override void Initialize() { }
      }
      """;

    const string NewContext =
      """
      using System.Text.Json.Serialization;

      namespace App.Features;

      /// <summary>
      /// Source-generated JSON type info for TimeWarp.State persistence and telemetry snapshots. Wire it with
      /// <c>AddTimeWarpState(options => options.JsonSerializerOptions.TypeInfoResolver = StateJsonContext.Default)</c>.
      /// </summary>
      [JsonSerializable(typeof(CounterState))]
      internal sealed partial class StateJsonContext : JsonSerializerContext
      {
      }

      """;

    CSharpCodeFixTest<StateJsonSerializableAnalyzer, StateJsonSerializableCodeFixProvider, FixieVerifier> codeFixTest = Tws0013.CodeFix();
    codeFixTest.TestState.Sources.Add(("/0/counter-state.cs", Source));
    codeFixTest.TestState.Sources.Add(("/0/telemetry.cs", Tws0013.TelemetryStandIn));
    codeFixTest.FixedState.Sources.Add(("/0/counter-state.cs", Source));
    codeFixTest.FixedState.Sources.Add(("/0/telemetry.cs", Tws0013.TelemetryStandIn));
    codeFixTest.FixedState.Sources.Add(("/0/state-json-context.cs", NewContext));
    codeFixTest.ExpectedDiagnostics.Add(Tws0013.Expected(5, 29, "CounterState", "telemetry snapshots", "/0/counter-state.cs"));
    await codeFixTest.RunAsync();
  }
}
