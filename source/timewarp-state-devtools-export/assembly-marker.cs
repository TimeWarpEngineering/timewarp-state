#region Purpose
// Marker type for the dev-only DevTools exporter assembly.
#endregion

#region Design
// No MediatorAssembly and no IState. Referencing this package must not register inspector states.
#endregion

namespace TimeWarp.State.DevTools.Export;

/// <summary>
/// Marker type for the TimeWarp.State.DevTools.Export assembly.
/// </summary>
public sealed class AssemblyMarker;
