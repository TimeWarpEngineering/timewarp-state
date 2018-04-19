namespace BlazorState.Behaviors.History
{
  using System;
  using System.Collections.Generic;
  using MediatR;

  public class History<TState>
  {
    public History()
    {
      Entries = new List<Entry>();
    }

    public List<Entry> Entries { get; }

    public class Entry
    {
      public Entry(TState state, IRequest<TState> request = default(IRequest<TState>))
      {
        State = state;
        Request = request;
        Time = DateTime.UtcNow;
      }

      public IRequest<TState> Request { get; }
      public TState State { get; }
      public DateTime Time { get; }
    }
  }
}