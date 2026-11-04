#region Purpose
// Runs the generated clone for member shapes the first generator round got wrong: generic declaring types, tuples,
// KeyValuePair, Nullable structs, BCL values behind collection interfaces, polymorphic members, ImmutableStack order,
// sorted and linked collections, StringBuilder, overloaded and dependency constructors, required members, ICloneable
// members and subtypes, generic subtypes of generic members, and [CloneShared] injected services.
#endregion

#region Design
// Each fixture is [GenerateClone] (or a State<T> registered by the module initializer), so these tests execute the
// emitted code, not a reflection path. Compile-only coverage of the same shapes is in the source generator tests.
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

  [GenerateClone]
  [NotTest]
  public sealed class Item
  {
    public int V { get; set; }
  }

  [NotTest]
  public class Wrapper<T>
  {
    public T? Value { get; set; }
  }

  [GenerateClone]
  [NotTest]
  public sealed class UsesWrapper
  {
    public Wrapper<Item> W { get; set; } = new();
  }

  [NotTest]
  public class Outer<T>
  {
    [NotTest]
    public sealed class Inner
    {
      public T? Value;
      public List<T> Values = [];
    }
  }

  [GenerateClone]
  [NotTest]
  public sealed class UsesNested
  {
    public Outer<Item>.Inner? Inner { get; set; }
  }

  [NotTest]
  public abstract class ListState<TState> : State<TState>
    where TState : ListState<TState>
  {
    public List<TState> History { get; set; } = [];
    public TState? Previous { get; set; }
  }

  [NotTest]
  public sealed class HistState : ListState<HistState>
  {
    public int Count { get; set; }
    public override void Initialize() { }
  }

  public struct Box
  {
    public List<int> Values;
  }

  [GenerateClone]
  [NotTest]
  public sealed class ValueShapes
  {
    public (string Name, Item It) Tuple { get; set; }
    public List<KeyValuePair<string, Item>> Pairs { get; set; } = [];
    public Box? Box { get; set; }
  }

  [GenerateClone]
  [NotTest]
  public sealed class InterfaceShapes
  {
    public IReadOnlyCollection<Item> ImmutableItems { get; set; } = [];
    public IReadOnlyDictionary<string, int> ImmutableLookup { get; set; } = new Dictionary<string, int>();
    public IEnumerable<Item> Lazy { get; set; } = [];
    public IReadOnlyList<Item> ReadOnly { get; set; } = [];
    public ISet<int> Sorted { get; set; } = new HashSet<int>();
    public IEnumerable<Item> Array { get; set; } = [];
  }

  [NotTest]
  public class Shape
  {
    public int X { get; set; }
  }

  [NotTest]
  public class Circle : Shape
  {
    public int Radius { get; set; }
  }

  [NotTest]
  public sealed class Ring : Circle
  {
    public int Inner { get; set; }
  }

  [GenerateClone]
  [NotTest]
  public sealed class Drawing
  {
    public Shape? Single { get; set; }
    public List<Shape> All { get; set; } = [];
  }

  [NotTest]
  public class GenericBase<T>
  {
    public T? V { get; set; }
  }

  [NotTest]
  public class GenericDerived<T> : GenericBase<T>
  {
    public int W { get; set; }
  }

  [NotTest]
  public abstract class AbstractBase<T>
  {
    public T? V { get; set; }
  }

  [NotTest]
  public class AbstractDerived<T> : AbstractBase<T>;

  [NotTest]
  public abstract record Result<T>;

  [NotTest]
  public sealed record Ok<T>(T Value) : Result<T>;

  [NotTest]
  public sealed record Err<T>(string Message) : Result<T>;

  public interface IResult<T>
  {
    T V { get; }
  }

  [NotTest]
  public class GenericResult<T> : IResult<T>
  {
    public T V { get; set; } = default!;
  }

  // Generic subtypes of generic members: the generator closes each subtype over the member's type arguments.
  [GenerateClone]
  [NotTest]
  public sealed class GenericHierarchies
  {
    public GenericBase<int>? Class { get; set; }
    public AbstractBase<int>? Abstract { get; set; }
    public Result<List<Item>>? Result { get; set; }
    public Result<int>? Failure { get; set; }
    public IResult<int>? Interface { get; set; }
  }

  [NotTest]
  public class PlainBase
  {
    public int V { get; set; }
  }

  [NotTest]
  public class CloneableBase : PlainBase, ICloneable
  {
    public object Clone() => new CloneableBase { V = V + 100 };
  }

  [NotTest]
  public sealed class CloneableDerived : CloneableBase
  {
    public int W { get; set; }
  }

  // A private subtype is cloned through its ICloneable case; without ICloneable it would be TWSG002.
  [GenerateClone]
  [NotTest]
  public sealed class CloneableHolder
  {
    public CloneableBase? Cloneable { get; set; }
    public PlainBase? Base { get; set; }
    public PlainBase? Hidden { get; set; }

    public static PlainBase MakeHidden(int value) => new HiddenPlain { V = value };

    private sealed class HiddenPlain : PlainBase, ICloneable
    {
      public object Clone() => new HiddenPlain { V = V };
    }
  }

  [GenerateClone]
  [NotTest]
  public sealed class StackHolder
  {
    public ImmutableStack<Item> Stack { get; set; } = ImmutableStack<Item>.Empty;
  }

  [GenerateClone]
  [NotTest]
  public sealed class BclShapes
  {
    public SortedDictionary<string, int> Sorted { get; set; } = new();
    public LinkedList<Item> Linked { get; set; } = new();
    public System.Text.StringBuilder Builder { get; set; } = new();
  }

  [NotTest]
  public sealed class Money
  {
    public Money(decimal amount) { Amount = amount; }
    public Money(string text) { Amount = decimal.Parse(text, System.Globalization.CultureInfo.InvariantCulture); }
    public decimal Amount { get; private set; }
  }

  [NotTest]
  public class RequiredBase
  {
    public required string Name { get; set; }
  }

  [NotTest]
  public sealed class RequiredHolder : RequiredBase
  {
    public required int Count;
  }

  [GenerateClone]
  [NotTest]
  public sealed class ConstructorShapes
  {
    public Money? Money { get; set; }
    public RequiredHolder? Required { get; set; }
  }

  [NotTest]
  public sealed class Clock;

  // A state constructed with a service: the clone receives default arguments, so the constructor must accept null,
  // and the ignored service field keeps that null. [CloneShared] is the opt-in that keeps the instance; see ServiceState.
  [NotTest]
  public sealed class ClockState : State<ClockState>
  {
    [System.Runtime.Serialization.IgnoreDataMember]
    private readonly Clock? ClockService;

    public ClockState(Clock? clock)
    {
      ClockService = clock;
    }

    public int Ticks { get; set; }
    public bool HasClock => ClockService is not null;
    public override void Initialize() { }
  }

  [NotTest]
  public sealed class ServiceState : State<ServiceState>
  {
    [CloneShared]
    public ILogger<ServiceState> Logger { get; }

    [CloneShared]
    public HttpClient HttpClient { get; }

    [CloneShared]
    public NavigationManager NavigationManager { get; }

    public int Count { get; set; }

    public ServiceState(ILogger<ServiceState>? logger, HttpClient? httpClient, NavigationManager? navigationManager)
    {
      Logger = logger!;
      HttpClient = httpClient!;
      NavigationManager = navigationManager!;
    }

    public override void Initialize() { }
  }

  [NotTest]
  private sealed class TestNavigationManager : NavigationManager
  {
    public TestNavigationManager()
    {
      Initialize("https://example.test/", "https://example.test/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad) { }
  }
}
