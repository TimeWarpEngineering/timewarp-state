using System;
using Microsoft.AspNetCore.Blazor;

namespace BlazorState.State
{
  public abstract class State<TState> : IState<TState>
  {
    public State()
    {
      Initialize();
    }

    public TState Hydrate(string aJsonString)
    {
      TState result = JsonUtil.Deserialize<TState>(aJsonString);
      return result;
    }

    public Guid Guid { get; } = Guid.NewGuid();
    TState IState<TState>.State { get; }

    public abstract object Clone();

    protected abstract void Initialize();
  }
}