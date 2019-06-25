namespace BlazorState
{
  using System.Collections.Generic;
  using System.Reflection;

  public class Options
  {
    ///// <summary>
    ///// Assemblies to be searched for MediatR Requests
    ///// </summary>
    public IEnumerable<Assembly> Assemblies { get; set; }

    public bool UseCloneStateBehavior { get; set; } = true;

    public bool UseReduxDevToolsBehavior { get; set; } = true;

    public bool UseRouting { get; set; } = true;

    public Options()
    {
      Assemblies = new Assembly[] { };
    }
  }
}