namespace TestApp.Client.Layout;

using BlazorState.Services;
using Microsoft.AspNetCore.Components;

public partial class MainLayout : LayoutComponentBase
{
  [Inject] public BlazorHostingLocation BlazorHostingLocation { get; set; }
}
