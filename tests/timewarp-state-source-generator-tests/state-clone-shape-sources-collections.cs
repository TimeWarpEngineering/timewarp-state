#region Purpose
// Source text for the collection member shapes the clone-stage tests use.
#endregion

#region Design
// Collection member shapes: BCL interfaces, immutable and frozen collections, other BCL collections, and collection subclasses.
// Every key has one [Input] on the matching Given_Shape test; state-clone-shape-guard-tests.cs fails when they drift.
#endregion

namespace StateCloneSourceGenerator_;

internal static partial class StateCloneShapeSources
{
  public static readonly Dictionary<string, string> CollectionShapes = new()
  {
    ["CollectionInterfaces"] =
      $$"""
      using System.Collections.Generic;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Interfaces
      {
        public IList<string> A { get; set; } = new List<string>();
        public IReadOnlyList<string> B { get; set; } = new List<string>();
        public ISet<int> C { get; set; } = new HashSet<int>();
        public IReadOnlySet<int> D { get; set; } = new HashSet<int>();
        public IEnumerable<Item> E { get; set; } = new List<Item>();
        public ICollection<int> F { get; set; } = new List<int>();
        public IReadOnlyCollection<Item> G { get; set; } = new List<Item>();
        public IDictionary<string, Item> H { get; set; } = new Dictionary<string, Item>();
        public IReadOnlyDictionary<string, int> I { get; set; } = new Dictionary<string, int>();
      }
      """,
    ["ImmutableAndFrozen"] =
      $$"""
      using System.Collections.Frozen;
      using System.Collections.Immutable;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Immutables
      {
        public ImmutableList<Item> A { get; set; } = ImmutableList<Item>.Empty;
        public ImmutableStack<Item> B { get; set; } = ImmutableStack<Item>.Empty;
        public ImmutableQueue<Item> C { get; set; } = ImmutableQueue<Item>.Empty;
        public ImmutableHashSet<Item> D { get; set; } = ImmutableHashSet<Item>.Empty;
        public ImmutableSortedSet<Item> E { get; set; } = ImmutableSortedSet<Item>.Empty;
        public ImmutableArray<Item> F { get; set; }
        public ImmutableDictionary<string, Item> G { get; set; } = ImmutableDictionary<string, Item>.Empty;
        public ImmutableSortedDictionary<string, Item> H { get; set; } = ImmutableSortedDictionary<string, Item>.Empty;
        public FrozenSet<Item> I { get; set; } = FrozenSet<Item>.Empty;
        public FrozenDictionary<string, Item> J { get; set; } = FrozenDictionary<string, Item>.Empty;
        public ImmutableList<int> Shared { get; set; } = ImmutableList<int>.Empty;
      }
      """,
    ["OtherBclCollections"] =
      $$"""
      using System.Collections.Concurrent;
      using System.Collections.Generic;
      using System.Collections.ObjectModel;
      using System.Text;
      using TimeWarp.State;
      {{Item}}
      [GenerateClone] public class Others
      {
        public SortedDictionary<string, Item> A { get; set; } = new();
        public SortedList<string, int> B { get; set; } = new();
        public SortedSet<int> C { get; set; } = new();
        public LinkedList<Item> D { get; set; } = new();
        public ConcurrentDictionary<string, Item> E { get; set; } = new();
        public ReadOnlyCollection<Item>? F { get; set; }
        public ReadOnlyDictionary<string, Item>? G { get; set; }
        public StringBuilder H { get; set; } = new();
        public Stack<Item> I { get; set; } = new();
        public Queue<Item> J { get; set; } = new();
      }
      """,
    ["CollectionSubclass"] =
      $$"""
      using System.Collections.Generic;
      using TimeWarp.State;
      {{Item}}
      public class Tagged : List<Item> { public string Tag { get; set; } = ""; }
      public class TaggedMap : Dictionary<string, Item> { public int Version; }
      [GenerateClone] public class SubclassHolder { public Tagged T { get; set; } = new(); public TaggedMap M { get; set; } = new(); }
      """
  };
}
