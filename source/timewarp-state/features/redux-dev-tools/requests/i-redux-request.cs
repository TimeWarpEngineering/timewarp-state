#region Purpose
// Marker interface for requests that originate from Redux DevTools.
#endregion

#region Design
// Internal, empty marker; no design decisions beyond letting DevTools requests be filtered by type.
#endregion

namespace TimeWarp.Features.ReduxDevTools;

/// <summary>
/// Marker Interface to allow for filtering of Devtools Requests
/// </summary>
internal interface IReduxRequest { }
