#region Purpose
// Runs the generated clone for collection and hierarchy shapes: BCL values behind collection interfaces, polymorphic members,
// generic subtypes, ImmutableStack order, sorted and linked collections, StringBuilder, immutable and frozen sets and queues,
// ConcurrentDictionary, and collection subclasses.
#endregion

#region Design
// Fixtures are in generated-clone-shape-fixtures.cs and generated-clone-extra-fixtures.cs. Where a collection is mutable the test
// mutates the clone and checks the original is untouched. Where it is immutable it checks element identity: the generator deep
// copies reference-type elements and shares value-type collections.
#endregion

namespace GeneratedCloneShapeTests;

public partial class Should_
{
  public void Clone_Bcl_Values_Behind_Collection_Interfaces()
  {
    var original = new InterfaceShapes
    {
      ImmutableItems = ImmutableList.Create(new Item { V = 1 }),
      ImmutableLookup = ImmutableDictionary<string, int>.Empty.Add("a", 1),
      Lazy = Enumerable.Range(1, 2).Select(index => new Item { V = index }),
      ReadOnly = new List<Item> { new() { V = 4 } }.AsReadOnly(),
      Sorted = new SortedSet<int> { 3, 1 },
      Array = new[] { new Item { V = 5 } }
    };

    InterfaceShapes clone = original.Clone()!;

    clone.ImmutableItems.Count.ShouldBe(1);
    clone.ImmutableItems.ShouldBeOfType<ImmutableList<Item>>();
    clone.ImmutableItems.First().ShouldNotBeSameAs(original.ImmutableItems.First());
    clone.ImmutableLookup.ShouldBeSameAs(original.ImmutableLookup);
    clone.Lazy.Select(item => item.V).ShouldBe([1, 2]);
    clone.Lazy.ShouldBeOfType<List<Item>>();
    clone.ReadOnly.ShouldBeOfType<System.Collections.ObjectModel.ReadOnlyCollection<Item>>();
    clone.ReadOnly[0].ShouldNotBeSameAs(original.ReadOnly[0]);
    clone.Sorted.ShouldBeOfType<SortedSet<int>>();
    clone.Sorted.ShouldBe([1, 3]);
    clone.Array.ShouldBeOfType<Item[]>();
    clone.Array.First().ShouldNotBeSameAs(original.Array.First());
  }

  public void Keep_Runtime_Type_Of_Polymorphic_Members()
  {
    var original = new Drawing
    {
      Single = new Circle { X = 1, Radius = 5 },
      All = [new Shape { X = 1 }, new Circle { X = 2, Radius = 3 }, new Ring { Radius = 4, Inner = 2 }]
    };

    Drawing clone = original.Clone()!;

    Circle circle = clone.Single.ShouldBeOfType<Circle>();
    circle.Radius.ShouldBe(5);
    circle.ShouldNotBeSameAs(original.Single);
    clone.All[0].ShouldBeOfType<Shape>();
    clone.All[1].ShouldBeOfType<Circle>().Radius.ShouldBe(3);
    clone.All[2].ShouldBeOfType<Ring>().Inner.ShouldBe(2);
  }

  public void Keep_Runtime_Type_Of_Generic_Subtypes_Of_Generic_Members()
  {
    var list = new List<Item> { new() { V = 1 } };
    var original = new GenericHierarchies
    {
      Class = new GenericDerived<int> { V = 1, W = 2 },
      Abstract = new AbstractDerived<int> { V = 3 },
      Result = new Ok<List<Item>>(list),
      Failure = new Err<int>("bad"),
      Interface = new GenericResult<int> { V = 5 }
    };

    GenericHierarchies clone = original.Clone()!;

    GenericDerived<int> derived = clone.Class.ShouldBeOfType<GenericDerived<int>>();
    derived.V.ShouldBe(1);
    derived.W.ShouldBe(2);
    derived.ShouldNotBeSameAs(original.Class);
    clone.Abstract.ShouldBeOfType<AbstractDerived<int>>().V.ShouldBe(3);
    Ok<List<Item>> ok = clone.Result.ShouldBeOfType<Ok<List<Item>>>();
    ok.Value.ShouldNotBeSameAs(list);
    ok.Value[0].V.ShouldBe(1);
    ok.Value[0].ShouldNotBeSameAs(list[0]);
    clone.Failure.ShouldBeOfType<Err<int>>().Message.ShouldBe("bad");
    clone.Interface.ShouldBeOfType<GenericResult<int>>().V.ShouldBe(5);
    clone.Interface.ShouldNotBeSameAs(original.Interface);
  }

  public void Keep_ImmutableStack_Order()
  {
    var original = new StackHolder
    {
      Stack = ImmutableStack<Item>.Empty.Push(new Item { V = 1 }).Push(new Item { V = 2 })
    };

    StackHolder clone = original.Clone()!;

    clone.Stack.Select(item => item.V).ShouldBe([2, 1]);
    clone.Stack.Peek().ShouldNotBeSameAs(original.Stack.Peek());
  }

