#region Purpose
// Source text for the generic and tuple member shapes StateCloneSourceGenerator tests.
#endregion

#region Design
// Generic and tuple member shapes: generic holders, generic state bases, nested generics, tuples, key-value pairs, nullable structs, and generic subtypes of generic member types.
// Every key has one [Input] on the matching Given_Shape test; state-clone-shape-guard-tests.cs fails when they drift.
#endregion

namespace StateCloneSourceGenerator_;

internal static partial class StateCloneShapeSources
{
  public static readonly Dictionary<string, string> GenericShapes = new()
  {
    ["GenericWrapper"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Wrapper<T> { public T? Value { get; set; } }
      [GenerateClone] public class UsesWrapper { public Wrapper<Item> W { get; set; } = new(); }
      """,
    ["GenericStateBase"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public abstract class ListState<TState> : State<TState> where TState : ListState<TState>
      {
        public List<TState> History { get; set; } = new();
        public TState? Previous { get; set; }
      }
      public sealed class HistState : ListState<HistState>
      {
        public int Count { get; set; }
        public override void Initialize() { }
      }
      """,
    ["NestedGeneric"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      public class Outer<T> { public class Inner { public T? Value; public System.Collections.Generic.List<T> Values = new(); } }
      [GenerateClone] public class UsesNested { public Outer<Item>.Inner? I { get; set; } }
      """,
    ["Tuple"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class TupleHolder
      {
        public (string Name, Item It) T { get; set; }
        public (int, int, int, int, int, int, int, Item) Long { get; set; }
      }
      """,
    ["KeyValuePair"] =
      $$"""
      using System.Collections.Generic;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class KvpHolder { public List<KeyValuePair<string, Item>> Pairs { get; set; } = new(); }
      """,
    ["NullableStruct"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public struct Box { public List<int> Values; }
      [GenerateClone] public class NullableHolder { public Box? B { get; set; } }
      """,
    ["GenericSubclassOfGenericClass"] =
      """
      using TimeWarp.State;
      public class GB<T> { public T? V { get; set; } }
      public class GD<T> : GB<T> { public int W { get; set; } }
      public class GDD<T> : GD<T> { }
      [GenerateClone] public class GenericClassHolder { public GB<int>? G { get; set; } }
      """,
    ["GenericSubclassOfAbstractGenericClass"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public abstract class AB<T> { public T? V { get; set; } }
      public class AD<T> : AB<T> { }
      public class AInt : AB<int> { }
      public class AList<T> : AB<List<T>> { }
      public abstract record Result<T>;
      public sealed record Ok<T>(T Value) : Result<T>;
      public sealed record Err<T>(string Message) : Result<T>;
      [GenerateClone] public class AbstractGenericHolder
      {
        public AB<int>? A { get; set; }
        public AB<List<string>>? L { get; set; }
        public Result<List<string>>? R { get; set; }
      }
      """,
    ["GenericImplementationOfGenericInterface"] =
      """
      using TimeWarp.State;
      public interface IR<T> { T V { get; } }
      public class IntR : IR<int> { public int V { get; set; } }
      public class GR<T> : IR<T> { public T V { get; set; } = default!; }
      public struct SR<T> : IR<T> { public T V { get; set; } }
      [GenerateClone] public class GenericInterfaceHolder { public IR<int>? R { get; set; } }
      """
  };
}
