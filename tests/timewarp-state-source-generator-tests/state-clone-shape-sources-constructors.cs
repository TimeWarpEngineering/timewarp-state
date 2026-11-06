#region Purpose
// Source text for the constructor, record, and polymorphism member shapes the clone-stage tests use.
#endregion

#region Design
// Constructor, record, and polymorphism member shapes: polymorphic members, ambiguous, private, and required constructors, records, name collisions, and dependency-injected state constructors.
// Every key has one [Input] on the matching Given_Shape test; state-clone-shape-guard-tests.cs fails when they drift.
#endregion

namespace StateCloneSourceGenerator_;

internal static partial class StateCloneShapeSources
{
  public static readonly Dictionary<string, string> ConstructorShapes = new()
  {
    ["Polymorphic"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      public class Shape { public int X { get; set; } }
      public class Circle : Shape { public int Radius { get; set; } }
      public sealed class Ring : Circle { public int Inner { get; set; } }
      [GenerateClone] public class Drawing { public Shape? S { get; set; } public List<Shape> All { get; set; } = new(); }
      """,
    ["AmbiguousConstructor"] =
      """
      using TimeWarp.State;
      public class Money { public Money(decimal amount) { } public Money(string text) { } }
      public class Defaults { public Defaults(int count = 3, string name = "x", System.DayOfWeek day = System.DayOfWeek.Friday) { } }
      [GenerateClone] public class AmbiguousHolder { public Money? M { get; set; } public Defaults? D { get; set; } }
      """,
    ["PrivateConstructor"] =
      """
      using TimeWarp.State;
      public class Hidden { private Hidden() { } public static Hidden Create() => new(); public int N { get; set; } }
      [GenerateClone] public class HiddenHolder { public Hidden? H { get; set; } }
      """,
    ["RequiredMembers"] =
      """
      using TimeWarp.State;
      public class RequiredBase { public required string Name { get; set; } }
      public class RequiredField { public required int Count; }
      [GenerateClone] public class RequiredHolder : RequiredBase { public RequiredField? F { get; set; } }
      """,
    ["Records"] =
      $$"""
      using TimeWarp.State;
      {{Item}}
      public record PersonRecord(string Name, Item Tag) { public int Age { get; init; } }
      public readonly record struct Point(int X, Item Tag);
      [GenerateClone] public class RecordHolder { public PersonRecord? P { get; set; } public Point Q { get; set; } }
      """,
    ["NameCollision"] =
      """
      using System.Collections.Generic;
      using TimeWarp.State;
      namespace N
      {
        public class Common { public List<int> Shared = new(); }
        public class A { [GenerateClone] public class B : Common { } }
        [GenerateClone] public class A_B : Common { }
      }
      """,
    ["DependencyConstructorState"] =
      """
      using TimeWarp.State;
      public interface IClock { }
      public sealed class ClockState : State<ClockState>
      {
        [System.Runtime.Serialization.IgnoreDataMember]
        private readonly IClock? Clock;
        public ClockState(IClock? clock) { Clock = clock; }
        public int Ticks { get; set; }
        public override void Initialize() { }
      }
      """
  };
}
