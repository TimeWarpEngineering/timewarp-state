#region Purpose
// Marker type for locating the Test.App.Client assembly.
#endregion

#region Design
// Program passes its assembly to AddTimeWarpState and AddActionCatalog, and the server passes it to
// AddAdditionalAssemblies so client pages are routable. Sealed and empty on purpose.
#endregion

namespace Test.App.Client;

/// <summary>
/// Serves as a marker for the assembly, facilitating easy identification and reflection-based operations.
/// </summary>
/// <remarks>
/// This class is intended to be used as a reference point within the assembly for scenarios such as assembly scanning,
/// where a stable, known type is required to locate the assembly at runtime. The class is sealed to indicate it is not
/// designed for inheritance or extension, reinforcing its role as a simple marker.
/// </remarks>
public sealed class AssemblyMarker { }
