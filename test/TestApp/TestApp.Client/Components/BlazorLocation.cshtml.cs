namespace TestApp.Client.Components
{
  using BlazorState.Services;
  using Microsoft.AspNetCore.Components;

  public class BlazorLocationModel: ComponentBase
  {
    [Inject] public BlazorHostingLocation BlazorHostingLocation { get; set; }
  }
}
