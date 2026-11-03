#region Purpose
// Regression tests for clone-planner correctness: JsonIgnore conditions, nameability, share roots, comparers, cancellation, and record EqualityContract.
#endregion

#region Design
// Each case compiles the generator output. The reference-assembly record also runs the clone against the implementation
// image, so a missing EqualityContract backing field fails here instead of only in the generated text.
#endregion

namespace StateCloneSourceGenerator_;

public class Should_Respect_JsonIgnore_Condition
{
  public static void Given_WhenWriting_Conditions()
  {
    const string Source =
      """
      using System.Text.Json.Serialization;
      using TimeWarp.State;

      [GenerateClone]
      public sealed class Holder
      {
        [JsonIgnore]
        public string AlwaysDefault { get; set; } = "a";

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string Always { get; set; } = "b";

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Nulls { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Defaults { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string Never { get; set; } = "n";
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldNotContain("<AlwaysDefault>k__BackingField");
    source.ShouldNotContain("<Always>k__BackingField");
    source.ShouldContain("<Nulls>k__BackingField");
    source.ShouldContain("<Defaults>k__BackingField");
    source.ShouldContain("<Never>k__BackingField");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Not_Name_Unnameable_Types
{
  public static void Given_FileLocal_ICloneable_State()
  {
    const string Source =
      """
      using System;
      using TimeWarp.State;

      file sealed class LocalState : State<LocalState>, ICloneable
      {
        public int V { get; set; }
        public override void Initialize() { }
        public object Clone() => new LocalState { V = V };
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldBeNull();
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }

  public static void Given_ICloneable_Closed_Over_Private_Nested_Type()
  {
    const string Source =
      """
      using System;
      using TimeWarp.State;

      public sealed class Outer
      {
        private sealed class Secret { }

        public sealed class Box<T> : State<Box<T>>, ICloneable
        {
          public override void Initialize() { }
          public object Clone() => new Box<T>();
        }

        private Box<Secret> Current { get; } = new();
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string? source = StateCloneGeneratorTestDriver.CloneSource(runResult);
    source?.ShouldNotContain("Secret");
    source?.ShouldNotContain("RootStateForTrim");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }

  public static void Given_ICloneable_Closed_Over_FileLocal_Type()
  {
    const string Source =
      """
      using System;
      using TimeWarp.State;

      file sealed class Secret { public int V { get; set; } }

      public sealed class Box<T> : State<Box<T>>, ICloneable
      {
        public T? Value { get; set; }
        public override void Initialize() { }
        public object Clone() => new Box<T>();
      }

      file static class Mention
      {
        public static object Sample() => new Box<Secret>();
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string? source = StateCloneGeneratorTestDriver.CloneSource(runResult);
    source?.ShouldNotContain("Secret");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }

  public static void Given_Internal_Type_From_Another_Assembly()
  {
    const string BaseSource =
      """
      using System.Runtime.CompilerServices;

      [assembly: InternalsVisibleTo("CloneGeneratorTests")]

      namespace Foreign;

      public class Outer
      {
        internal class Secret { public int V { get; set; } }
      }
      """;

    const string DerivedSource =
      """
      using System;
      using TimeWarp.State;

      public sealed class Box<T> : State<Box<T>>, ICloneable
      {
        public T? Value { get; set; }
        public override void Initialize() { }
        public object Clone() => new Box<T>();
      }

      internal static class Mention
      {
        internal static Box<Foreign.Outer.Secret> Sample() => new();
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) =
      StateCloneGeneratorTestDriver.RunWithMetadataBase(BaseSource, DerivedSource);
    runResult.Diagnostics.ShouldBeEmpty();
    string? source = StateCloneGeneratorTestDriver.CloneSource(runResult);
    source?.ShouldNotContain("Secret");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }

  public static void Given_GenerateClone_Closed_Over_Private_Type_Reports_And_Does_Not_Emit_It()
  {
    const string Source =
      """
      using TimeWarp.State;

      public sealed class Outer
      {
        private sealed class Secret { public int V { get; set; } }

        [GenerateClone]
        public sealed class Wrapper<T>
        {
          public T? Value { get; set; }
        }

        private Wrapper<Secret> Current { get; } = new();
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.Select(diagnostic => diagnostic.Id).ShouldContain(StateCloneSourceGenerator.DiagnosticId);
    string? source = StateCloneGeneratorTestDriver.CloneSource(runResult);
    source?.ShouldNotContain("Secret");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Keep_Internal_Extension_For_Internal_Type_Argument
{
  public static void Given_Wrapper_Of_Internal_And_Public()
  {
    const string Source =
      """
      using TimeWarp.State;

      [GenerateClone]
      public sealed class Wrapper<T>
      {
        public T? Value { get; set; }
      }

      internal sealed class Secret { public int V { get; set; } }

      public sealed class Visible { public int V { get; set; } }

      internal sealed class Mention
      {
        internal Wrapper<Secret> Hidden { get; set; } = new();
        internal Wrapper<Visible> Shown { get; set; } = new();
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("internal static global::Wrapper<global::Secret>? Clone");
    source.ShouldNotContain("public static global::Wrapper<global::Secret>? Clone");
    source.ShouldContain("public static global::Wrapper<global::Visible>? Clone");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Emit_Identity_Clone_For_Shared_Struct
{
  public static void Given_Unmanaged_And_Shared_Fields()
  {
    const string Source =
      """
      using TimeWarp.State;

      [GenerateClone]
      public struct Point
      {
        public int X;
        public int Y;
      }

      [GenerateClone]
      public struct Named
      {
        public string? Name { get; set; }
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("static global::Point Clone(this global::Point source)");
    source.ShouldContain("static global::Named Clone(this global::Named source)");
    source.ShouldContain("return source;");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }
}

public class Should_Pass_Comparer_To_Collection_Subclass
{
  public static void Given_Constructors_That_Forward_Comparer()
  {
    const string Source =
      """
      using System.Collections.Generic;
      using TimeWarp.State;

      public sealed class TaggedSet : HashSet<string>
      {
        public TaggedSet() { }
        public TaggedSet(IEqualityComparer<string>? comparer) : base(comparer) { }
        public TaggedSet(int capacity, IEqualityComparer<string>? comparer) : base(capacity, comparer) { }
        public int Version { get; set; }
      }

      public sealed class TaggedSortedSet : SortedSet<string>
      {
        public TaggedSortedSet() { }
        public TaggedSortedSet(IComparer<string>? comparer) : base(comparer) { }
        public int Version { get; set; }
      }

      public sealed class TaggedMap : Dictionary<string, int>
      {
        public TaggedMap() { }
        public TaggedMap(int capacity, IEqualityComparer<string>? comparer) : base(capacity, comparer) { }
        public int Version { get; set; }
      }

      public sealed class TaggedSortedMap : SortedDictionary<string, int>
      {
        public TaggedSortedMap() { }
        public TaggedSortedMap(IComparer<string>? comparer) : base(comparer) { }
        public int Version { get; set; }
      }

      public sealed class TaggedList : SortedList<string, int>
      {
        public TaggedList() { }
        public TaggedList(int capacity, IComparer<string>? comparer) : base(capacity, comparer) { }
        public int Version { get; set; }
      }

      [GenerateClone]
      public sealed class Holder
      {
        public TaggedSet Set { get; set; } = new();
        public TaggedSortedSet Sorted { get; set; } = new();
        public TaggedMap Map { get; set; } = new();
        public TaggedSortedMap SortedMap { get; set; } = new();
        public TaggedList List { get; set; } = new();
      }
      """;

    (GeneratorDriverRunResult runResult, Compilation outputCompilation) = StateCloneGeneratorTestDriver.Run(Source);
    runResult.Diagnostics.ShouldBeEmpty();
    string source = StateCloneGeneratorTestDriver.CloneSource(runResult).ShouldNotBeNull();
    source.ShouldContain("global::TaggedSet clone = new(source.Count, source.Comparer);");
    source.ShouldContain("global::TaggedSortedSet clone = new(source.Comparer);");
    source.ShouldContain("global::TaggedMap clone = new(source.Count, source.Comparer);");
    source.ShouldContain("global::TaggedSortedMap clone = new(source.Comparer);");
    source.ShouldContain("global::TaggedList clone = new(source.Count, source.Comparer);");
    CorrectnessDiagnostics.GeneratedErrors(outputCompilation).ShouldBeEmpty();
  }
}

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

file static class CorrectnessDiagnostics
{
  public static IEnumerable<string> GeneratedErrors(Compilation compilation) =>
    compilation.GetDiagnostics()
      .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error && diagnostic.Location.SourceTree?.FilePath.EndsWith(".g.cs", StringComparison.Ordinal) == true)
      .Select(diagnostic => diagnostic.ToString());
}
