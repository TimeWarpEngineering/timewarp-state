namespace TestApp.Client.Components
{
  using BlazorState;
  using BlazorState.Services;
  using Microsoft.AspNetCore.Components;

  public class BlazorLocationBase : BlazorStateComponent
  {
    [Inject] public BlazorHostingLocation BlazorHostingLocation { get; set; }

    public string LocationName => BlazorHostingLocation.IsClientSide ? "Client Side" : "Server Side";
  }
}