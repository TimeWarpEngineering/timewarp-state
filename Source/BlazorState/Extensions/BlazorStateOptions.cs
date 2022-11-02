namespace BlazorState;

using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

public class BlazorStateOptions
{
  ///// <summary>
  ///// Assemblies to be searched for MediatR Requests
  ///// </summary>
  public IEnumerable<Assembly> Assemblies { get; set; }

  public bool UseCloneStateBehavior { get; set; } = true;

  public bool UseReduxDevToolsBehavior { get; set; } = false;

  public bool UseRouting { get; set; } = true;

  public JsonSerializerOptions JsonSerializerOptions { get; }

  public BlazorStateOptions()
  {
    Assemblies = Array.Empty<Assembly>();
    JsonSerializerOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
  }
}
