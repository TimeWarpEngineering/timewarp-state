using BlazorState;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;

namespace CounterSample.Client
{
  public class Program
  {
    private static void Main(string[] args)
    {
      var serviceProvider = new BrowserServiceProvider(services =>
      {
        services.AddBlazorState(options =>
        {
          options.UseReduxDevToolsBehavior = false; // See other demo on using ReduxDevTools
          options.UseRouting = false; // See other demo on Routing.
          options.UseCloneStateBehavior = true; // The basics.
        });
      });

      new BrowserRenderer(serviceProvider).AddComponent<App>("app");
    }
  }
}