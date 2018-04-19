namespace BlazorState.Extentions.DependencyInjection
{
  using System;
  using System.Linq;
  using BlazorState.Behaviors.JavaScriptInterop;
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Behaviors.State;
  using BlazorState.Features.Routing;
  using BlazorState.Store;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Logging.Abstractions;

  public static class ServiceCollectionExtensions
  {
    /// <summary>
    ///
    /// </summary>
    /// <param name="aServices"></param>
    /// <param name="aConfigure"></param>
    /// <returns></returns>
    /// <example></example>
    /// <remarks>The order of registration matters. If the user wants to change they can configure themself vs using the extention</remarks>
    public static IServiceCollection AddBlazorState(
      this IServiceCollection aServices,
      Action<Options> aConfigure = null)
    {
      var options = new Options();
      aConfigure?.Invoke(options);

      ServiceDescriptor loggerServiceDescriptor =
        aServices.FirstOrDefault(s => s.ServiceType == typeof(ILogger<>));
      if (loggerServiceDescriptor == null)
      {
        aServices.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
      }

      //TODO: some behaviors depend on others
      // example ReduxDevToosl depends on StateBehavoir
      // We should build a dependency list based on the Options and then register from the resulting list.
      // If we seperate behaviors into own packages that will change things.
      aServices.AddMediatR();
      if (options.UseCloneStateBehavior)
      {
        aServices.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
        aServices.AddSingleton(typeof(IStore), typeof(Store));
      }
      if (options.UseReduxDevToolsBehavior)
      {
        aServices.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ReduxDevToolsBehavior<,>));
        aServices.AddSingleton<ReduxDevToolsInterop>();
        aServices.AddSingleton<JsonRequestHandler>();
        aServices.AddSingleton<ComponentRegistry>();
      }
      if (options.UseRouting)
      {
        aServices.AddSingleton<RouteManager>();
      }

      return aServices;
    }
  }

  public class Options
  {
    public bool UseCloneStateBehavior { get; set; } = true;
    public bool UseReduxDevToolsBehavior { get; set; } = true;
    public bool UseRouting { get; set; } = true;
  }
}