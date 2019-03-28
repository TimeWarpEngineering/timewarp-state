namespace TestApp.Client.Layout
{
  using Microsoft.AspNetCore.Components;

  public class NavMenuModel : ComponentBase
  {
    protected bool CollapseNavMenu { get; set; }

    protected void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;
  }
}