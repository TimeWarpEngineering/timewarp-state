namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Runtime.Serialization;

  public abstract class State<TState> : IState<TState>
  {
    public State()
    {
      Initialize();
    }

    [IgnoreDataMember]
    public Guid Guid { get; protected set; } = Guid.NewGuid();

    TState IState<TState>.State { get; }

    /// <summary>
    /// returns a new instance of type TState
    /// </summary>
    /// <param name="aKeyValuePairs">Initialize the TState instance with these values</param>
    /// <returns>The particular State of type TState</returns>
    /// <remarks>Implement this if you want to use ReduxDevTools Time Travel</remarks>
    public virtual TState Hydrate(IDictionary<string, object> aKeyValuePairs) => throw new NotImplementedException();

    /// <summary>
    /// Use this method to prevent running methods from source other than Tests
    /// </summary>
    /// <param name="aAssembly"></param>
    public void ThrowIfNotTestAssembly(Assembly aAssembly)
    {
      if (!aAssembly.FullName.Contains("Test"))
      {
        throw new FieldAccessException("Do not use this in production. This method is intended for Test access only!");
      }
    }

    /// <summary>
    /// Override this to Set the initial state
    /// </summary>
    protected abstract void Initialize();

  }
}