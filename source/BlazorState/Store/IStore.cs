﻿using System;
using System.Collections.Generic;
using BlazorState.State;

namespace BlazorState.Store
{
  public interface IStore
  {
    Guid Guid { get; }

    IDictionary<string, object> GetSerializableState();

    TState GetState<TState>();

    object GetState(Type aType);

    void LoadStatesFromJson(string aJsonString);

    void SetState(IState aState);
  }
}