namespace BlazorState
{
  using System;
  using System.Reflection;
  using Microsoft.AspNetCore.Blazor;

  public abstract class State<TState> : IState<TState>
  {
    public State()
    {
      Initialize();
    }

    public Guid Guid { get; } = Guid.NewGuid();

    TState IState<TState>.State { get; }

    public abstract object Clone();

    public TState Hydrate(string aJsonString) => JsonUtil.Deserialize<TState>(aJsonString);

    protected abstract void Initialize();

    public void ThrowIfNotTestAssembly(Assembly aAssembly)
    {
      if (!aAssembly.FullName.Contains("Test"))
      {
        throw new System.FieldAccessException("Do not use this in production. This method is intended for Test access only!");
      }
    }
  }
}