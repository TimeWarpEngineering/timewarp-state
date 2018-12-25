namespace BlazorHosted_CSharp.Client.Features.Counter
{
  using BlazorState;

  public partial class CounterState : State<CounterState>
  {
    public CounterState() { }

    /// <summary>
    /// Creates new instance based off an exiting one.
    /// </summary>
    /// <remarks>Constructor used for Clone</remarks>
    /// <param name="aState"></param>
    protected CounterState(CounterState aState) : this()
    {
      Count = aState.Count;
    }

    public override object Clone() => new CounterState(this);

    /// <summary>
    /// Set the Initial State
    /// </summary>
    protected override void Initialize() => Count = 3;
  }
}