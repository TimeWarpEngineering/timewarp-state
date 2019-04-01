namespace BlazorHosted_CSharp.Client.Layout
{
  using BlazorState.Services;
  using Microsoft.AspNetCore.Components;
  using Microsoft.AspNetCore.Components.Layouts;

  public class MainLayoutModel : LayoutComponentBase
  {
    [Inject] public BlazorHostingLocation BlazorHostingLocation { get; set; }
  }
}
