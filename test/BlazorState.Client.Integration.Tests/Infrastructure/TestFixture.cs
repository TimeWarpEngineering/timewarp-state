namespace BlazorState.Integration.Tests.Infrastructure
{
  using System;
  using System.Net.Http;
  using System.Threading.Tasks;
  using BlazorState;
  using BlazorState.Client.Integration.Tests.Infrastructure;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;

  /// <summary>
  /// A known starting state(baseline) for all tests.
  /// And Common set of functions
  /// </summary>
  public class TestFixture//: IMediatorFixture, IStoreFixture, IServiceProviderFixture
  {
    //private static readonly IServiceScopeFactory s_scopeFactory;
    public ServiceProvider ServiceProvider { get; set; }
    private IMediator Mediator { get; set; }
    public TestFixture()
    {
      var serviceCollection = new ServiceCollection();
      ConfigureServices(serviceCollection);
      ServiceProvider = serviceCollection.BuildServiceProvider();
      //s_scopeFactory = ServiceProvider.GetService<IServiceScopeFactory>();
    }

    #region ServiceProvider

    /// <summary>
    /// This does what would be done in a StartUp class
    /// </summary>
    /// <param name="services"></param>
    private static void ConfigureServices(ServiceCollection aServiceCollection)
    {
      // TODO: why not inject this.
      //var blazorStateTestServer = new BlazorStateTestServer();
      //aServiceCollection.AddSingleton(blazorStateTestServer.CreateClient());
      //aServiceCollection.AddSingleton(new HttpClient(new BrowserHttpMessageHandler())
      //{
      //  BaseAddress = new Uri(BrowserUriHelper.Instance.GetBaseUri())
      //});
      // Add HttpClient here like is done in BrowserServiceProvider?
      aServiceCollection.AddBlazorState();
      //aServiceCollection.AddSingleton<BlazorStateTestServer>();
    }
    #endregion

    #region Mediator

    //public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    //{
    //  IMediator mediator = ServiceProvider.GetService<IMediator>();
    //  return Mediator.Send(request);
    //}

    //public Task SendAsync(IRequest request)
    //{
    //  IMediator mediator = ServiceProvider.GetService<IMediator>();
    //  return mediator.Send(request);
    //}
    #endregion

    #region Respawn
    // Not needed for Client side see BlazorState instead.
    #endregion

    #region DbContext
    // Not needed for Client side see BlazorState instead.
    #endregion

    #region BlazorState
    //Test the SendAsync stuff then think how to initialize State to know condition.

    #endregion
  }
}
