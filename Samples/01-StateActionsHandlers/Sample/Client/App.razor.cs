namespace Sample.Client;

using TimeWarp.Features.ReduxDevTools;
using TimeWarp.Features.JavaScriptInterop;
using Microsoft.AspNetCore.Components;

public partial class App : ComponentBase
{
  [Inject] private JsonRequestHandler JsonRequestHandler { get; set; } = null!;
  [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; } = null!;
}
