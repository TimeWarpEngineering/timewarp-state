namespace Test.App.Client.Features.Counter;

using Microsoft.JSInterop;
using System.Collections.Generic;

public partial class CounterZeroState : State<CounterZeroState>
{
  public override CounterZeroState Hydrate(IDictionary<string, object> aKeyValuePairs)
  {
    var counterState = new CounterZeroState()
    {
      Count = Convert.ToInt32(aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Count))].ToString()),
      Guid = new System.Guid(aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString()),
    };

    return counterState;
  }

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
