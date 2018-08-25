#if DEBUG
namespace BlazorStateSample.Client.Features.Counter
{
  using System.Collections.Generic;
  using BlazorState;

  public partial class CounterState : State<CounterState>
  {
    public override CounterState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      return new CounterState()
      {
        Count = (int)aKeyValuePairs[nameof(Count)],
        Guid = new System.Guid((string)aKeyValuePairs[nameof(Guid)])
      };
    }

  }
}
#endif