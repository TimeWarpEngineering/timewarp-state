#region Purpose
// Proves TimeWarp.Features.Cloning deep-copies state graphs (AnyClone replacement) with the rules
// StateTransactionBehavior relies on: ignored members keep constructor values, graphs are independent.
#endregion

#region Design
// Server-side unit coverage. The browser (single-threaded WASM) proof is the CloneTestPage and CounterPage E2E
// tests, which run the same cloner in a real browser on net11.
#endregion

namespace DeepClonerTests;

public class Should_
{
  public void Copy_Public_And_Private_Fields()
  {
    var original = new Sample(privateValue: 7) { Name = "a", Numbers = [1, 2, 3] };

    Sample clone = original.Clone();

    clone.ShouldNotBeSameAs(original);
    clone.Name.ShouldBe("a");
    clone.PrivateValue.ShouldBe(7);
    clone.Numbers.ShouldNotBeSameAs(original.Numbers);
    clone.Numbers.ShouldBe([1, 2, 3]);
  }

  public void Skip_Ignored_Members_And_Keep_Constructor_Values()
  {
    var original = new Sample(privateValue: 1) { Ignored = "original", JsonIgnored = "original" };

    Sample clone = original.Clone();

    clone.Id.ShouldNotBe(original.Id);
    clone.Id.ShouldNotBe(Guid.Empty);
    clone.Ignored.ShouldBe("default");
    clone.JsonIgnored.ShouldBe("default");
  }

  public void Produce_Independent_Nested_Collections()
  {
    var original = new Sample(privateValue: 0)
    {
      Children = [new Child { Value = 1 }, new Child { Value = 2 }],
      Lookup = new Dictionary<string, Child> { ["x"] = new() { Value = 10 } }
    };

    Sample clone = original.Clone();
    clone.Children[0].Value = 99;
    clone.Lookup["x"].Value = 99;
    clone.Lookup["y"] = new Child { Value = 3 };

    original.Children[0].Value.ShouldBe(1);
    original.Lookup["x"].Value.ShouldBe(10);
    original.Lookup.ContainsKey("y").ShouldBeFalse();
    clone.Lookup["x"].Value.ShouldBe(99);
  }

  public void Preserve_Shared_References_And_Cycles()
  {
    var shared = new Child { Value = 5 };
    var original = new Sample(privateValue: 0) { Children = [shared, shared] };
    original.Self = original;

    Sample clone = original.Clone();

    clone.Self.ShouldBeSameAs(clone);
    clone.Children[0].ShouldBeSameAs(clone.Children[1]);
    clone.Children[0].ShouldNotBeSameAs(shared);
  }

  public void Copy_Multi_Dimensional_Arrays()
  {
    ArrayHolder original = new()
    {
      Numbers = new[,] { { 1, 2 }, { 3, 4 } },
      Children = new[,] { { new Child { Value = 1 } }, { new Child { Value = 2 } } }
    };

    ArrayHolder clone = original.Clone();

    clone.Numbers.ShouldNotBeSameAs(original.Numbers);
    clone.Numbers[1, 1].ShouldBe(4);
    clone.Children[1, 0].ShouldNotBeSameAs(original.Children[1, 0]);
    clone.Children[1, 0].Value.ShouldBe(2);
  }

  public void Deep_Copy_Reference_Fields_Inside_Structs()
  {
    var original = new Holder { Pair = new Pair { Child = new Child { Value = 1 }, Count = 2 } };

    Holder clone = original.Clone();
    clone.Pair.Child.Value = 50;

    original.Pair.Child.Value.ShouldBe(1);
    clone.Pair.Count.ShouldBe(2);
  }

  public void Share_Delegates_And_Types()
  {
    Action action = () => { };
    var original = new Sample(privateValue: 0) { Callback = action, Kind = typeof(string) };

    Sample clone = original.Clone();

    clone.Callback.ShouldBeSameAs(action);
    clone.Kind.ShouldBeSameAs(typeof(string));
  }

  public void Return_Null_For_Null()
  {
    Sample? original = null;
    original.Clone().ShouldBeNull();
  }

  [GenerateClone]
  [NotTest]
  public sealed class Sample
  {
    private readonly int PrivateValueField;

    public Sample() : this(0) { }

    public Sample(int privateValue)
    {
      PrivateValueField = privateValue;
    }

    public int PrivateValue => PrivateValueField;
    public string? Name { get; set; }
    public int[] Numbers { get; set; } = [];
    public List<Child> Children { get; set; } = [];
    public Dictionary<string, Child> Lookup { get; set; } = new();
    public Sample? Self { get; set; }
    public Action? Callback { get; set; }
    public Type? Kind { get; set; }

    [System.Runtime.Serialization.IgnoreDataMember]
    public Guid Id { get; } = Guid.NewGuid();

    [System.Runtime.Serialization.IgnoreDataMember]
    public string Ignored { get; set; } = "default";

    [System.Text.Json.Serialization.JsonIgnore]
    public string JsonIgnored { get; set; } = "default";
  }

  [GenerateClone]
  [NotTest]
  public sealed class Child
  {
    public int Value { get; set; }
  }

  [GenerateClone]
  public struct Pair
  {
    public Child Child;
    public int Count;
  }

  [GenerateClone]
  [NotTest]
  public sealed class Holder
  {
    public Pair Pair;
  }

  [GenerateClone]
  [NotTest]
  public sealed class ArrayHolder
  {
    public int[,] Numbers { get; set; } = new int[0, 0];
    public Child[,] Children { get; set; } = new Child[0, 0];
  }
}
