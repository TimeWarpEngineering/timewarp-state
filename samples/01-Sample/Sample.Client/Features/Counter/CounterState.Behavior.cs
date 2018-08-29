namespace Sample.Client.Features.Counter
{
  using BlazorState;

  public partial class CounterState : State<CounterState>
  {

    /// <summary>
    /// Required Parameterless constructor for automatic creation of the State.
    /// </summary>
    public CounterState() { }

    /// <summary>
    /// Creates new instance based off an exiting one.
    /// </summary>
    /// <remarks>Constructor used for Clone</remarks>
    /// <param name="aState">The item we want to clone</param>
    protected CounterState(CounterState aState)
    {
      Count = aState.Count;
    }

    /// <summary>
    /// Clone the existing object
    /// </summary>
    /// <returns>A clone of this object</returns>
    public override object Clone() => new CounterState(this);

    /// <summary>
    /// Set the Initial State
    /// </summary>
    protected override void Initialize() => Count = 3;
  }
}
