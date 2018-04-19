namespace BlazorState.Client
{
  using System;
  using Blazor.Extensions.Logging;
  using BlazorState.Extentions.DependencyInjection;
  using Microsoft.AspNetCore.Blazor.Browser.Rendering;
  using Microsoft.AspNetCore.Blazor.Browser.Services;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;

  public class Program
  {
    private static void Main(string[] args)
    {
      var serviceProvider = new BrowserServiceProvider(services =>
      {
        services.AddLogging(builder => builder
            .AddBrowserConsole()
            .SetMinimumLevel(LogLevel.Trace)
        );
        services.AddBlazorState();
      });

      new BrowserRenderer(serviceProvider).AddComponent<App>("app");
    }
  }
}