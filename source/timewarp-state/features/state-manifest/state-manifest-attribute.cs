#region Purpose
// Assembly attribute the State generator applies with the assembly's State manifest JSON.
#endregion

#region Design
// Public and concrete, so two readers work. At runtime, StateManifest.FromAssembly reads the attribute instance, which
// needs no member reflection and stays AOT and trim safe. An analyzer in a consuming project reads the constructor
// arguments from metadata (AttributeData on the referenced assembly symbol), for example an architecture coverage
// check. The generator emits it in TimeWarp.State.StateManifest.g.cs, only when an assembly declares a state.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Carries an assembly's TimeWarp.State manifest: its states and their actions, as JSON. Applied by the source generator.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
public sealed class StateManifestAttribute : Attribute
{
  /// <summary>Creates the attribute. Called by generated code.</summary>
  /// <param name="version">Manifest format version.</param>
  /// <param name="json">The manifest document.</param>
  public StateManifestAttribute(int version, string json)
  {
    Version = version;
    Json = json;
  }

  /// <summary>Manifest format version. See <see cref="StateManifest.CurrentVersion"/>.</summary>
  public int Version { get; }

  /// <summary>The manifest document. Parse it with <see cref="StateManifest.Parse"/>.</summary>
  public string Json { get; }
}
