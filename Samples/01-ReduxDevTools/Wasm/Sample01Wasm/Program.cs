namespace Sample01Wasm;

public class Program
{
  public static async Task Main
  (
    string[] args
  )
  {
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
  }
}
