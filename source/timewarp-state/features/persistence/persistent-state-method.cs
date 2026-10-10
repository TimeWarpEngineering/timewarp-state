#region Purpose
// The storage options for a [PersistentState] state: SessionStorage or LocalStorage.
// PreRender and Server were removed until a host implementation exists.
#endregion

#region Design
// Plain enum; no design decisions beyond naming the supported storage targets.
#endregion

namespace TimeWarp.Features.Persistence;

public enum PersistentStateMethod
{
  SessionStorage,
  LocalStorage
}
