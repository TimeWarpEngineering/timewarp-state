namespace TestApp.Client.Integration.Tests.Infrastructure;

/// <summary>
/// Base class for all tests
/// </summary>
public abstract class BaseTest
{
  private readonly ISender Sender;
  protected readonly IStore Store;
  protected readonly Subscriptions Subscriptions;
  protected readonly RenderSubscriptionContext RenderSubscriptionContext;

  protected BaseTest(ClientHost clientHost)
  {
    Console.WriteLine("BaseTest");
    IServiceScopeFactory serviceScopeFactory = clientHost.ServiceProvider.GetService<IServiceScopeFactory>()!;
    IServiceScope serviceScope = serviceScopeFactory.CreateScope();
    ServiceProvider = serviceScope.ServiceProvider;
    Sender = ServiceProvider.GetService<ISender>()!;
    Store = ServiceProvider.GetService<IStore>()!;
    Subscriptions = ServiceProvider.GetService<Subscriptions>()!;
    RenderSubscriptionContext = ServiceProvider.GetService<RenderSubscriptionContext>()!;
  }

  private IServiceProvider ServiceProvider { get; }

  /// <summary>
  /// Send a request to the TimeWarp.Mediator pipeline
  /// </summary>
  /// <param name="request"></param>
  protected async Task Send(IRequest request) => await Sender.Send(request);
}
