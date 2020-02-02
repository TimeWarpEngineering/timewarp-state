namespace TestApp.Client.Layout
{
  using Microsoft.AspNetCore.Components;
  using TestApp.Client.Features.Base.Components;

  public class NavMenuBase : BaseComponent
  {
    protected bool CollapseNavMenu { get; set; }

    protected string NavMenuCssClass => CollapseNavMenu ? "collapse" : null;

    protected void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;
  }
}