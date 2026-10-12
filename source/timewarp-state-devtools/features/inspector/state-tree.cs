#region Purpose
// Builds a collapsible JSON tree from a snapshot string.
#endregion

#region Design
// System.Text.Json.Nodes only: no reflection and no reference to TimeWarp.State.Telemetry.
// A truncated payload is not parsed; the caller shows the raw text and the truncated marker.
// Object keys are ordered with StringComparer.Ordinal so the tree is stable across renders.
#endregion

namespace TimeWarp.State.DevTools.Features.Inspector;

/// <summary>
/// JSON tree model for one snapshot.
/// </summary>
public static class StateTree
{
  /// <summary>
  /// Parses <paramref name="json"/> into a tree rooted at <paramref name="rootName"/>.
  /// </summary>
  /// <param name="json">Snapshot text. Null is treated as empty.</param>
  /// <param name="truncated">When true, the payload is returned unparsed.</param>
  /// <param name="rootName">Display name and path of the root node.</param>
  public static Document Parse(string? json, bool truncated, string rootName)
  {
    ArgumentException.ThrowIfNullOrEmpty(rootName);
    string raw = json ?? "";
    if (truncated || raw.Length == 0)
    {
      return new Document(parsed: false, truncated: truncated, raw: raw, root: null);
    }

    try
    {
      JsonNode? node = JsonNode.Parse(raw);
      return new Document(parsed: true, truncated: false, raw: raw, root: Build(rootName, rootName, node));
    }
    catch (JsonException)
    {
      return new Document(parsed: false, truncated: false, raw: raw, root: null);
    }
  }

  internal static (string Text, ValueKind Kind) Describe(JsonNode? node)
  {
    if (node is null)
    {
      return ("null", ValueKind.Null);
    }

    if (node is not JsonValue)
    {
      return (node.ToJsonString(), ValueKind.String);
    }

    string raw = node.ToJsonString();
    if (raw == "null")
    {
      return ("null", ValueKind.Null);
    }

    if (raw == "true" || raw == "false")
    {
      return (raw, ValueKind.Boolean);
    }

    if (raw.Length >= 2 && raw[0] == '"')
    {
      try
      {
        return (node.GetValue<string>(), ValueKind.String);
      }
      catch (InvalidOperationException)
      {
        return (raw, ValueKind.String);
      }
    }

    return (raw, ValueKind.Number);
  }

  private static Node Build(string name, string path, JsonNode? node)
  {
    if (node is JsonObject jsonObject)
    {
      return new Branch
      {
        Name = name,
        Path = path,
        Array = false,
        Children = ObjectChildren(path, jsonObject)
      };
    }

    if (node is JsonArray jsonArray)
    {
      return new Branch
      {
        Name = name,
        Path = path,
        Array = true,
        Children = ArrayChildren(path, jsonArray)
      };
    }

    (string text, ValueKind kind) = Describe(node);
    return new Scalar
    {
      Name = name,
      Path = path,
      Text = text,
      Kind = kind
    };
  }

  private static List<Node> ObjectChildren(string path, JsonObject jsonObject)
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
      children.Add(Build(key, path + "." + key, jsonObject[key]));
    }

    return children;
  }

  private static List<Node> ArrayChildren(string path, JsonArray jsonArray)
  {
    List<Node> children = new(jsonArray.Count);
    for (int index = 0; index < jsonArray.Count; index++)
    {
      string label = index.ToString(CultureInfo.InvariantCulture);
      children.Add(Build(label, path + "[" + label + "]", jsonArray[index]));
    }

    return children;
  }

  /// <summary>Parsed snapshot, or the raw text when parsing was refused.</summary>
  public sealed class Document
  {
    /// <summary>Creates a document.</summary>
    public Document(bool parsed, bool truncated, string raw, Node? root)
    {
      Parsed = parsed;
      Truncated = truncated;
      Raw = raw;
      Root = root;
    }

    /// <summary>True when <see cref="Root"/> was built from JSON.</summary>
    public bool Parsed { get; }

    /// <summary>True when the caller flagged the payload as truncated.</summary>
    public bool Truncated { get; }

    /// <summary>Original snapshot text.</summary>
    public string Raw { get; }

    /// <summary>Tree root. Null when <see cref="Parsed"/> is false.</summary>
    public Node? Root { get; }
  }

  /// <summary>One node in a snapshot tree.</summary>
  public abstract class Node
  {
    /// <summary>Property name, array index, or state type name.</summary>
    public required string Name { get; init; }

    /// <summary>Stable expansion key. Roots are the state type name; children append <c>.name</c> or <c>[index]</c>.</summary>
    public required string Path { get; init; }
  }

  /// <summary>Object or array node.</summary>
  public sealed class Branch : Node
  {
    /// <summary>True when this node is a JSON array.</summary>
    public bool Array { get; init; }

    /// <summary>Child nodes. Object children are ordered by name.</summary>
    public IReadOnlyList<Node> Children { get; init; } = [];
  }

  /// <summary>JSON scalar.</summary>
  public sealed class Scalar : Node
  {
    /// <summary>Display text. Strings are unquoted.</summary>
    public required string Text { get; init; }

    /// <summary>Scalar kind used to colour the value.</summary>
    public required ValueKind Kind { get; init; }
  }

  /// <summary>Colour class for a scalar.</summary>
  public enum ValueKind
  {
    /// <summary>JSON string.</summary>
    String = 0,

    /// <summary>JSON number.</summary>
    Number = 1,

    /// <summary>JSON true or false.</summary>
    Boolean = 2,

    /// <summary>JSON null.</summary>
    Null = 3
  }
}
