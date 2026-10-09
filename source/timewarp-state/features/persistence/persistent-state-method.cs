#region Purpose
// The storage options for a [PersistentState] state: PreRender, Server, SessionStorage or LocalStorage.
#endregion

#region Design
// Plain enum; no design decisions beyond naming the supported storage targets.
#endregion

namespace TimeWarp.Features.Persistence;

public enum PersistentStateMethod
{
  PreRender,
  Server,
  SessionStorage,
  LocalStorage
}
