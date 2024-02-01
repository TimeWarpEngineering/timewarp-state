namespace Test.App.Client.Features.CloneTest;

using Microsoft.JSInterop;
using System.Collections.Generic;

internal partial class CloneTestState : State<CloneTestState>
{
  public override CloneTestState Hydrate(IDictionary<string, object> aKeyValuePairs)
  {
    var counterState = new CloneTestState
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
