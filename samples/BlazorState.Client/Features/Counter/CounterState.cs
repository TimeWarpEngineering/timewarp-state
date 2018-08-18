namespace BlazorState.Client.Features.Counter
{
  using System.Reflection;
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

    public int Count { get; private set; }

    public override object Clone() => new CounterState(this);

    /// <summary>
    /// Set the Initial State
    /// </summary>
    protected override void Initialize() => Count = 3;

    /// <summary>
    /// Use in Tests ONLY, to initialize the State
    /// </summary>
    /// <param name="aCount"></param>
    public void Initialize(int aCount)
    {
      ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
      Count = aCount;
    }
  }
}