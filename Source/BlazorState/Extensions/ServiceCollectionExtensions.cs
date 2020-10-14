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
  using System.Collections.Generic;
  using System.Linq;
  using System.Net.Http;
  using System.Reflection;
  using static BlazorState.Features.Routing.RouteState;

  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Register BlazorState services based on the aConfigure options
    /// </summary>
    /// <param name="aServiceCollection"></param>
    /// <param name="aConfigureBlazorStateOptionsAction"></param>
    /// <returns></returns>
    /// <example></example>
    /// <remarks>The order of registration matters.
    /// If the user wants to change they can configure themselves vs using this extension</remarks>
    public static IServiceCollection AddBlazorState
    (
      this IServiceCollection aServiceCollection,
      Action<BlazorStateOptions> aConfigureBlazorStateOptionsAction = null
    )
    {
      ServiceDescriptor flagServiceDescriptor = aServiceCollection.FirstOrDefault
      (
        aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(BlazorHostingLocation)
      );

      if (flagServiceDescriptor == null)
      {
        var blazorStateOptions = new BlazorStateOptions();
        aConfigureBlazorStateOptionsAction?.Invoke(blazorStateOptions);

        EnsureLogger(aServiceCollection);
        EnsureHttpClient(aServiceCollection);
        EnsureMediator(aServiceCollection, blazorStateOptions);
        EnusureStates(aServiceCollection, blazorStateOptions);

        aServiceCollection.AddScoped<BlazorHostingLocation>();
        aServiceCollection.AddScoped<JsonRequestHandler>();
        aServiceCollection.AddScoped<Subscriptions>();
        aServiceCollection.AddScoped(typeof(IRequestPostProcessor<,>), typeof(RenderSubscriptionsPostProcessor<,>));
        aServiceCollection.AddScoped<IStore, Store>();
        aServiceCollection.AddSingleton(blazorStateOptions);

        if (blazorStateOptions.UseCloneStateBehavior)
        {
          aServiceCollection.AddScoped(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
        }
        if (blazorStateOptions.UseReduxDevToolsBehavior)
        {
          aServiceCollection.AddScoped(typeof(IRequestPostProcessor<,>), typeof(ReduxDevToolsPostProcessor<,>));
          aServiceCollection.AddScoped<ReduxDevToolsInterop>();

          aServiceCollection.AddTransient<IRequestHandler<CommitRequest, Unit>, CommitHandler>();
          aServiceCollection.AddTransient<IRequestHandler<JumpToStateRequest, Unit>, JumpToStateHandler>();
          aServiceCollection.AddTransient<IRequestHandler<StartRequest, Unit>, StartHandler>();
          aServiceCollection.AddScoped(aServiceProvider => (IReduxDevToolsStore)aServiceProvider.GetService<IStore>());
        }
        if (blazorStateOptions.UseRouting)
        {
          aServiceCollection.AddScoped<RouteManager>();
          aServiceCollection.AddScoped<RouteState>();

          aServiceCollection.AddTransient<IRequestHandler<ChangeRouteAction, Unit>, ChangeRouteHandler>();
          aServiceCollection.AddTransient<IRequestHandler<InitializeRouteAction, Unit>, InitializeRouteHandler>();
        }
      }
      return aServiceCollection;
    }

    private static void EnsureHttpClient(IServiceCollection aServiceCollection)
    {
      var blazorHostingLocation = new BlazorHostingLocation();

      // Server Side Blazor doesn't register HttpClient by default
      if (blazorHostingLocation.IsServerSide)
      {
        // Double check that nothing is registered.
        if (!aServiceCollection.Any(aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(HttpClient)))
        {
          // Setup HttpClient for server side in a client side compatible fashion
          aServiceCollection.AddScoped<HttpClient>
          (
            aServiceProvider =>
            {
              // Creating the NavigationManager needs to wait until the JS Runtime is initialized, so defer it.
              NavigationManager navigationManager = aServiceProvider.GetRequiredService<NavigationManager>();
              return new HttpClient
              {
                BaseAddress = new Uri(navigationManager.BaseUri)
              };
            }
          );
        }
      }
    }

    /// <summary>
    /// If no ILogger is registered it would throw as we inject it.  This provides us with a NullLogger to avoid that
    /// </summary>
    /// <param name="aServiceCollection"></param>
    private static void EnsureLogger(IServiceCollection aServiceCollection)
    {
      ServiceDescriptor loggerServiceDescriptor = aServiceCollection.FirstOrDefault
      (
        aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(ILogger<>)
      );

      if (loggerServiceDescriptor == null)
      {
        aServiceCollection.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
      }
    }

    /// <summary>
    /// Scan Assemblies for Handlers.
    /// </summary>
    /// <param name="aServiceCollection"></param>
    /// <param name="aBlazorStateOptions"></param>
    private static void EnsureMediator(IServiceCollection aServiceCollection, BlazorStateOptions aBlazorStateOptions)
    {
      ServiceDescriptor mediatorServiceDescriptor = aServiceCollection.FirstOrDefault
      (
        aServiceDescriptor => aServiceDescriptor.ServiceType == typeof(IMediator)
      );

      if (mediatorServiceDescriptor == null)
      {
        aServiceCollection.AddMediatR(aBlazorStateOptions.Assemblies.ToArray());
      }
    }

    private static void EnusureStates(IServiceCollection aServiceCollection, BlazorStateOptions aBlazorStateOptions)
    {
      foreach (Assembly assembly in aBlazorStateOptions.Assemblies)
      {
        IEnumerable<Type> types = assembly.GetTypes().Where
        (
          aType =>
          !aType.IsAbstract &&
          !aType.IsInterface &&
          aType.BaseType != null &&
          aType.BaseType.IsGenericType &&
          aType.BaseType.GetGenericTypeDefinition() == typeof(State<>)
        );

        foreach (Type type in types)
        {
          if (!aServiceCollection.Any(aServiceDescriptor => aServiceDescriptor.ServiceType == type))
          {
            aServiceCollection.AddTransient(type);
          }
        }
      }
    }
  }
}