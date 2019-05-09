namespace TestApp.Client.Features.Counter
{
  internal partial class CounterState
  {

    public int Count { get; private set; }

    public CounterState() { }

    /// <summary>
    /// Set the Initial State
    /// </summary>
    protected override void Initialize() => Count = 3;
  }
}