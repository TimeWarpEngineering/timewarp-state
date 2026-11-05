#region Purpose
// Runs the generated clone for generic declaring types, types nested in generic types, generic state bases, tuples,
// KeyValuePair, Nullable structs, and null or default members that the clone must carry through unchanged.
#endregion

#region Design
// Fixtures are in generated-clone-shape-fixtures.cs and generated-clone-extra-fixtures.cs. These tests execute the emitted
// code, not a reflection path. Compile-only coverage of the same shapes is in the source generator tests.
#endregion

namespace GeneratedCloneShapeTests;

public partial class Should_
{
  public void Clone_Fields_Declared_On_Generic_Types()
  {
    var original = new UsesWrapper { W = new Wrapper<Item> { Value = new Item { V = 3 } } };

    UsesWrapper clone = original.Clone()!;

    clone.W.Value!.V.ShouldBe(3);
    clone.W.Value.ShouldNotBeSameAs(original.W.Value);
  }

  public void Clone_Fields_Declared_On_Types_Nested_In_Generic_Types()
  {
    var original = new UsesNested { Inner = new Outer<Item>.Inner { Value = new Item { V = 6 } } };
    original.Inner.Values.Add(new Item { V = 7 });

    UsesNested clone = original.Clone()!;

    clone.Inner!.Value!.V.ShouldBe(6);
    clone.Inner.Value.ShouldNotBeSameAs(original.Inner.Value);
    clone.Inner.Values[0].V.ShouldBe(7);
    clone.Inner.Values.ShouldNotBeSameAs(original.Inner.Values);
  }

  public void Clone_State_With_Generic_Base_Fields()
  {
    var original = new HistState { Count = 2 };
    original.History.Add(new HistState { Count = 1 });
    original.Previous = original;

    var clone = (HistState)StateCloneRegistry.Clone(original);

    clone.Count.ShouldBe(2);
    clone.History.Count.ShouldBe(1);
    clone.History[0].ShouldNotBeSameAs(original.History[0]);
    clone.Previous.ShouldBeSameAs(clone);
    clone.Guid.ShouldNotBe(original.Guid);
  }

  public void Clone_Tuples_KeyValuePairs_And_Nullable_Structs()
  {
    var original = new ValueShapes
    {
      Tuple = ("a", new Item { V = 1 }),
      Pairs = [new KeyValuePair<string, Item>("k", new Item { V = 2 })],
      Box = new Box { Values = [1, 2] }
    };

    ValueShapes clone = original.Clone()!;

    clone.Tuple.Name.ShouldBe("a");
    clone.Tuple.It.ShouldNotBeSameAs(original.Tuple.It);
    clone.Pairs[0].Value.ShouldNotBeSameAs(original.Pairs[0].Value);
    clone.Pairs[0].Value.V.ShouldBe(2);
    clone.Box!.Value.Values.ShouldNotBeSameAs(original.Box!.Value.Values);
    clone.Box.Value.Values.ShouldBe([1, 2]);
  }


  public void Preserve_Null_Reference_And_Nullable_Box_Members()
  {
    NullShapes original = new();
    original.Single.ShouldBeNull();
    original.NullBox.ShouldBeNull();
    original.NullList.ShouldBeNull();

    NullShapes clone = original.Clone()!;

    clone.ShouldNotBeSameAs(original);
    clone.Single.ShouldBeNull();
    clone.NullBox.ShouldBeNull();
    clone.NullList.ShouldBeNull();
  }

  public void Preserve_Empty_And_Default_ImmutableArray_Members()
  {
    NullShapes original = new();
    original.EmptyArray.IsEmpty.ShouldBeTrue();
    original.DefaultArray.IsDefault.ShouldBeTrue();

    NullShapes clone = original.Clone()!;

    clone.EmptyArray.IsDefault.ShouldBeFalse();
    clone.EmptyArray.IsEmpty.ShouldBeTrue();
    clone.DefaultArray.IsDefault.ShouldBeTrue();
  }

  public void Preserve_Default_Tuple_Members()
  {
    NullShapes original = new();

    NullShapes clone = original.Clone()!;

    clone.DefaultTuple.Name.ShouldBeNull();
    clone.DefaultTuple.It.ShouldBeNull();
    clone.DefaultValueTuple.ShouldBe(default);
  }

  public void Clone_Members_That_Are_Set_Beside_Null_Members()
  {
    NullShapes original = new() { Single = new Item { V = 9 }, NullBox = new Box { Values = [4] } };

    NullShapes clone = original.Clone()!;

    clone.Single!.V.ShouldBe(9);
    clone.Single.ShouldNotBeSameAs(original.Single);
    clone.NullBox!.Value.Values.ShouldBe([4]);
    clone.NullBox.Value.Values.ShouldNotBeSameAs(original.NullBox.Value.Values);
    clone.NullList.ShouldBeNull();
  }
}
