namespace TestApp.Client.Integration.Tests.Infrastructure;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Based on Jimmy's SliceFixture
/// https://github.com/jbogard/ContosoUniversityDotNetCore-Pages/blob/master/ContosoUniversity.IntegrationTests/SliceFixture.cs
/// </remarks>
public abstract class BaseTest
{
  private readonly IServiceScopeFactory ServiceScopeFactory;
  private readonly IServiceScope ServiceScope;
  private readonly ISender Sender;
  protected readonly IStore Store;
  protected readonly HttpClient HttpClient;

  public BaseTest(ClientHost aWebAssemblyHost)
  {
    Console.WriteLine("BaseTest");
    ServiceScopeFactory = aWebAssemblyHost.ServiceProvider.GetService<IServiceScopeFactory>();
    ServiceScope = ServiceScopeFactory.CreateScope();
    ServiceProvider = ServiceScope.ServiceProvider;
    Sender = ServiceProvider.GetService<ISender>();
    Store = ServiceProvider.GetService<IStore>();
  }

  public IServiceProvider ServiceProvider { get; }

  protected Task<TResponse> Send<TResponse>(IRequest<TResponse> aRequest) => Send(aRequest);

  protected async Task Send(IRequest aRequest) => await Sender.Send(aRequest);

}
