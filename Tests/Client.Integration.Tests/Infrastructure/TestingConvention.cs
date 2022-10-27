namespace TestApp.Client.Integration.Tests.Infrastructure;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

[NotTest]
public class TestingConvention : TimeWarp.Fixie.TestingConvention
{ 
  public TestingConvention():base(ConfigureTestServices)  {  }


  private static void ConfigureTestServices(ServiceCollection aServiceCollection)
  {
    var serverWebApplicationFactory = new WebApplicationFactory<TestApp.Server.Startup>();
    HttpClient serverHttpClient = serverWebApplicationFactory.CreateClient();

    ConfigureWebAssemblyHost(aServiceCollection, serverHttpClient);

    aServiceCollection.AddSingleton(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
  }

  private static void ConfigureWebAssemblyHost(ServiceCollection aServiceCollection, HttpClient serverHttpClient)
  {
    var clientHostBuilder = ClientHostBuilder.CreateDefault();
    ConfigureServices(clientHostBuilder.Services, serverHttpClient);

    ClientHost clientHost = clientHostBuilder.Build();
    aServiceCollection.AddSingleton(clientHost);
  }

  private static void ConfigureServices(IServiceCollection aServiceCollection, HttpClient serverHttpClient)
  {
    // Need an HttpClient to talk to the Server side configured before calling AddBlazorState.
    aServiceCollection.AddSingleton(serverHttpClient);
    aServiceCollection.AddBlazorState
    (
      aOptions => aOptions.Assemblies =
      new Assembly[] { typeof(TestApp.Client.Program).GetTypeInfo().Assembly }
    );

    aServiceCollection.AddSingleton
    (
      new JsonSerializerOptions
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      }
    );
  }

  //private bool DisposedValue;

  //protected virtual void Dispose(bool aIsDisposing)
  //{
  //  if (!DisposedValue)
  //  {
  //    if (aIsDisposing)
  //    {
  //      Console.WriteLine("==== Disposing ====");
  //      ServerWebApplicationFactory?.Dispose();
  //    }

  //    DisposedValue = true;
  //  }
  //}

  //public void Dispose() => Dispose(true);
}
