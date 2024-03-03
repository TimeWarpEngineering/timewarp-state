#nullable enable
namespace TimeWarp.Features.Persistence;

public interface IPersistenceService
{
    Task<object?> LoadState(Type stateType, PersistentStateMethod persistentStateMethod);
}
