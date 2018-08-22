namespace BlazorState
{
  using System;
  using System.Reflection;
  using Microsoft.JSInterop;

  public abstract class State<TState> : IState<TState>
  {
    public State()
    {
      Initialize();
    }

    public Guid Guid { get; } = Guid.NewGuid();

    TState IState<TState>.State { get; }

    public abstract object Clone();

    public TState Hydrate(string aJsonString) => Json.Deserialize<TState>(aJsonString);

    public void ThrowIfNotTestAssembly(Assembly aAssembly)
    {
      if (!aAssembly.FullName.Contains("Test"))
      {
        throw new FieldAccessException("Do not use this in production. This method is intended for Test access only!");
      }
    }

    protected abstract void Initialize();
  }
}