// // ReSharper disable InconsistentNaming
// namespace StateReadOnlyPublicPropertiesAnalyzer_;
//
// public class Should_Trigger_StateReadOnlyPublicPropertiesRule
// {
//     public static async Task Given_PublicPropertyWithPublicSetter()
//     {
//         const string TestCode = 
//             """
//             using System.Threading.Tasks;
//             using TimeWarp.State;
//
//             public class SampleState : State<SampleState>
//             {
//                 public int PublicProperty { get; set; }
//
//                 public override void Initialize() { }
//             }
//             """;
//         
//         var expectedDiagnostic = new DiagnosticResult("StateReadOnlyPublicPropertiesRule", DiagnosticSeverity.Warning)
//             .WithSpan(6, 16, 6, 30)
//             .WithArguments("PublicProperty");
//
//         var analyzerTest = new CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier>
//         {
//             TestCode = TestCode
//         };
//
//         analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
//
//         const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
//         analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));
//
//         await analyzerTest.RunAsync();
//     }
//
//     public static async Task Given_PublicPropertyWithProtectedSetter()
//     {
//         const string TestCode = 
//             """
//             using System.Threading.Tasks;
//             using TimeWarp.State;
//
//             public class SampleState : State<SampleState>
//             {
//                 public int PublicProperty { get; protected set; }
//
//                 public override void Initialize() { }
//             }
//             """;
//
//         var expectedDiagnostic = new DiagnosticResult("StateReadOnlyPublicPropertiesRule", DiagnosticSeverity.Warning)
//             .WithSpan(6, 16, 6, 30)
//             .WithArguments("PublicProperty");
//
//         var analyzerTest = new CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier>
//         {
//             TestCode = TestCode
//         };
//
//         analyzerTest.ExpectedDiagnostics.Add(expectedDiagnostic);
//
//         const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
//         analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));
//
//         await analyzerTest.RunAsync();
//     }
//
//     public static async Task Given_PublicPropertyWithPrivateSetter()
//     {
//         const string TestCode = 
//             """
//             using System.Threading.Tasks;
//             using TimeWarp.State;
//
//             public class SampleState : State<SampleState>
//             {
//                 public int PublicProperty { get; private set; }
//
//                 public override void Initialize() { }
//             }
//             """;
//
//         var analyzerTest = new CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier>
//         {
//             TestCode = TestCode
//         };
//
//         const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
//         analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));
//
//         await analyzerTest.RunAsync();
//     }
//
//     public static async Task Given_PublicReadOnlyProperty()
//     {
//         const string TestCode = 
//             """
//             using System.Threading.Tasks;
//             using TimeWarp.State;
//
//             public class SampleState : State<SampleState>
//             {
//                 public int PublicProperty { get; }
//
//                 public override void Initialize() { }
//             }
//             """;
//
//         var analyzerTest = new CSharpAnalyzerTest<StateReadOnlyPublicPropertiesAnalyzer, FixieVerifier>
//         {
//             TestCode = TestCode
//         };
//
//         const string TimeWarpStateAssemblyPath = @"TimeWarp.State.dll";
//         analyzerTest.TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(TimeWarpStateAssemblyPath));
//
//         await analyzerTest.RunAsync();
//     }
// }
