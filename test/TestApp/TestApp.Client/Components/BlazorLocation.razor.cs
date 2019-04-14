namespace TestApp.Client.Components
{
  using BlazorState;
  using BlazorState.Services;
  using Microsoft.AspNetCore.Components;

  public class BlazorLocationModel: BlazorStateComponent
  {
    [Inject] public BlazorHostingLocation BlazorHostingLocation { get; set; }
  }
}
