#region Purpose
// Structural diff of two state snapshots.
#endregion

#region Design
// Walks JsonNode trees. Objects match by property name, arrays by index.
// Equal subtrees are omitted. A truncated side is not parsed.
// Missing previous JSON is an initial snapshot: every node is added.
// No reflection. Paths match StateTree so expansion state can be shared.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// Diff of one state's snapshot against the previous snapshot of that state.
/// </summary>
public static class StateDiff
{
  /// <summary>
  /// Compares <paramref name="beforeJson"/> to <paramref name="afterJson"/>.
  /// </summary>
  /// <param name="beforeJson">Previous snapshot. Null or empty means there is no earlier snapshot.</param>
  /// <param name="beforeTruncated">True when the previous payload was truncated.</param>
  /// <param name="afterJson">Selected snapshot. Null or empty means the span had none.</param>
  /// <param name="afterTruncated">True when the selected payload was truncated.</param>
  /// <param name="rootName">State type name used as the root path.</param>
  public static Result Compare
  (
    string? beforeJson,
    bool beforeTruncated,
    string? afterJson,
    bool afterTruncated,
    string rootName
  )
  {
    ArgumentException.ThrowIfNullOrEmpty(rootName);
    if (string.IsNullOrEmpty(afterJson))
    {
      return new Result(Status.NoSnapshot, [], "", initial: false);
    }

    if (beforeTruncated || afterTruncated)
    {
      return new Result(Status.Truncated, [], afterJson, initial: false);
    }

    JsonNode? afterNode;
    try
    {
      afterNode = JsonNode.Parse(afterJson);
    }
    catch (JsonException)
    {
      return new Result(Status.Unparsed, [], afterJson, initial: false);
    }

    if (string.IsNullOrEmpty(beforeJson))
    {
      Node added = Added(rootName, rootName, afterNode);
      return new Result(Status.Changes, [added], afterJson, initial: true);
    }

    JsonNode? beforeNode;
    try
    {
      beforeNode = JsonNode.Parse(beforeJson);
    }
    catch (JsonException)
    {
      return new Result(Status.Unparsed, [], afterJson, initial: false);
    }

    Node? diff = DiffPresent(rootName, rootName, beforeNode, afterNode);
    if (diff is null)
    {
      return new Result(Status.NoChange, [], afterJson, initial: false);
    }

    return new Result(Status.Changes, [diff], afterJson, initial: false);
  }

  private static Node? DiffPresent(string name, string path, JsonNode? before, JsonNode? after)
  {
    if (JsonNode.DeepEquals(before, after))
    {
      return null;
    }

    if (before is JsonObject beforeObject && after is JsonObject afterObject)
    {
      List<Node> children = DiffObjects(path, beforeObject, afterObject);
      if (children.Count == 0)
      {
        return null;
      }

      return Branch(name, path, ChangeKind.Nested, children);
    }

    if (before is JsonArray beforeArray && after is JsonArray afterArray)
    {
      List<Node> children = DiffArrays(path, beforeArray, afterArray);
      if (children.Count == 0)
      {
        return null;
      }

      return Branch(name, path, ChangeKind.Nested, children);
    }

    (string beforeText, StateTree.ValueKind beforeKind) = StateTree.Describe(before);
    (string afterText, StateTree.ValueKind afterKind) = StateTree.Describe(after);
    return new Node
    {
      Name = name,
      Path = path,
      Kind = ChangeKind.Changed,
      BeforeText = beforeText,
      AfterText = afterText,
      ValueKind = beforeKind == afterKind ? afterKind : null
    };
  }

  private static List<Node> DiffObjects(string path, JsonObject before, JsonObject after)
  {
    List<string> keys = [];
    foreach (KeyValuePair<string, JsonNode?> pair in before)
    {
      keys.Add(pair.Key);
    }

    foreach (KeyValuePair<string, JsonNode?> pair in after)
    {
      if (!before.ContainsKey(pair.Key))
      {
        keys.Add(pair.Key);
      }
    }

    keys.Sort(StringComparer.Ordinal);
    List<Node> children = [];
    foreach (string key in keys)
    {
      string childPath = path + "." + key;
      bool inBefore = before.ContainsKey(key);
      bool inAfter = after.ContainsKey(key);
      if (inBefore && !inAfter)
      {
        children.Add(Removed(key, childPath, before[key]));
        continue;
      }

      if (!inBefore && inAfter)
      {
        children.Add(Added(key, childPath, after[key]));
        continue;
      }

      Node? child = DiffPresent(key, childPath, before[key], after[key]);
      if (child is not null)
      {
        children.Add(child);
      }
    }

    return children;
  }

