namespace BlazorHosted_CSharp.Client.Shared
{
  using BlazorState.Services;
  using Microsoft.AspNetCore.Blazor.Components;
  using Microsoft.AspNetCore.Blazor.Layouts;

  public class MainLayoutModel : BlazorLayoutComponent
  {
    [Inject] public JsRuntimeLocation JsRuntimeLocation { get; set; }
  }
}
