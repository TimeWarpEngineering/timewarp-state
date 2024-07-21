---
uid: TimeWarp.State:Tutorial.md
title: TimeWarp.State Adding ReduxDevTools
---

### Prerequisites

00-StateActionSet

## ReduxDevTools JavaScript Interop and RouteState

To [enable ReduxDevTools](xref:TimeWarp.State:AddReduxDevTools.md) update `Program.cs` as follows:

```csharp
using TimeWarp.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sample.Client;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddTimeWarpState
(
    options =>
    {
        options.UseReduxDevTools(); // <== Add this line
        options.Assemblies =
        new Assembly[]
        {
            typeof(Program).GetTypeInfo().Assembly,
        };
    }
);

await builder.Build().RunAsync();    
```

To facilitate JavaScript Interop, enable ReduxDevTools, and manage RouteState, add `App.razor.cs` in the same directory as `App.razor` as follows:

```csharp
namespace Sample.Client;

using System.Threading.Tasks;
using TimeWarp.State.Pipeline.ReduxDevTools;
using TimeWarp.State.Features.JavaScriptInterop;
using TimeWarp.State.Features.Routing;
using Microsoft.AspNetCore.Components;

public partial class App : ComponentBase
{
  [Inject] private JsonRequestHandler JsonRequestHandler { get; set; } = null!;
  [Inject] private ReduxDevToolsInterop ReduxDevToolsInterop { get; set; } = null!;

  [Inject]
  [System.Diagnostics.CodeAnalysis.SuppressMessage
    (
      "CodeQuality", 
      "IDE0051:Remove unused private members", 
      Justification = "It is used, the constructor has side effects "
    )
  ]
  private RouteManager RouteManager { get; set; } = null!;

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await ReduxDevToolsInterop.InitAsync();
    await JsonRequestHandler.InitAsync();
  }
}
```

Now run your app again and then Open the Redux Dev Tools (a tab in Chrome Dev Tools) and you should see Actions as they are executed.

![ReduxDevTools](Images/ReduxDevTools.png)
If you inspect the State in the DevTools you will also notice it maintains the current Route in RouteState.

![ReduxRouteState](Images/ReduxRouteState.png)

Congratulations that is the basics of TimeWarp.State.


[^1]: https://github.com/TimeWarpEngineering/timewarp-architecture/blob/master/TimeWarp.Architecture/Documentation/Developer/Conceptual/ArchitecturalDecisionRecords/ProjectStructureAndConventions/ProjectStructureAndConventions.md
