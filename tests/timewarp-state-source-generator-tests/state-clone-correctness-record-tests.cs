#region Purpose
// Cancellation and reference-assembly record clone regressions.
#endregion

#region Design
// The cancelled plan throws before it emits. The record clone runs against the implementation image.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Stop_When_Planning_Is_Cancelled
{
  public static void Given_Already_Cancelled_Token()
  {
    CSharpCompilation compilation = CSharpCompilation.Create("Cancelled");
    StateClonePlanner planner = new(compilation);
    Should.Throw<OperationCanceledException>(() => planner.Plan(default, new CancellationToken(true)));
  }
}

public class Should_Clone_Reference_Assembly_Record
{
  public static void Given_Unsealed_Record_Without_EqualityContract_Field()
  {
    const string BaseSource =
      """
      namespace Foreign;

      public record Dto(string Name, int Count);
      """;

    const string DerivedSource =
      """
      using TimeWarp.State;

      [GenerateClone]
      public sealed class Holder
      {
        public Foreign.Dto? Dto { get; set; }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation, Assembly loaded) =
      StateCloneGeneratorTestDriver.LoadReferenceClone(BaseSource, DerivedSource, "ForeignRecord");
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldNotContain("EqualityContract");
    source.ShouldContain("<Name>k__BackingField");
    source.ShouldContain("<Count>k__BackingField");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();

    Type holderType = loaded.GetType("Holder").ShouldNotBeNull();
    object holder = Activator.CreateInstance(holderType).ShouldNotBeNull();
    PropertyInfo dtoProperty = holderType.GetProperty("Dto").ShouldNotBeNull();
    object dto = Activator.CreateInstance(dtoProperty.PropertyType, "ada", 3).ShouldNotBeNull();
    dtoProperty.SetValue(holder, dto);

    MethodInfo clone = loaded.GetType("TimeWarp.Features.Cloning.GeneratedCloneExtensions")
      .ShouldNotBeNull()
      .GetMethods()
      .Single(method => method.Name == "Clone");
    object cloned = Invoke(clone, holder).ShouldNotBeNull();
    object clonedDto = dtoProperty.GetValue(cloned).ShouldNotBeNull();
    clonedDto.ShouldNotBeSameAs(dto);
    dtoProperty.PropertyType.GetProperty("Name")!.GetValue(clonedDto).ShouldBe("ada");
    dtoProperty.PropertyType.GetProperty("Count")!.GetValue(clonedDto).ShouldBe(3);
  }

  private static object? Invoke(MethodInfo method, object target)
  {
    try
    {
      return method.Invoke(null, [target]);
    }
    catch (TargetInvocationException exception) when (exception.InnerException is not null)
    {
      throw exception.InnerException;
    }
  }
}

internal static class CorrectnessDiagnostics
{
  public static IEnumerable<string> GeneratedErrors(Compilation compilation) =>
    compilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath.EndsWith(".g.cs", StringComparison.Ordinal) == true)
      .Select(diagnostic => diagnostic.ToString());

  public static IEnumerable<string> CompilerErrors(Compilation compilation) =>
    compilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Id.StartsWith("CS", StringComparison.Ordinal))
      .Select(diagnostic => diagnostic.ToString());
}
