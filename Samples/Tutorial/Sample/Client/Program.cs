using BlazorState;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sample.Client;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazorState
(
    (aOptions) =>
    {
      aOptions.UseReduxDevTools();
      aOptions.Assemblies =
      new Assembly[]
      {
            typeof(Program).GetTypeInfo().Assembly,
      };
    }
);
builder.Services.AddSingleton(builder.Services);

await builder.Build().RunAsync();
