using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Sample.Client;
using System.Reflection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazorState
(
  options =>
  {
    options.UseReduxDevTools();
    options.Assemblies =
    new Assembly[]
    {
      typeof(Program).GetTypeInfo().Assembly,
    };
  }
);

await builder.Build().RunAsync();
