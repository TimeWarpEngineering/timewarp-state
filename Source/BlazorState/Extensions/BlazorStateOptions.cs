namespace BlazorState;

public class BlazorStateOptions
{
  ///// <summary>
  ///// Assemblies to be searched for MediatR Requests
  ///// </summary>
  public IEnumerable<Assembly> Assemblies { get; set; }

  public bool UseStateTransactionBehavior { get; set; } = true;

  public bool UseRouting { get; set; } = true;

  public JsonSerializerOptions JsonSerializerOptions { get; }
  
  public readonly IServiceCollection ServiceCollection;

  public BlazorStateOptions(IServiceCollection aServiceCollection)
  {
    ServiceCollection = aServiceCollection;
    Assemblies = Array.Empty<Assembly>();
    JsonSerializerOptions = new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
  }
}
