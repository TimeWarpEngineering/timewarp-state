namespace TestApp.Client.Features.Counter
{
  using BlazorState;

  internal partial class CounterState : State<CounterState>
  {

    public int Count { get; private set; }

    public CounterState() { }

    /// <summary>
    /// Set the Initial State
    /// </summary>
    protected override void Initialize() => Count = 3;
  }
}