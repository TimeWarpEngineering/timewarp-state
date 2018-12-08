namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Behaviors.State;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using BlazorState.Services;
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
    /// <remarks>The order of registration matters. 
    /// If the user wants to change they can configure themselves vs using this extension</remarks>

    public static IServiceCollection AddBlazorState(
      this IServiceCollection aServices,
      Action<Options> aConfigure = null)
    {
      var options = new Options();
      aConfigure?.Invoke(options);

      var assemblies = new List<Assembly>(options.Assemblies)
      {
        // Need to add this assembly
        Assembly.GetAssembly(typeof(ServiceCollectionExtensions))
      };

      // By default add in the calling assembly
      if (assemblies.Count() == 1)
      {
        assemblies.Add(Assembly.GetCallingAssembly());
      }

      EnsureLogger(aServices);

      aServices.AddMediatR(assemblies);
      aServices.AddSingleton<JsRuntimeLocation>();
      aServices.AddSingleton<JsonRequestHandler>();
      if (options.UseCloneStateBehavior)
      {
        aServices.AddSingleton(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
        aServices.AddSingleton<IStore, Store>();
      }
      if (options.UseReduxDevToolsBehavior)
      {
        aServices.AddSingleton(typeof(IPipelineBehavior<,>), typeof(ReduxDevToolsBehavior<,>));
        aServices.AddSingleton<ReduxDevToolsInterop>();
        aServices.AddSingleton<Subscriptions>();
        aServices.AddSingleton(aServiceProvider => (IReduxDevToolsStore)aServiceProvider.GetService<IStore>());
      }
      if (options.UseRouting)
      {
        aServices.AddSingleton<RouteManager>();
      }

      return aServices;
    }

    private static void EnsureLogger(IServiceCollection aServices)
    {
      ServiceDescriptor loggerServiceDescriptor = aServices.FirstOrDefault(
        aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(ILogger<>));

      if (loggerServiceDescriptor == null)
      {
        aServices.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
      }
    }
  }

  public class Options
  {
    public Options()
    {
      Assemblies = new Assembly[] { };
    }

    public bool UseCloneStateBehavior { get; set; } = true;
    public bool UseReduxDevToolsBehavior { get; set; } = true;
    public bool UseRouting { get; set; } = true;
    ///// <summary>
    ///// Assemblies to be searched for MediatR Requests
    ///// </summary>
    public IEnumerable<Assembly> Assemblies { get; set; }
  }
}