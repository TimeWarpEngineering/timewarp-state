#nullable enable
namespace BlazorState.Features.Persistence.Abstractions;

public interface IPersistenceService
{
    Task<object?> LoadStateAsync(Type stateType, PersistentStateMethod persistentStateMethod);
}
