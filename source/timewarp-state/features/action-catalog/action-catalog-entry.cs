#region Purpose
// Runtime descriptor of one [CatalogAction] action, emitted by the source generator.
#endregion

#region Design
// Execute is a generated static lambda that resolves the state from the store and calls the same
// generated State.Method(...) the ActionSet method generator emits: no reflection, AOT and trim safe.
// Arguments are positional in constructor order; trailing optional arguments may be omitted.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Describes one cataloged action and executes it through the store.
/// </summary>
public sealed class ActionCatalogEntry
{
  private readonly Func<IStore, object?[], CancellationToken, Task> Executor;

  /// <summary>
  /// Creates an entry. Called by generated code.
  /// </summary>
  public ActionCatalogEntry
  (
    string name,
    string description,
    IReadOnlyList<string> permissions,
    ActionVisibility visibility,
    Type stateType,
    Type actionType,
    IReadOnlyList<ActionCatalogParameter> parameters,
    string inputSchema,
    Func<IStore, object?[], CancellationToken, Task> executor
  )
  {
    Name = name;
    Description = description;
    Permissions = permissions;
    Visibility = visibility;
    StateType = stateType;
    ActionType = actionType;
    Parameters = parameters;
    InputSchema = inputSchema;
    Executor = executor;
  }

  /// <summary>Unique catalog name, for example <c>Credentials.AddPasskey</c>.</summary>
  public string Name { get; }

  /// <summary>One plain sentence describing the action.</summary>
  public string Description { get; }

  /// <summary>Consumer-defined permission ids. The consumer enforces them.</summary>
  public IReadOnlyList<string> Permissions { get; }

  /// <summary>Who the action is offered to.</summary>
  public ActionVisibility Visibility { get; }

  /// <summary>The state that owns the action.</summary>
  public Type StateType { get; }

  /// <summary>The nested <c>Action</c> type.</summary>
  public Type ActionType { get; }

  /// <summary>Constructor parameters in order.</summary>
  public IReadOnlyList<ActionCatalogParameter> Parameters { get; }

  /// <summary>
  /// JSON schema object describing <see cref="Parameters"/>. Complex parameter types carry their CLR type name only.
  /// </summary>
  public string InputSchema { get; }

  /// <summary>
  /// Sends the action through the store's state. Does not check <see cref="Permissions"/>.
  /// </summary>
  /// <param name="store">Store that resolves the owning state.</param>
  /// <param name="arguments">Positional arguments in constructor order; trailing optional arguments may be omitted.</param>
  /// <param name="cancellationToken">Cancels the send.</param>
  public Task Execute(IStore store, object?[]? arguments = null, CancellationToken cancellationToken = default)
  {
    ArgumentNullException.ThrowIfNull(store);
    return Executor(store, arguments ?? [], cancellationToken);
  }

  /// <inheritdoc />
  public override string ToString() => Name;
}
