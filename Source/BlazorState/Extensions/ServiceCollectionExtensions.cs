namespace BlazorState
{
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using BlazorState.Pipeline.ReduxDevTools;
  using BlazorState.Pipeline.State;
  using BlazorState.Services;
  using MediatR;
  using MediatR.Pipeline;
  using Microsoft.AspNetCore.Components;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Logging.Abstractions;
  using System;
  using System.Linq;
  using System.Net.Http;
  using static BlazorState.Features.Routing.RouteState;

  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Register BlazorState services based on the aConfigure options
    /// </summary>
    /// <param name="aServices"></param>
    /// <param name="aConfigure"></param>
    /// <returns></returns>
    /// <example></example>
    /// <remarks>The order of registration matters.
    /// If the user wants to change they can configure themselves vs using this extension</remarks>
    public static IServiceCollection AddBlazorState(
      this IServiceCollection aServices,
      Action<BlazorStateOptions> aConfigure = null)
    {
      ServiceDescriptor flagServiceDescriptor = aServices.FirstOrDefault(
        aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(BlazorHostingLocation));

      if (flagServiceDescriptor == null)
      {
        var blazorStateOptions = new BlazorStateOptions();
        aConfigure?.Invoke(blazorStateOptions);

        EnsureLogger(aServices);
        EnsureHttpClient(aServices);
        EnsureMediator(aServices, blazorStateOptions);

        aServices.AddScoped<BlazorHostingLocation>();
        aServices.AddScoped<JsonRequestHandler>();
        aServices.AddScoped<Subscriptions>();
        aServices.AddScoped(typeof(IRequestPostProcessor<,>), typeof(RenderSubscriptionsPostProcessor<,>));
        aServices.AddScoped<IStore, Store>();
        aServices.AddSingleton(blazorStateOptions);

        if (blazorStateOptions.UseCloneStateBehavior)
        {
          aServices.AddScoped(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
        }
        if (blazorStateOptions.UseReduxDevToolsBehavior)
        {
          aServices.AddScoped(typeof(IRequestPostProcessor<,>), typeof(ReduxDevToolsPostProcessor<,>));
          aServices.AddScoped<ReduxDevToolsInterop>();

          aServices.AddTransient<IRequestHandler<CommitRequest, Unit>, CommitHandler>();
          aServices.AddTransient<IRequestHandler<JumpToStateRequest, Unit>, JumpToStateHandler>();
          aServices.AddTransient<IRequestHandler<StartRequest, Unit>, StartHandler>();
          aServices.AddScoped(aServiceProvider => (IReduxDevToolsStore)aServiceProvider.GetService<IStore>());
        }
        if (blazorStateOptions.UseRouting)
        {
          aServices.AddScoped<RouteManager>();
          aServices.AddScoped<RouteState>();

          aServices.AddTransient<IRequestHandler<ChangeRouteAction, Unit>, ChangeRouteHandler>();
          aServices.AddTransient<IRequestHandler<InitializeRouteAction, Unit>, InitializeRouteHandler>();
        }
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
            // Creating the NavigationManager needs to wait until the JS Runtime is initialized, so defer it.
            NavigationManager navigationManager = aServiceProvider.GetRequiredService<NavigationManager>();
            return new HttpClient
            {
              BaseAddress = new Uri(navigationManager.BaseUri)
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
    /// Scan Assemblies for Handlers.
    /// </summary>
    /// <param name="aServices"></param>
    /// <param name="aOptions"></param>
    /// <param name="aCallingAssembly">The calling assembly</param>
    private static void EnsureMediator(IServiceCollection aServices, BlazorStateOptions aOptions)
    {
      ServiceDescriptor mediatorServiceDescriptor = aServices.FirstOrDefault(
        aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(IMediator));

      if (mediatorServiceDescriptor == null)
      {
        aServices.AddMediatR(aOptions.Assemblies.ToArray());
      }
    }
  }
}