#region Purpose
// Thrown by StateTransactionBehavior when a state clone is unusable (its Guid is empty or equal to the original's).
#endregion

#region Design
// The cause distinguishes an empty Guid (initializer did not run) from a Guid copied off the original.
// The message names that cause and points at ICloneable plus the opt-out attributes the default cloner honors:
// IgnoreDataMember, NonSerialized and JsonIgnore.
#endregion

namespace TimeWarp.Features.StateTransactions;

public class InvalidCloneException : Exception
{
  /// <summary>
  /// Why <see cref="StateTransactionBehavior{TRequest, TResponse}"/> rejected the clone.
  /// </summary>
  public enum Cause
  {
    /// <summary>The clone's Guid is <see cref="Guid.Empty"/>.</summary>
    EmptyGuid,

    /// <summary>The clone's Guid equals the original state's Guid.</summary>
    EqualGuid
  }

  /// <summary>
  /// Creates an exception for a clone whose Guid is empty or equal to the original.
  /// </summary>
  /// <param name="enclosingStateType">State type that owned the action.</param>
  /// <param name="cause">Whether the clone Guid was empty or copied.</param>
  public InvalidCloneException(Type enclosingStateType, Cause cause)
    : base(CreateMessage(enclosingStateType, cause))
  {
    ArgumentNullException.ThrowIfNull(enclosingStateType);
    EnclosingStateType = enclosingStateType;
    CloneCause = cause;
  }

  /// <summary>State type that owned the action whose clone was rejected.</summary>
  public Type EnclosingStateType { get; }

  /// <summary>Whether the clone Guid was empty or equal to the original.</summary>
  public Cause CloneCause { get; }

  private static string CreateMessage(Type enclosingStateType, Cause cause) =>
    cause switch
    {
      Cause.EmptyGuid =>
        $"State of type {enclosingStateType} has an invalid clone: the clone has an empty Guid. " +
        "The state initializer did not run, for example an uninitialized instance or a custom ICloneable " +
        "that skips construction. Implement ICloneable.Clone so the clone is constructed, or use the default " +
        "cloner, which leaves members marked [IgnoreDataMember], [NonSerialized], or [JsonIgnore] at their " +
        "constructor values so Guid is regenerated.",
      Cause.EqualGuid =>
        $"State of type {enclosingStateType} has an invalid clone: the clone has an equal Guid to the original. " +
        "A custom ICloneable.Clone, such as MemberwiseClone, copied Guid, or [IgnoreDataMember] is missing from " +
        "Guid. Implement ICloneable.Clone so the clone gets a new Guid, or mark Guid with [IgnoreDataMember]. " +
        "The default cloner also skips [NonSerialized] and [JsonIgnore].",
      _ => throw new ArgumentOutOfRangeException(nameof(cause), cause, null)
    };
}
