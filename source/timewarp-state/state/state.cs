#region Purpose
// Base state type: identity, hydration, disposal, and the test-only access guard.
#endregion

#region Design
// Test-only members call ThrowIfNotTestAssembly.
// StateTestOptions.Enable() is the supported opt-in. A test host calls it and the guard allows the call.
// Assembly-name sniffing (ordinal ignore-case substring "test") is a fallback shipped in 12.0.0-beta.5.
// The next release drops the sniff; hosts call StateTestOptions.Enable().
// Sniffing matches by name convention. The flag does not.
// Kebab-case names such as web-spa-integration-tests match the fallback, so an AssemblyName override
// that only capitalizes "Test" can be removed.
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

  // JsonIgnore is used to prevent serialization of the property by both AnyClone and ReduxDevTools 

  [JsonIgnore]
  public ISender<ClientPipeline> Sender { get; set; } = null!;

  #endregion

  #region IgnoreDataMember

  // IgnoreDataMember is used to prevent serialization of properties by AnyClone
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
  /// returns a new instance of type TState
  /// </summary>
  /// <param name="keyValuePairs">Initialize the TState instance with these values</param>
  /// <returns>The particular State of type TState</returns>
  /// <remarks>Implement this if you want to use ReduxDevTools Time Travel</remarks>
  public virtual TState Hydrate(IDictionary<string, object> keyValuePairs) => throw new NotImplementedException();

  /// <summary>
  /// Throws when the caller is not allowed to use a test-only member.
  /// </summary>
  /// <param name="assembly">
  /// Assembly classified when <see cref="StateTestOptions.AllowTestAccess"/> is false.
  /// </param>
  /// <exception cref="FieldAccessException">
  /// The process has not called <see cref="StateTestOptions.Enable"/> and <paramref name="assembly"/>
  /// does not contain "test" in its full name (ordinal, ignore case).
  /// </exception>
  protected void ThrowIfNotTestAssembly(Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(assembly);

    if (StateTestOptions.AllowTestAccess)
    {
      return;
    }

    string? fullName = assembly.FullName;
    ArgumentNullException.ThrowIfNull(fullName);

    if (!fullName.Contains("test", StringComparison.OrdinalIgnoreCase))
    {
      throw new FieldAccessException("Do not use this in production. This method is intended for Test access only!");
    }
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
