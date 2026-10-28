#region Purpose
// Stores the request entries parsed from a TimeWarp.Mediator 15 MediatorManifest JSON document.
#endregion

#region Design
// The generated MediatorManifest type is internal to the host assembly, so the library accepts the
// public Json const as text. Version 2 has kind, scope, and Behaviors in pipeline order, and it has
// no profile key. 15.0 does not register IPipelineBehavior<,> in the container, so this registry does
// not match service descriptors. Read replaces the snapshot so a test can load one document at a time.
// BehaviorNames walks requests in document order and keeps the first sighting of each simple name.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Request entries from a <c>MediatorManifest</c> version 2 JSON document.
/// </summary>
public static class PipelineBehaviorRegistry
{
  private static readonly Snapshot Empty = new([], []);

  private static Snapshot Current = Empty;

  /// <summary>
  /// One request row from <c>MediatorManifest</c>.
  /// </summary>
  /// <param name="Request">Request display name.</param>
  /// <param name="Kind">Manifest kind: Action, Command, Query, IdempotentCommand, or Request.</param>
  /// <param name="Scope">Scope display name, or null when the request is unscoped.</param>
  /// <param name="Behaviors">Closed behavior display names in pipeline order.</param>
  public readonly record struct ManifestRequest(
    string Request,
    string Kind,
    string? Scope,
    IReadOnlyList<string> Behaviors);

  /// <summary>
  /// Replaces the stored manifest with <paramref name="mediatorManifestJson"/>.
  /// </summary>
  /// <param name="mediatorManifestJson"><c>MediatorManifest.Json</c> from the host.</param>
  /// <exception cref="InvalidOperationException">The document is not version 2, or it still has a profile key.</exception>
  public static void Read(string mediatorManifestJson)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(mediatorManifestJson);
    Snapshot snapshot = Parse(mediatorManifestJson);
    Interlocked.Exchange(ref Current, snapshot);
  }

  /// <summary>
  /// Requests from the last <see cref="Read"/> call, in document order.
  /// </summary>
  public static IReadOnlyList<ManifestRequest> Requests => Volatile.Read(ref Current).Requests;

  /// <summary>
  /// Distinct behavior simple names in first-seen pipeline order.
  /// </summary>
  public static IReadOnlyList<string> BehaviorNames() => Volatile.Read(ref Current).BehaviorNames;

  internal static string SimpleName(string behaviorDisplay)
  {
    int generic = behaviorDisplay.IndexOf('<');
    string withoutGeneric = generic < 0 ? behaviorDisplay : behaviorDisplay[..generic];
    int lastDot = withoutGeneric.LastIndexOf('.');
    return lastDot < 0 ? withoutGeneric : withoutGeneric[(lastDot + 1)..];
  }

  private static Snapshot Parse(string mediatorManifestJson)
  {
    using var document = JsonDocument.Parse(mediatorManifestJson);
    JsonElement root = document.RootElement;
    if (root.ValueKind != JsonValueKind.Object)
    {
      throw new InvalidOperationException("MediatorManifest JSON must be an object.");
    }

    if (root.TryGetProperty("profile", out _))
    {
      throw new InvalidOperationException("MediatorManifest version 2 has no profile key.");
    }

    if (!root.TryGetProperty("version", out JsonElement versionElement)
      || versionElement.ValueKind != JsonValueKind.Number
      || versionElement.GetInt32() != 2)
    {
      throw new InvalidOperationException("MediatorManifest version must be 2.");
    }

    if (!root.TryGetProperty("requests", out JsonElement requestsElement)
      || requestsElement.ValueKind != JsonValueKind.Array)
    {
      throw new InvalidOperationException("MediatorManifest version 2 requires a requests array.");
    }

    List<ManifestRequest> requests = [];
    List<string> behaviorNames = [];
    HashSet<string> seen = new(StringComparer.Ordinal);
    foreach (JsonElement requestElement in requestsElement.EnumerateArray())
    {
      string request = RequiredString(requestElement, "request");
      string kind = RequiredString(requestElement, "kind");
      string? scope = requestElement.TryGetProperty("scope", out JsonElement scopeElement)
        && scopeElement.ValueKind == JsonValueKind.String
          ? scopeElement.GetString()
          : null;
      IReadOnlyList<string> behaviors = ReadBehaviors(requestElement);
      requests.Add(new ManifestRequest(request, kind, scope, behaviors));
      foreach (string behavior in behaviors)
      {
        string simpleName = SimpleName(behavior);
        if (seen.Add(simpleName))
        {
          behaviorNames.Add(simpleName);
        }
      }
    }

    return new Snapshot(requests, behaviorNames);
  }

  private static IReadOnlyList<string> ReadBehaviors(JsonElement requestElement)
  {
    if (!requestElement.TryGetProperty("behaviors", out JsonElement behaviorsElement))
    {
      return [];
    }

    if (behaviorsElement.ValueKind != JsonValueKind.Array)
    {
      throw new InvalidOperationException("MediatorManifest behaviors must be an array.");
    }

    List<string> behaviors = [];
    foreach (JsonElement behaviorElement in behaviorsElement.EnumerateArray())
    {
      if (behaviorElement.ValueKind != JsonValueKind.String)
      {
        throw new InvalidOperationException("MediatorManifest behavior names must be strings.");
      }

      string? behavior = behaviorElement.GetString();
      if (string.IsNullOrWhiteSpace(behavior))
      {
        throw new InvalidOperationException("MediatorManifest behavior names must be non-empty.");
      }

      behaviors.Add(behavior);
    }

    return behaviors;
  }

  private static string RequiredString(JsonElement element, string propertyName)
  {
    if (!element.TryGetProperty(propertyName, out JsonElement property)
      || property.ValueKind != JsonValueKind.String
      || string.IsNullOrWhiteSpace(property.GetString()))
    {
      throw new InvalidOperationException($"MediatorManifest request is missing {propertyName}.");
    }

    return property.GetString()!;
  }

  private sealed class Snapshot
  {
    public Snapshot(IReadOnlyList<ManifestRequest> requests, IReadOnlyList<string> behaviorNames)
    {
      Requests = requests;
      BehaviorNames = behaviorNames;
    }

    public IReadOnlyList<ManifestRequest> Requests { get; }

    public IReadOnlyList<string> BehaviorNames { get; }
  }
}
