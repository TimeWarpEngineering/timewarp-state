namespace TestApp.Client.Layout
{
  using TestApp.Client.Features.Base.Components;

  public partial class NavMenu : BaseComponent
  {
    protected bool CollapseNavMenu { get; set; }

    protected string NavMenuCssClass => CollapseNavMenu ? "collapse" : null;

    protected void ToggleNavMenu() => CollapseNavMenu = !CollapseNavMenu;
  }
}