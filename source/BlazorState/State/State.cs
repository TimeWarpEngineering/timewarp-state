namespace BlazorState
{
  using System;
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
  }
}