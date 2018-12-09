namespace BlazorState
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Net.Http;
  using System.Reflection;
  using BlazorState.Behaviors.ReduxDevTools;
  using BlazorState.Behaviors.State;
  using BlazorState.Features.JavaScriptInterop;
  using BlazorState.Features.Routing;
  using BlazorState.Services;
  using MediatR;
  using Microsoft.AspNetCore.Blazor.Services;
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

      aServices.AddScoped<JsRuntimeLocation>();
      aServices.AddScoped<JsonRequestHandler>();
      if (options.UseCloneStateBehavior)
      {
        aServices.AddScoped(typeof(IPipelineBehavior<,>), typeof(CloneStateBehavior<,>));
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

    private static void EnsureMediator(IServiceCollection aServices, Options aOptions, Assembly aExecutingAssembly)
    {
      var assemblies = new List<Assembly>(aOptions.Assemblies)
      {
        // Need to add this assembly
        Assembly.GetAssembly(typeof(ServiceCollectionExtensions))
      };

      // By default add in the executing assembly
      if (assemblies.Count() == 1)
      {
        assemblies.Add(aExecutingAssembly);
      }
      aServices.AddMediatR(assemblies);
    }

    //private static void AddMediator(IServiceCollection aServices, Options options)
    //{
    //  var assemblies = new List<Assembly>(options.Assemblies)
    //  {
    //    // Need to add this assembly
    //    Assembly.GetAssembly(typeof(ServiceCollectionExtensions))
    //  };
    //  // By default add in the assembly that is calling AddBlazorState
    //  if (assemblies.Count() == 1)
    //  {
    //    Assembly clientAssembly;
    //    var jsRuntimeLocation = new JsRuntimeLocation();
    //    if (jsRuntimeLocation.HasMono)
    //    {
    //      Console.WriteLine("HasMono == true");
    //      clientAssembly = Assembly.GetExecutingAssembly();
    //    }
    //    else
    //    {
    //      Console.WriteLine("HasMono == false");
    //      StackFrame[] stackFrames = new StackTrace().GetFrames();
    //      Console.WriteLine($"stackFrames.Length: {stackFrames.Length}");
    //      int frameIndex = Array.FindIndex(stackFrames, aStackFrame => aStackFrame.GetMethod().Name == "AddBlazorState");
    //      Console.WriteLine($"frameIndex:{frameIndex}");
    //      clientAssembly = stackFrames[frameIndex + 1].GetMethod().ReflectedType.Assembly;
    //    }

    //    assemblies.Add(clientAssembly);
    //  }
    //  aServices.AddMediatR(assemblies);
    //}


    private static void EnsureHttpClient(IServiceCollection aServices)
    {
      var jsRuntimeLocation = new JsRuntimeLocation();
      if (jsRuntimeLocation.IsServerSide)
      {
        // Server Side Blazor doesn't register HttpClient by default
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

        Console.WriteLine("Running app server side");
      }
      else
      {
        Console.WriteLine("Running client side in Mono WASM");
      }
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