namespace TestApp.Server.Integration.Tests.Infrastructure
{
  using MediatR;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.DependencyInjection;
  using System;
  using System.Net.Http;
  using System.Text.Json;
  using System.Threading.Tasks;

  public abstract class BaseTest
  {
    private readonly JsonSerializerOptions JsonSerializerOptions;
    protected readonly IServiceScopeFactory ServiceScopeFactory;
    protected readonly HttpClient HttpClient;

    public BaseTest(WebApplicationFactory<Startup> aWebApplicationFactory, JsonSerializerOptions aJsonSerializerOptions)
    {
      ServiceScopeFactory = aWebApplicationFactory.Services.GetService<IServiceScopeFactory>();
      HttpClient = aWebApplicationFactory.CreateClient();
      JsonSerializerOptions = aJsonSerializerOptions;
    }

    [
      System.Diagnostics.CodeAnalysis.SuppressMessage
      (
        "AsyncUsage",
        "AsyncFixer01:Unnecessary async/await usage",
        Justification = "The serviceScope is disposed to early if not awaited here"
      )
    ]
    protected async Task<T> ExecuteInScope<T>(Func<IServiceProvider, Task<T>> aAction)
    {
      using IServiceScope serviceScope = ServiceScopeFactory.CreateScope();
      return await aAction(serviceScope.ServiceProvider);
    }

    protected Task Send(IRequest aRequest)
    {
      return ExecuteInScope
      (
        aServiceProvider =>
        {
          ISender sender = aServiceProvider.GetService<ISender>();

          return sender.Send(aRequest);
        }
      );
    }

    protected Task<TResponse> Send<TResponse>(IRequest<TResponse> aRequest)
    {
      return ExecuteInScope
      (
        aServiceProvider =>
        {
          ISender sender = aServiceProvider.GetService<ISender>();

          return sender.Send(aRequest);
        }
      );
    }

    protected async Task<TResponse> GetJsonAsync<TResponse>(string aUri)
    {
      HttpResponseMessage httpResponseMessage = await HttpClient.GetAsync(aUri);

      httpResponseMessage.EnsureSuccessStatusCode();

      string json = await httpResponseMessage.Content.ReadAsStringAsync();

      TResponse response = JsonSerializer.Deserialize<TResponse>(json, JsonSerializerOptions);

      return response;
    }

  }
}