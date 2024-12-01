var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTimeWarpState
(
    options =>
    {
        options.UseReduxDevTools(); // Enable Redux DevTools
    }
);

await builder.Build().RunAsync();
