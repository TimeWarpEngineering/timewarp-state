namespace ServiceProvider;

public class GetService_Returns : BaseTest
{
  private readonly IServiceProvider ServiceProvider;

  public GetService_Returns
  (
    WebApplicationFactory<TestApp.Server.Startup> aWebApplicationFactory,
    JsonSerializerOptions aJsonSerializerOptions
  ) : base(aWebApplicationFactory, aJsonSerializerOptions)
  {
    ServiceProvider = ServiceScopeFactory.CreateScope().ServiceProvider;
  }
  public void IMediator()
  {
    IMediator mediator = ServiceProvider.GetService<IMediator>();
    mediator.Should().NotBeNull();
  }

  public void Generic_IRequestHandler_GetWeatherForecastsRequest_GetWeatherForecastsResponse()
  {
    IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse> handler = ServiceProvider.GetService<IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse>>();
    handler.Should().NotBeNull();
  }

  public void IConfiguration()
  {
    IConfiguration configuration = ServiceProvider.GetService<IConfiguration>();
    configuration.Should().NotBeNull();
  }
}