  public void Clone_Sorted_Linked_And_StringBuilder_Members()
  {
    var original = new BclShapes();
    original.Sorted["b"] = 2;
    original.Sorted["a"] = 1;
    original.Linked.AddLast(new Item { V = 1 });
    original.Linked.AddLast(new Item { V = 2 });
    original.Builder.Append("hello");

    BclShapes clone = original.Clone()!;

    clone.Sorted.Keys.ShouldBe(["a", "b"]);
    clone.Linked.Select(item => item.V).ShouldBe([1, 2]);
    clone.Linked.First!.Value.ShouldNotBeSameAs(original.Linked.First!.Value);
    clone.Builder.ToString().ShouldBe("hello");
    clone.Builder.ShouldNotBeSameAs(original.Builder);
  }


  public void Clone_Immutable_Queue_And_HashSet_Elements()
  {
    Item first = new() { V = 1 };
    Item second = new() { V = 2 };
    ImmutableAndFrozenShapes original = new()
    {
      Queue = ImmutableQueue<Item>.Empty.Enqueue(first).Enqueue(second),
      Set = ImmutableHashSet<Item>.Empty.Add(first),
      Numbers = ImmutableHashSet<int>.Empty.Add(3).Add(4)
    };

    ImmutableAndFrozenShapes clone = original.Clone()!;

    clone.Queue.Select(item => item.V).ShouldBe([1, 2]);
    clone.Queue.Peek().ShouldNotBeSameAs(first);
    clone.Queue.ShouldNotContain(first);
    clone.Set.Count.ShouldBe(1);
    clone.Set.Single().V.ShouldBe(1);
    clone.Set.Single().ShouldNotBeSameAs(first);
    clone.Numbers.OrderBy(number => number).ShouldBe([3, 4]);

    // Immutable: changing the clone's collection builds a new one and leaves the original's alone.
    ImmutableQueue<Item> grown = clone.Queue.Enqueue(new Item { V = 3 });
    grown.Count().ShouldBe(3);
    original.Queue.Count().ShouldBe(2);
  }

  public void Clone_Frozen_Set_And_Dictionary_Elements()
  {
    Item first = new() { V = 1 };
    ImmutableAndFrozenShapes original = new()
    {
      Frozen = new[] { first }.ToFrozenSet(),
      FrozenNumbers = new[] { 5, 6 }.ToFrozenSet(),
      FrozenMap = new Dictionary<string, Item> { ["k"] = first }.ToFrozenDictionary()
    };

    ImmutableAndFrozenShapes clone = original.Clone()!;

    clone.Frozen.Count.ShouldBe(1);
    clone.Frozen.Single().V.ShouldBe(1);
    clone.Frozen.Single().ShouldNotBeSameAs(first);
    clone.FrozenNumbers.OrderBy(number => number).ShouldBe([5, 6]);
    clone.FrozenMap.Count.ShouldBe(1);
    clone.FrozenMap["k"].V.ShouldBe(1);
    clone.FrozenMap["k"].ShouldNotBeSameAs(first);
    first.V.ShouldBe(1);
  }

  public void Clone_Empty_Immutable_And_Frozen_Collections()
  {
    ImmutableAndFrozenShapes original = new();

    ImmutableAndFrozenShapes clone = original.Clone()!;

    clone.Queue.IsEmpty.ShouldBeTrue();
    clone.Set.IsEmpty.ShouldBeTrue();
    clone.Frozen.Count.ShouldBe(0);
    clone.FrozenMap.Count.ShouldBe(0);
  }

  public void Clone_ConcurrentDictionary_Without_Sharing_Entries()
  {
    ConcurrentShapes original = new();
    original.Map["a"] = new Item { V = 1 };
    original.Map["b"] = new Item { V = 2 };

    ConcurrentShapes clone = original.Clone()!;

    clone.Map.ShouldNotBeSameAs(original.Map);
    clone.Map.Count.ShouldBe(2);
    clone.Map["a"].V.ShouldBe(1);
    clone.Map["a"].ShouldNotBeSameAs(original.Map["a"]);
    clone.Map.TryAdd("c", new Item { V = 3 }).ShouldBeTrue();
    clone.Map["b"].V = 20;
    original.Map.ContainsKey("c").ShouldBeFalse();
    original.Map["b"].V.ShouldBe(2);
  }

  public void Clone_Collection_Subclass_Elements_Without_Sharing()
  {
    SubclassHolder original = new();
    original.Tagged.Tag = "tag";
    original.Tagged.Add(new Item { V = 1 });

    SubclassHolder clone = original.Clone()!;

    clone.Tagged.ShouldNotBeSameAs(original.Tagged);
    clone.Tagged.Count.ShouldBe(1);
    clone.Tagged[0].V.ShouldBe(1);
    clone.Tagged[0].ShouldNotBeSameAs(original.Tagged[0]);
    clone.Tagged.Add(new Item { V = 2 });
    original.Tagged.Count.ShouldBe(1);
    // The subclass's own property is copied too.
    clone.Tagged.Tag.ShouldBe("tag");
  }
}
