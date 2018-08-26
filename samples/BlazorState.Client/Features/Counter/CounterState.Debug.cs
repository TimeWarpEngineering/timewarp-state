#if  DEBUG
namespace BlazorState.Client.Features.Counter
{
  using System;
  using System.Collections.Generic;
  using BlazorState;
  using Microsoft.JSInterop;

  public partial class CounterState : State<CounterState>
  {

    public override CounterState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {
      var counterState = new CounterState()
      {
        Count = Convert.ToInt32(aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Count))]),
        Guid = new System.Guid((string)aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))]),
      };

      return counterState;
    }
  }
}
#endif