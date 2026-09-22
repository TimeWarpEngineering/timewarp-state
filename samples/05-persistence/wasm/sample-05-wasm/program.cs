#region Purpose
// Blazor WebAssembly host that persists one state to session storage and another to local storage.
#endregion

#region Design
// Which browser API a state uses is PersistentStateMethod. Which implementation is registered is
// Blazored: AddBlazoredSessionStorage versus AddBlazoredLocalStorage.
// PersistenceService takes both storage services, so this host registers both even though each
// state uses only one.
// Save and load share TimeWarpStateOptions.JsonSerializerOptions. The enum converter is added
// there so accent values round-trip as names.
// UseReduxDevTools satisfies CommitHandler, which the mediator links even when the sample
// does not show the extension. The component that would call InitAsync is absent.
// AddGeneratedMediator is emitted into this assembly. There is no reflection AddMediator call.
#endregion

namespace Sample05Wasm;

public class Program
{
  public static async Task Main(string[] args)
  {
    WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");
    builder.RootComponents.Add<HeadOutlet>("head::after");

    // Session storage is per tab and dies with the tab. Local storage stays on this origin
    // until script or the user clears it. Both are readable by any script on the origin.
    // Do not put credentials or tokens in either one.
    builder.Services.AddBlazoredSessionStorage();
    builder.Services.AddBlazoredLocalStorage();

    builder.Services.AddGeneratedMediator<ClientPipeline>();

    builder.Services.AddTimeWarpState
    (
      options =>
      {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        // CommitHandler is linked by the generated mediator and its constructor needs the
        // redux services. Development validates the container, so those services must exist.
        // This sample does not render <ReduxDevTools />, so InitAsync never runs and actions
        // are not forwarded to the browser extension.
        options.UseReduxDevTools();
      }
    );

    builder.Services.AddScoped<IPersistenceService, PersistenceService>();

    await builder.Build().RunAsync();
  }
}
