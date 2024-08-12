namespace TimeWarp.State;

public abstract class State<TState> : IState<TState>, IDisposable
where TState : State<TState>
{
  private CancellationTokenSource CancellationTokenSource { get; } = new();
  protected CancellationToken CancellationToken => CancellationTokenSource.Token;
  protected bool IsDisposed;

  #region JsonIgnore

  // JsonIgnore is used to prevent serialization of the property by both AnyClone and ReduxDevTools 

  [JsonIgnore]
  public ISender Sender { get; set; } = null!;

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
  protected State(ISender sender)
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
  /// Use this method to prevent running methods from source other than Tests
  /// </summary>
  /// <param name="assembly"></param>
  protected void ThrowIfNotTestAssembly(Assembly assembly)
  {
    ArgumentNullException.ThrowIfNull(assembly);
    ArgumentNullException.ThrowIfNull(assembly.FullName);

    if (!assembly.FullName.Contains("Test"))
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
