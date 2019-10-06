namespace TestApp.Client.Features.CloneTest
{
  using BlazorState;
  using System;

  internal partial class CloneTestState : State<CloneTestState>, ICloneable
  {

    public int Count { get; private set; }

    public CloneTestState() { }

    /// <summary>
    /// Set the Initial State
    /// </summary>
    public override void Initialize() => Count = 3;
    public object Clone() => new CloneTestState { Count = 42 };
  }
}