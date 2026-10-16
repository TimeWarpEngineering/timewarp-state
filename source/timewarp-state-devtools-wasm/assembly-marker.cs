#region Purpose
// Marker type for the browser DevTools exporter assembly.
#endregion

#region Design
// No MediatorAssembly and no IState. Referencing this package must not register inspector states.
#endregion

namespace TimeWarp.State.DevTools.Wasm;

/// <summary>
/// Marker type for the TimeWarp.State.DevTools.Wasm assembly.
/// </summary>
public sealed class AssemblyMarker;
