namespace TestApp.Client.Layout
{
  using BlazorState.Services;
  using Microsoft.AspNetCore.Components;

  public class MainLayoutModel : LayoutComponentBase
  {
    [Inject] public BlazorHostingLocation BlazorHostingLocation { get; set; }
  }
}
