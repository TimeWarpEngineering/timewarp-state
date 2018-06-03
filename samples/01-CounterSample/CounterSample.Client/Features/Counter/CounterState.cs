namespace CounterSample.Client.Features.Counter.State
{
  using BlazorState;

  // region used by docfx
  #region CounterState
    
  public class CounterState : State<CounterState>
  {
    /// <summary>
    /// Parameterless constructor needed for deserialization.
    /// </summary>
    public CounterState() { } // needed for serialization

    /// <summary>
    /// Utilize a constructor for cloning
    /// </summary>
    /// <param name="aState"></param>
    protected CounterState(CounterState aState) : this()
    {
      Count = aState.Count;
    }

    public int Count { get; set; }

    public override object Clone() => new CounterState(this);

    /// <summary>
    /// Set the initial state
    /// </summary>
    protected override void Initialize()
    {
      Count = 3;
    }
  }
  #endregion
}