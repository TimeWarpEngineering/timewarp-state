namespace TestApp.Client.Components
{
  using BlazorState.Services;
  using Microsoft.AspNetCore.Components;
  using TestApp.Client.Features.Base.Components;

  public partial class BlazorLocation : BaseComponent
  {
    [Inject] public BlazorHostingLocation BlazorHostingLocation { get; set; }

    public string LocationName => BlazorHostingLocation.IsClientSide ? "Client Side" : "Server Side";
  }
}