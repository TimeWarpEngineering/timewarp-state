namespace TestApp.Client.Features.Application
{
  using System.Collections.Generic;
  using System.Reflection;
  using BlazorState;
  using Microsoft.JSInterop;

  internal partial class ApplicationState : State<ApplicationState>
  {
    public override ApplicationState Hydrate(IDictionary<string, object> aKeyValuePairs)
    {

      return new ApplicationState
      {
        Guid = new System.Guid(aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString()),
        Name = aKeyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Name))].ToString(),
      };
    }

    internal void Initialize(string aName)
    {
      ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
      Name = aName;
    }
  }
}