  private static List<Node> DiffArrays(string path, JsonArray before, JsonArray after)
  {
    int count = Math.Max(before.Count, after.Count);
    List<Node> children = [];
    for (int index = 0; index < count; index++)
    {
      string label = index.ToString(CultureInfo.InvariantCulture);
      string childPath = path + "[" + label + "]";
      if (index >= before.Count)
      {
        children.Add(Added(label, childPath, after[index]));
        continue;
      }

      if (index >= after.Count)
      {
        children.Add(Removed(label, childPath, before[index]));
        continue;
      }

      Node? child = DiffPresent(label, childPath, before[index], after[index]);
      if (child is not null)
      {
        children.Add(child);
      }
    }

    return children;
  }

  private static Node Added(string name, string path, JsonNode? node) =>
    Shape(name, path, ChangeKind.Added, node, removed: false);

  private static Node Removed(string name, string path, JsonNode? node) =>
    Shape(name, path, ChangeKind.Removed, node, removed: true);

  private static Node Shape(string name, string path, ChangeKind kind, JsonNode? node, bool removed)
  {
    if (node is JsonObject jsonObject)
    {
      List<string> keys = [];
      foreach (KeyValuePair<string, JsonNode?> pair in jsonObject)
      {
        keys.Add(pair.Key);
      }

      keys.Sort(StringComparer.Ordinal);
      List<Node> children = new(keys.Count);
      foreach (string key in keys)
      {
        children.Add(Shape(key, path + "." + key, kind, jsonObject[key], removed));
      }

      return Branch(name, path, kind, children);
    }

    if (node is JsonArray jsonArray)
    {
      List<Node> children = new(jsonArray.Count);
      for (int index = 0; index < jsonArray.Count; index++)
      {
        string label = index.ToString(CultureInfo.InvariantCulture);
        children.Add(Shape(label, path + "[" + label + "]", kind, jsonArray[index], removed));
      }

      return Branch(name, path, kind, children);
    }

    (string text, StateTree.ValueKind valueKind) = StateTree.Describe(node);
    return new Node
    {
      Name = name,
      Path = path,
      Kind = kind,
      BeforeText = removed ? text : null,
      AfterText = removed ? null : text,
      ValueKind = valueKind
    };
  }

  private static Node Branch(string name, string path, ChangeKind kind, List<Node> children) =>
    new()
    {
      Name = name,
      Path = path,
      Kind = kind,
      Children = children
    };

  /// <summary>Outcome of <see cref="Compare"/>.</summary>
  public sealed class Result
  {
    /// <summary>Creates a result.</summary>
    public Result(Status status, IReadOnlyList<Node> nodes, string raw, bool initial)
    {
      Status = status;
      Nodes = nodes;
      Raw = raw;
      Initial = initial;
    }

    /// <summary>How the two snapshots compared.</summary>
    public Status Status { get; }

    /// <summary>Changed nodes. Empty unless <see cref="Status"/> is <see cref="StateDiff.Status.Changes"/>.</summary>
    public IReadOnlyList<Node> Nodes { get; }

    /// <summary>Selected snapshot text, or empty when there was none.</summary>
    public string Raw { get; }

    /// <summary>True when there was no earlier snapshot and the selected JSON is entirely added.</summary>
    public bool Initial { get; }
  }

  /// <summary>Comparison outcome.</summary>
  public enum Status
  {
    /// <summary>The selected span had no snapshot.</summary>
    NoSnapshot = 0,

    /// <summary>A side was truncated, so the tree was not built.</summary>
    Truncated = 1,

    /// <summary>The snapshots are structurally equal.</summary>
    NoChange = 2,

    /// <summary><see cref="Result.Nodes"/> lists the structural changes.</summary>
    Changes = 3,

    /// <summary>A side was not valid JSON.</summary>
    Unparsed = 4
  }

  /// <summary>One changed node.</summary>
  public sealed class Node
  {
    /// <summary>Property name or array index.</summary>
    public required string Name { get; init; }

    /// <summary>Expansion path. Same shape as <see cref="StateTree.Node.Path"/>.</summary>
    public required string Path { get; init; }

    /// <summary>How this node differs.</summary>
    public required ChangeKind Kind { get; init; }

    /// <summary>Previous scalar text. Set for removed and changed scalars.</summary>
    public string? BeforeText { get; init; }

    /// <summary>New scalar text. Set for added and changed scalars.</summary>
    public string? AfterText { get; init; }

    /// <summary>Scalar colour. Null when the two sides have different kinds.</summary>
    public StateTree.ValueKind? ValueKind { get; init; }

    /// <summary>Nested changes. Empty for a scalar.</summary>
    public IReadOnlyList<Node> Children { get; init; } = [];
  }

  /// <summary>Kind of structural change.</summary>
  public enum ChangeKind
  {
    /// <summary>Present only in the selected snapshot.</summary>
    Added = 0,

    /// <summary>Present only in the previous snapshot.</summary>
    Removed = 1,

    /// <summary>Scalar or type change at this node.</summary>
    Changed = 2,

    /// <summary>Container whose children differ. The container itself is not added or removed.</summary>
    Nested = 3
  }
}
