#region Purpose
// Later generated-clone cases: runtime types, stacks, constructors, and shared services.
#endregion

#region Design
// Partial of Should_ in the same test project. The fixtures live in generated-clone-shape-tests.cs.
// The generator sees every syntax tree in that compilation.
#endregion

namespace GeneratedCloneShapeTests;

public partial class Should_
{
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

  public void Cast_Inherited_ICloneable_Result_To_The_Declared_Type()
  {
    var original = new CloneableHolder
    {
      Cloneable = new CloneableDerived { V = 1, W = 2 },
      Base = new CloneableDerived { V = 3, W = 4 },
      Hidden = CloneableHolder.MakeHidden(5)
    };

    CloneableHolder clone = original.Clone()!;

    // CloneableBase.Clone() returns a CloneableBase even for the subclass; that is the user's Clone(), not a cast error.
    clone.Cloneable.ShouldBeOfType<CloneableBase>().V.ShouldBe(101);
    clone.Base.ShouldBeOfType<CloneableBase>().V.ShouldBe(103);
    clone.Hidden.ShouldNotBeNull().V.ShouldBe(5);
    clone.Hidden.ShouldNotBeSameAs(original.Hidden);
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

  public void Construct_With_Typed_Default_Arguments()
  {
    var original = new ConstructorShapes { Money = new Money(5m), Required = new RequiredHolder { Name = "n", Count = 2 } };

    ConstructorShapes clone = original.Clone()!;

    clone.Money!.Amount.ShouldBe(5m);
    clone.Required!.Name.ShouldBe("n");
    clone.Required.Count.ShouldBe(2);
  }

  public void Clone_State_Whose_Constructor_Takes_A_Service()
  {
    var original = new ClockState(new Clock()) { Ticks = 7 };

    var clone = (ClockState)StateCloneRegistry.Clone(original);

    clone.Ticks.ShouldBe(7);
    clone.HasClock.ShouldBeFalse();
  }

  public void Share_Injected_Services_By_Reference()
  {
    ILogger<ServiceState> logger = NullLogger<ServiceState>.Instance;
    using HttpClient httpClient = new();
    NavigationManager navigationManager = new TestNavigationManager();
    var original = new ServiceState(logger, httpClient, navigationManager) { Count = 4 };

    var clone = (ServiceState)StateCloneRegistry.Clone(original);

    clone.ShouldNotBeSameAs(original);
    clone.Count.ShouldBe(4);
    clone.Logger.ShouldBeSameAs(logger);
    clone.HttpClient.ShouldBeSameAs(httpClient);
    clone.NavigationManager.ShouldBeSameAs(navigationManager);
    clone.Guid.ShouldNotBe(original.Guid);
    clone.Logger.LogInformation("clone kept the logger");
    clone.NavigationManager.BaseUri.ShouldBe("https://example.test/");
  }

}
