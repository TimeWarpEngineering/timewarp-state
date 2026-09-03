using TimeWarp.Mediator;

// Member of the TimeWarp.Mediator compile-time graph so the host's generator links the handlers in
// this assembly (routing, timers, theme, action tracking, persistence). The Plus pipeline behaviors
// (ActiveActionBehavior, PersistentStatePostProcessor, MultiTimerPostProcessor) are opt-in: the host
// declares the ones it wants with [assembly: MediatorBehavior(typeof(...<,>), order: ...)].
[assembly: MediatorAssembly]

namespace TimeWarp.State.Plus;

/// <summary>
/// Serves as a marker for the assembly, facilitating easy identification and reflection-based operations.
/// </summary>
/// <remarks>
/// This class is intended to be used as a reference point within the assembly for scenarios such as assembly scanning,
/// where a stable, known type is required to locate the assembly at runtime. The class is sealed to indicate it is not
/// designed for inheritance or extension, reinforcing its role as a simple marker.
/// </remarks>
public sealed class AssemblyMarker;
