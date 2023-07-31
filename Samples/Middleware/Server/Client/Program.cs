using System.Reflection;
using BlazorState;
using MediatR;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Middleware.Client;
using Middleware.Client.Features.Application;
using Middleware.Client.Pipeline;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });


builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ProcessingBehavior<,>));

// adding blazor state to the blazore project.
builder.Services.AddBlazorState((aOptions) =>  {
    aOptions.UseReduxDevTools();
    aOptions.Assemblies = new Assembly[]
    {
        typeof(Program).GetTypeInfo().Assembly,
    };
});

await builder.Build().RunAsync();
