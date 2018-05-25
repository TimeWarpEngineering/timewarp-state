namespace BlazorState.Client.State
{
  using BlazorState;

  public class CounterState : State<CounterState>
  {
    public CounterState() { }

    protected CounterState(CounterState aState) : this()
    {
      Count = aState.Count;
    }

    public int Count { get; set; }

    public override object Clone() => new CounterState(this);

    protected override void Initialize()
    {
      Count = 3;
    }
  }
}