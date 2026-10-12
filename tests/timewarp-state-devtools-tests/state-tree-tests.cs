#region Purpose
// Proves StateTree turns snapshot JSON into a stable collapsible model and refuses truncated text.
#endregion

#region Design
// Calls StateTree.Parse directly. No component host: the DevTools tests stay on Fixie.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class StateTree_Should
{
  public static void BuildAnObjectTreeWithOrderedKeysTypedScalarsAndArrayPaths()
  {
    StateTree.Document document = StateTree.Parse
    (
      """{"z":1,"a":"hi","b":true,"c":null,"items":[2]}""",
      truncated: false,
      rootName: "CounterState"
    );

    document.Parsed.ShouldBeTrue();
    document.Truncated.ShouldBeFalse();
    StateTree.Branch root = document.Root.ShouldBeOfType<StateTree.Branch>();
    root.Path.ShouldBe("CounterState");
    root.Array.ShouldBeFalse();
    root.Children.Select(child => child.Name).ShouldBe(["a", "b", "c", "items", "z"]);

    StateTree.Scalar text = root.Children[0].ShouldBeOfType<StateTree.Scalar>();
    text.Text.ShouldBe("hi");
    text.Kind.ShouldBe(StateTree.ValueKind.String);
    text.Path.ShouldBe("CounterState.a");

    root.Children[1].ShouldBeOfType<StateTree.Scalar>().Kind.ShouldBe(StateTree.ValueKind.Boolean);
    root.Children[2].ShouldBeOfType<StateTree.Scalar>().Kind.ShouldBe(StateTree.ValueKind.Null);
    root.Children[4].ShouldBeOfType<StateTree.Scalar>().Kind.ShouldBe(StateTree.ValueKind.Number);

    StateTree.Branch items = root.Children[3].ShouldBeOfType<StateTree.Branch>();
    items.Array.ShouldBeTrue();
    items.Path.ShouldBe("CounterState.items");
    StateTree.Scalar item = items.Children.ShouldHaveSingleItem().ShouldBeOfType<StateTree.Scalar>();
    item.Path.ShouldBe("CounterState.items[0]");
    item.Text.ShouldBe("2");
  }

  public static void ParseAJsonNullRootAsANullScalar()
  {
    StateTree.Document document = StateTree.Parse("null", truncated: false, rootName: "CounterState");
    document.Parsed.ShouldBeTrue();
    StateTree.Scalar scalar = document.Root.ShouldBeOfType<StateTree.Scalar>();
    scalar.Kind.ShouldBe(StateTree.ValueKind.Null);
    scalar.Text.ShouldBe("null");
  }

  public static void KeepTheTruncationFlagAndRefuseToParse()
  {
    const string json = """{"count":1}""";
    StateTree.Document document = StateTree.Parse(json, truncated: true, rootName: "CounterState");
    document.Parsed.ShouldBeFalse();
    document.Truncated.ShouldBeTrue();
    document.Root.ShouldBeNull();
    document.Raw.ShouldBe(json);
  }

  public static void RefuseEmptyAndInvalidJson()
  {
    StateTree.Parse(null, truncated: false, rootName: "CounterState").Parsed.ShouldBeFalse();
    StateTree.Parse("", truncated: false, rootName: "CounterState").Parsed.ShouldBeFalse();
    StateTree.Document invalid = StateTree.Parse("{", truncated: false, rootName: "CounterState");
    invalid.Parsed.ShouldBeFalse();
    invalid.Truncated.ShouldBeFalse();
    invalid.Raw.ShouldBe("{");
  }
}
