namespace BlazorHosted_CSharp.Client.Layout
{
  using BlazorState.Services;
  using Microsoft.AspNetCore.Blazor.Components;
  using Microsoft.AspNetCore.Blazor.Layouts;

  public class MainLayoutModel : BlazorLayoutComponent
  {
    [Inject] public JsRuntimeLocation JsRuntimeLocation { get; set; }
  }
}
