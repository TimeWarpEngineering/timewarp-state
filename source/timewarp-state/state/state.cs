#region Purpose
// Base state type: identity, disposal, and the test-only access guard.
#endregion

#region Design
// Test-only members call ThrowIfNotTestAssembly.
// StateTestOptions.Enable() is the only opt-in. The assembly-name sniff is gone: a name that contains
// "test" (Contoso.Latest, Acme.Testimonials) is not enough.
#endregion

namespace TimeWarp.State;

public abstract class State<TState> : IState<TState>, IDisposable
where TState : State<TState>
{
  [IgnoreDataMember]
  private CancellationTokenSource CancellationTokenSource { get; } = new();
  protected CancellationToken CancellationToken => CancellationTokenSource.Token;
  protected bool IsDisposed;

  #region JsonIgnore

  // JsonIgnore keeps Sender out of JSON serialization and out of the state deep clone.

  [JsonIgnore]
  public ISender<ClientPipeline> Sender { get; set; } = null!;

  #endregion

  #region IgnoreDataMember

  // IgnoreDataMember keeps properties out of the state deep clone (TimeWarp.Features.Cloning)
  // They change on every instance creation and are not needed for cloning

  [IgnoreDataMember]
  public Guid Guid { get; protected init; } = Guid.NewGuid();

  #endregion

  /// <summary>
  /// DI Constructor
  /// </summary>
  /// <param name="sender"></param>
  protected State(ISender<ClientPipeline> sender)
  {
    Sender = sender;
  }

  [JsonConstructor]
  protected State() {}

  /// <summary>
  /// Throws when the caller is not allowed to use a test-only member.
  /// </summary>
  /// <param name="assembly">
  /// Caller assembly. Recorded only so the exception can name it. The name is not a pass.
  /// </param>
  /// <exception cref="FieldAccessException">
  /// The process has not called <see cref="StateTestOptions.Enable"/>.
  /// </exception>
  protected void ThrowIfNotTestAssembly(Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(assembly);

    if (StateTestOptions.AllowTestAccess)
    {
      return;
    }

    throw new FieldAccessException
    (
      $"Do not use this in production. Call {nameof(StateTestOptions)}.{nameof(StateTestOptions.Enable)}() from the test host. Caller: {assembly.FullName}"
    );
  }

  /// <summary>
  /// Override this to Set the initial state
  /// </summary>
  public abstract void Initialize();

  public void CancelOperations()
  {
    if (!CancellationTokenSource.IsCancellationRequested)
    {
      CancellationTokenSource.Cancel();
    }
  }

  protected virtual void Dispose(bool disposing)
  {
    if (IsDisposed) return;
    if (disposing)
    {
      CancelOperations();
      CancellationTokenSource.Dispose();
    }

    IsDisposed = true;
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }
}
