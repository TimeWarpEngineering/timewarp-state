namespace TestApp.Client.Integration.Tests.Infrastructure;

/// <summary>
/// Base class for all tests
/// </summary>
public abstract class BaseTest
{
  private readonly ISender Sender;
  protected readonly IStore Store;
  //protected readonly HttpClient HttpClient;

  protected BaseTest(ClientHost clientHost)
  {
    Console.WriteLine("BaseTest");
    IServiceScopeFactory serviceScopeFactory = clientHost.ServiceProvider.GetService<IServiceScopeFactory>()!;
    IServiceScope serviceScope = serviceScopeFactory.CreateScope();
    ServiceProvider = serviceScope.ServiceProvider;
    Sender = ServiceProvider.GetService<ISender>()!;
    Store = ServiceProvider.GetService<IStore>()!;
  }

  private IServiceProvider ServiceProvider { get; }

  /// <summary>
  /// Send a request to the MediatR pipeline
  /// </summary>
  /// <param name="aRequest"></param>
  /// <typeparam name="TResponse"></typeparam>
  /// <returns></returns>
  // protected Task<TResponse> Send<TResponse>(IRequest<TResponse> aRequest) => Send(aRequest);

  /// <summary>
  /// Send a request to the MediatR pipeline
  /// </summary>
  /// <param name="aRequest"></param>
  protected async Task Send(IRequest aRequest) => await Sender.Send(aRequest);

}
