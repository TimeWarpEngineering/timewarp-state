#region Purpose
// Fixtures for the null and default member clone tests, plus the immutable, frozen, concurrent, record, and private-constructor shapes.
#endregion

#region Design
// Split from generated-clone-shape-fixtures.cs for the line cap. Item, Box, and the other base fixtures live there.
// Every holder is [GenerateClone] so the emitted extension Clone runs in the tests.
#endregion

namespace GeneratedCloneShapeTests;

public partial class Should_
{
  [GenerateClone]
  [NotTest]
  public sealed class NullShapes
  {
    public Item? Single { get; set; }
    public Box? NullBox { get; set; }
    public List<Item>? NullList { get; set; }
    public ImmutableArray<Item> EmptyArray { get; set; } = ImmutableArray<Item>.Empty;
    public ImmutableArray<Item> DefaultArray { get; set; }
    public (string? Name, Item? It) DefaultTuple { get; set; }
    public (int Count, int Total) DefaultValueTuple { get; set; }
  }

  [GenerateClone]
  [NotTest]
  public sealed class ImmutableAndFrozenShapes
  {
    public ImmutableQueue<Item> Queue { get; set; } = ImmutableQueue<Item>.Empty;
    public ImmutableHashSet<Item> Set { get; set; } = ImmutableHashSet<Item>.Empty;
    public ImmutableHashSet<int> Numbers { get; set; } = ImmutableHashSet<int>.Empty;
    public FrozenSet<Item> Frozen { get; set; } = FrozenSet<Item>.Empty;
    public FrozenSet<int> FrozenNumbers { get; set; } = FrozenSet<int>.Empty;
    public FrozenDictionary<string, Item> FrozenMap { get; set; } = FrozenDictionary<string, Item>.Empty;
  }

  [GenerateClone]
  [NotTest]
  public sealed class ConcurrentShapes
  {
    public ConcurrentDictionary<string, Item> Map { get; set; } = new();
  }

  [NotTest]
  public class TaggedList : List<Item>
  {
    public string Tag { get; set; } = "";
  }

  [GenerateClone]
  [NotTest]
  public sealed class SubclassHolder
  {
    public TaggedList Tagged { get; set; } = new();
  }

  [NotTest]
  public record PersonRecord(string Name, Item Tag)
  {
    public int Age { get; init; }
  }

  public readonly record struct Point(int X, Item Tag);

  [GenerateClone]
  [NotTest]
  public sealed class RecordHolder
  {
    public PersonRecord? Person { get; set; }
    public Point Point { get; set; }
  }

  [NotTest]
  public class Hidden
  {
    private Hidden() { }

    public static Hidden Create(int n) => new() { N = n };

    public int N { get; set; }
  }

  [GenerateClone]
  [NotTest]
  public sealed class HiddenHolder
  {
    public Hidden? Hidden { get; set; }
  }

  [GenerateClone]
  [NotTest]
  public sealed class FieldSharedHolder
  {
    [field: CloneShared]
    public Clock? Shared { get; set; }

    public Item? Own { get; set; }
  }
}
