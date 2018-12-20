namespace BlazorHosted_CSharp.Client.Layout
{
  using Microsoft.AspNetCore.Blazor.Components;

  public class NavMenuModel : BlazorComponent
  {
    protected bool CollapseNavMenu { get; set; }

    protected void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;
  }
}