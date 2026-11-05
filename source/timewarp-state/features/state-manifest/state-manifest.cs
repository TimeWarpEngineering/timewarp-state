#region Purpose
// Public reader for the generated State manifest: the states an assembly declares, and their actions, known up front
// (before any action runs), for DevTools, startup checks and tooling.
#endregion

#region Design
// The document is the JSON the State generator's ManifestEmitter writes (Version 1). Parsing uses JsonDocument, not a
// serializer, so it needs no reflection and no JsonSerializerContext and stays AOT and trim safe.
// FromAssembly reads the StateManifestAttribute instance. An assembly without states has none and returns null.
// Version handling: a document with a higher version than CurrentVersion still parses the fields this reader knows.
// Unknown fields are ignored, so the generator can add facts without breaking older readers.
// Type names are runtime Type.FullName strings (nested types joined with '+'). Name is Type.Name, the value
// TimeWarp.State.Telemetry puts in timewarp.state.state_type. An action's Name is the timewarp.state.action value.
#endregion

namespace TimeWarp.State;

/// <summary>
/// The TimeWarp.State manifest of one assembly: every state it declares and the actions nested in each state.
/// </summary>
public sealed class StateManifest
{
  /// <summary>The manifest format this reader was written for.</summary>
  public const int CurrentVersion = 1;

  private StateManifest(int version, string assemblyName, IReadOnlyList<StateManifestState> states, string json)
  {
    Version = version;
    AssemblyName = assemblyName;
    States = states;
    Json = json;
  }

  /// <summary>Manifest format version.</summary>
  public int Version { get; }

  /// <summary>Name of the assembly the manifest describes.</summary>
  public string AssemblyName { get; }

  /// <summary>States declared in the assembly, ordered by type name.</summary>
  public IReadOnlyList<StateManifestState> States { get; }

  /// <summary>The source document.</summary>
  public string Json { get; }

  /// <summary>Reads the generated manifest of <paramref name="assembly"/>, or null when it declares no state.</summary>
  /// <param name="assembly">An assembly compiled with TimeWarp.State's source generator.</param>
  public static StateManifest? FromAssembly(Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(assembly);
    StateManifestAttribute? attribute = assembly.GetCustomAttribute<StateManifestAttribute>();
    return attribute is null ? null : Parse(attribute.Json);
  }

  /// <summary>Parses a manifest document.</summary>
  /// <param name="json">The JSON from <see cref="StateManifestAttribute.Json"/>.</param>
  /// <exception cref="JsonException">The document is not valid JSON.</exception>
  /// <exception cref="FormatException">The document is not a State manifest.</exception>
  public static StateManifest Parse(string json)
  {
    ArgumentNullException.ThrowIfNull(json);
    using JsonDocument document = JsonDocument.Parse(json);
    JsonElement root = document.RootElement;
    if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty("version", out JsonElement version) || version.ValueKind != JsonValueKind.Number)
    {
      throw new FormatException("A State manifest is a JSON object with a numeric \"version\".");
    }

    List<StateManifestState> states = [];
    foreach (JsonElement state in Array(root, "states"))
    {
      List<StateManifestAction> actions = [];
      foreach (JsonElement action in Array(state, "actions"))
      {
        List<StateManifestParameter> parameters = [];
        foreach (JsonElement parameter in Array(action, "parameters"))
        {
          parameters.Add
          (
            new StateManifestParameter
            (
              String(parameter, "name") ?? "",
              String(parameter, "type") ?? "",
              String(parameter, "default"),
              Boolean(parameter, "required")
            )
          );
        }

        actions.Add
        (
          new StateManifestAction
          (
            String(action, "type") ?? "",
            String(action, "name") ?? "",
            String(action, "method"),
            parameters,
            String(action, "catalog"),
            Boolean(action, "tracked")
          )
        );
      }

      states.Add
      (
        new StateManifestState
        (
          String(state, "type") ?? "",
          String(state, "name") ?? "",
          String(state, "persistent"),
          String(state, "clone") ?? "",
          actions
        )
      );
    }

    return new StateManifest(version.GetInt32(), String(root, "assembly") ?? "", states, json);
  }

  private static IEnumerable<JsonElement> Array(JsonElement element, string name) =>
    element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.Array
      ? value.EnumerateArray().ToArray()
      : [];

  private static string? String(JsonElement element, string name) =>
    element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

  private static bool Boolean(JsonElement element, string name) =>
    element.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.True;
}

/// <summary>A state in a <see cref="StateManifest"/>.</summary>
/// <param name="Type">Runtime <c>Type.FullName</c> of the state.</param>
/// <param name="Name"><c>Type.Name</c> of the state, the <c>timewarp.state.state_type</c> span tag value.</param>
/// <param name="Persistent"><c>SessionStorage</c> or <c>LocalStorage</c> for a <c>[PersistentState]</c> state, otherwise null.</param>
/// <param name="CloneKind"><c>generated</c> when the source generator clones the state, <c>cloneable</c> when it implements ICloneable.</param>
/// <param name="Actions">Actions nested in the state's action sets, ordered by type name.</param>
public sealed record StateManifestState
(
  string Type,
  string Name,
  string? Persistent,
  string CloneKind,
  IReadOnlyList<StateManifestAction> Actions
);

/// <summary>An action in a <see cref="StateManifestState"/>.</summary>
/// <param name="Type">Runtime <c>Type.FullName</c> of the action.</param>
/// <param name="Name">Nested name below the state, for example <c>AddActionSet.Action</c>: the <c>timewarp.state.action</c> span tag value.</param>
/// <param name="Method">The state method that sends the action, generated or hand-written, or null when there is none.</param>
/// <param name="Parameters">The method's parameters, in order.</param>
/// <param name="CatalogName">The action catalog name when the action is <c>[CatalogAction]</c>, otherwise null.</param>
/// <param name="Tracked">True when the action is <c>[TrackAction]</c>.</param>
public sealed record StateManifestAction
(
  string Type,
  string Name,
  string? Method,
  IReadOnlyList<StateManifestParameter> Parameters,
  string? CatalogName,
  bool Tracked
);

/// <summary>A parameter of a <see cref="StateManifestAction"/> method.</summary>
/// <param name="Name">Parameter name, as the action catalog uses it.</param>
/// <param name="Type">CLR type name, for example <c>int</c> or <c>System.Guid</c>.</param>
/// <param name="Default">Default value source text, or null when the parameter is required.</param>
/// <param name="Required">True when the caller must supply a value.</param>
public sealed record StateManifestParameter
(
  string Name,
  string Type,
  string? Default,
  bool Required
);
