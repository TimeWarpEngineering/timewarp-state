#region Purpose
// Looks up the clone delegate the source generator registered for a state type.
#endregion

#region Design
// Each compilation that contains a concrete State&lt;T&gt; emits a module initializer that calls Register.
// The dictionary is swapped with ImmutableInterlocked so registration does not take a lock. Lookup uses the
// runtime type, which matches typeof(TState) for the sealed states the generator registers. A miss throws:
// there is no reflection fallback.
#endregion

namespace TimeWarp.Features.Cloning;

/// <summary>
/// Generated state clone delegates, keyed by the state type.
/// </summary>
public static class StateCloneRegistry
{
  private static ImmutableDictionary<Type, Func<IState, IState>> Cloners =
    ImmutableDictionary<Type, Func<IState, IState>>.Empty;

  /// <summary>
  /// Registers the generated clone for <typeparamref name="TState"/>.
  /// </summary>
  public static void Register<TState>(Func<TState, TState> clone)
    where TState : class, IState
  {
    ArgumentNullException.ThrowIfNull(clone);
    Func<IState, IState> untyped = state => clone((TState)state);
    ImmutableInterlocked.Update
    (
      ref Cloners,
      static (dictionary, registration) => dictionary.SetItem(registration.Key, registration.Value),
      (Key: typeof(TState), Value: untyped)
    );
  }

  /// <summary>
  /// Clones <paramref name="state"/> with the delegate registered for its runtime type.
  /// </summary>
  /// <exception cref="InvalidOperationException">No generated clone is registered for the runtime type.</exception>
  public static IState Clone(IState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    Type type = state.GetType();
    ImmutableDictionary<Type, Func<IState, IState>> cloners = Cloners;
    if (cloners.TryGetValue(type, out Func<IState, IState>? clone))
    {
      return clone(state);
    }

    throw new InvalidOperationException
    (
      $"No generated clone is registered for '{type.FullName}'. " +
      "Implement ICloneable on that state, or fix TWSG002 so the clone source generator can emit one."
    );
  }
}
