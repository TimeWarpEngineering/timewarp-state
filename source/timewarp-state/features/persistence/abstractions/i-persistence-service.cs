#region Purpose
// Abstraction for loading a persisted state by type and persistence method.
#endregion

#region Design
// Defined in the core package so the pipeline can depend on it. Implementations live elsewhere, for example
// TimeWarp.State.Plus browser storage.
#endregion

namespace TimeWarp.Features.Persistence;

public interface IPersistenceService
{
    Task<object?> LoadState(Type stateType, PersistentStateMethod persistentStateMethod);
}
