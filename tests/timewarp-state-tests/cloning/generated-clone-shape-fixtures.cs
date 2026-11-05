#region Purpose
// Fixture types for the generated clone shape tests: generic, nested, tuple, collection, polymorphic, record, constructor, and service shapes.
#endregion

#region Design
// Each fixture is [GenerateClone] (or a State<T> registered by the module initializer) and nested in Should_ so the test
// files that share them name them without public test types. The generator sees every syntax tree in the compilation, so the
// concern files (generics, collections, constructors) can use any fixture here. ServiceState is shared with the pipeline tests
// and lives in shared/service-state-fixtures.cs.
#endregion

namespace GeneratedCloneShapeTests;

public partial class Should_
{
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
}
