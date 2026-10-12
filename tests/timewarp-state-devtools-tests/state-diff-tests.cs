#region Purpose
// Proves StateDiff reports add, remove, change, nested, and array edits, and the no-snapshot cases.
#endregion

#region Design
// Compares JSON strings. Truncated and unparsed payloads stay unparsed so the UI can show raw text.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class StateDiff_Should
{
  public static void ReportNoSnapshotWhenTheSelectedSpanHasNone()
  {
    StateDiff.Result result = StateDiff.Compare("{}", beforeTruncated: false, afterJson: null, afterTruncated: false, rootName: "CounterState");
    result.Status.ShouldBe(StateDiff.Status.NoSnapshot);
    result.Nodes.ShouldBeEmpty();
    result.Initial.ShouldBeFalse();
  }

  public static void KeepTruncationAndSkipTheTree()
  {
    const string after = """{"count":1}""";
    StateDiff.Result result = StateDiff.Compare("{}", beforeTruncated: false, after, afterTruncated: true, rootName: "CounterState");
    result.Status.ShouldBe(StateDiff.Status.Truncated);
    result.Raw.ShouldBe(after);
    result.Nodes.ShouldBeEmpty();

    StateDiff.Compare(after, beforeTruncated: true, """{"count":2}""", afterTruncated: false, rootName: "CounterState")
      .Status.ShouldBe(StateDiff.Status.Truncated);
  }

  public static void ReportUnparsedJsonOnEitherSide()
  {
    StateDiff.Compare("{}", false, "{", false, "CounterState").Status.ShouldBe(StateDiff.Status.Unparsed);
    StateDiff.Compare("{", false, "{}", false, "CounterState").Status.ShouldBe(StateDiff.Status.Unparsed);
  }

  public static void TreatAMissingPreviousSnapshotAsAnAddedTree()
  {
    StateDiff.Result result = StateDiff.Compare
    (
      beforeJson: null,
      beforeTruncated: false,
      afterJson: """{"count":1}""",
      afterTruncated: false,
      rootName: "CounterState"
    );

    result.Status.ShouldBe(StateDiff.Status.Changes);
    result.Initial.ShouldBeTrue();
    StateDiff.Node root = result.Nodes.ShouldHaveSingleItem();
    root.Kind.ShouldBe(StateDiff.ChangeKind.Added);
    StateDiff.Node count = root.Children.ShouldHaveSingleItem();
    count.Name.ShouldBe("count");
    count.Kind.ShouldBe(StateDiff.ChangeKind.Added);
    count.AfterText.ShouldBe("1");
    count.Path.ShouldBe("CounterState.count");
  }

  public static void ReportNoChangeWhenObjectsAreEqualIncludingKeyOrder()
  {
    StateDiff.Result result = StateDiff.Compare
    (
      """{"b":1,"a":2}""",
      false,
      """{"a":2,"b":1}""",
      false,
      "CounterState"
    );
    result.Status.ShouldBe(StateDiff.Status.NoChange);
    result.Nodes.ShouldBeEmpty();
  }

  public static void ReportAddedRemovedChangedNestedAndArrayEdits()
  {
    string before = """{"keep":1,"gone":2,"nested":{"x":1,"y":2},"list":[1,2]}""";
    string after = """{"keep":1,"added":3,"nested":{"x":9,"y":2},"list":[1,4,5]}""";
    StateDiff.Result result = StateDiff.Compare(before, false, after, false, "CounterState");

    result.Status.ShouldBe(StateDiff.Status.Changes);
    result.Initial.ShouldBeFalse();
    StateDiff.Node root = result.Nodes.ShouldHaveSingleItem();
    root.Kind.ShouldBe(StateDiff.ChangeKind.Nested);
    root.Children.Select(child => child.Name).ShouldBe(["added", "gone", "list", "nested"]);

    root.Children[0].Kind.ShouldBe(StateDiff.ChangeKind.Added);
    root.Children[0].AfterText.ShouldBe("3");
    root.Children[1].Kind.ShouldBe(StateDiff.ChangeKind.Removed);
    root.Children[1].BeforeText.ShouldBe("2");

    StateDiff.Node list = root.Children[2];
    list.Kind.ShouldBe(StateDiff.ChangeKind.Nested);
    list.Children.Select(child => child.Name).ShouldBe(["1", "2"]);
    list.Children[0].Kind.ShouldBe(StateDiff.ChangeKind.Changed);
    list.Children[0].BeforeText.ShouldBe("2");
    list.Children[0].AfterText.ShouldBe("4");
    list.Children[0].Path.ShouldBe("CounterState.list[1]");
    list.Children[1].Kind.ShouldBe(StateDiff.ChangeKind.Added);
    list.Children[1].AfterText.ShouldBe("5");
    list.Children[1].Path.ShouldBe("CounterState.list[2]");

    StateDiff.Node nested = root.Children[3];
    nested.Kind.ShouldBe(StateDiff.ChangeKind.Nested);
    StateDiff.Node changed = nested.Children.ShouldHaveSingleItem();
    changed.Name.ShouldBe("x");
    changed.Kind.ShouldBe(StateDiff.ChangeKind.Changed);
    changed.BeforeText.ShouldBe("1");
    changed.AfterText.ShouldBe("9");
    changed.Path.ShouldBe("CounterState.nested.x");
  }
}
