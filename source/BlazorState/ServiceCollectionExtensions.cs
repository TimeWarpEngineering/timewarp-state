namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Reflection;
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Behaviors.State;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using BlazorState.Services;
  using MediatR;
  using Microsoft.AspNetCore.Components.Services;
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

      EnsureLogger(aServices);
      EnsureHttpClient(aServices);

      // GetCallingAssembly is dangerous.  But seems to be the only one that works for this.
      // Getting a stack trace doesn't work on mono.
      EnsureMediator(aServices, options, Assembly.GetCallingAssembly());

      aServices.AddScoped<BlazorHostingLocation>();
      aServices.AddScoped<JsonRequestHandler>();
      if (options.UseCloneStateBehavior)
      {
        aServices.AddScoped(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
        aServices.AddScoped(typeof(IPipelineBehavior<,>), typeof(RenderSubscriptionsBehavior<,>));
        aServices.AddScoped<IStore, Store>();
      }
      if (options.UseReduxDevToolsBehavior)
      {
        aServices.AddScoped(typeof(IPipelineBehavior<,>), typeof(ReduxDevToolsBehavior<,>));
        aServices.AddScoped<ReduxDevToolsInterop>();
        aServices.AddScoped<Subscriptions>();
        aServices.AddScoped(aServiceProvider => (IReduxDevToolsStore)aServiceProvider.GetService<IStore>());
      }
      if (options.UseRouting)
      {
        aServices.AddScoped<RouteManager>();
      }

      return aServices;
    }

    private static void EnsureHttpClient(IServiceCollection aServices)
    {
      var blazorHostingLocation = new BlazorHostingLocation();

      // Server Side Blazor doesn't register HttpClient by default
      if (blazorHostingLocation.IsServerSide)
      {
        // Double check that nothing is registered.
        if (!aServices.Any(aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(HttpClient)))
        {
          // Setup HttpClient for server side in a client side compatible fashion
          aServices.AddScoped<HttpClient>(aServiceProvider =>
          {
            // Creating the URI helper needs to wait until the JS Runtime is initialized, so defer it.
            IUriHelper uriHelper = aServiceProvider.GetRequiredService<IUriHelper>();
            return new HttpClient
            {
              BaseAddress = new Uri(uriHelper.GetBaseUri())
            };
          });
        }
      }
    }

    /// <summary>
    /// If no ILogger is registered it would throw as we inject it.  This provides us with a NullLogger to avoid that
    /// </summary>
    /// <param name="aServices"></param>
    private static void EnsureLogger(IServiceCollection aServices)
    {
      ServiceDescriptor loggerServiceDescriptor = aServices.FirstOrDefault(
        aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(ILogger<>));

      if (loggerServiceDescriptor == null)
      {
        aServices.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
      }
    }

    /// <summary>
    /// Scan Assemblies for Handlers.  Will add this assembly and the calling assembly.
    /// </summary>
    /// <param name="aServices"></param>
    /// <param name="aOptions"></param>
    /// <param name="aCallingAssembly">The calling assembly</param>
    private static void EnsureMediator(IServiceCollection aServices, Options aOptions, Assembly aCallingAssembly)
    {
      var assemblies = new List<Assembly>(aOptions.Assemblies)
      {
        Assembly.GetAssembly(typeof(ServiceCollectionExtensions)),
        aCallingAssembly
      };

      aServices.AddMediatR(assemblies.ToArray());
    }
  }

  public class Options
  {
    public Options()
    {
      Assemblies = new Assembly[] { };
    }

    ///// <summary>
    ///// Assemblies to be searched for MediatR Requests
    ///// </summary>
    public IEnumerable<Assembly> Assemblies { get; set; }

    public bool UseCloneStateBehavior { get; set; } = true;
    public bool UseReduxDevToolsBehavior { get; set; } = true;
    public bool UseRouting { get; set; } = true;
  }
}