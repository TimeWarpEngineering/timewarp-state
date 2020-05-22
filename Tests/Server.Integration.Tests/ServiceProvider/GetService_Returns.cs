namespace TestApp.Server.ServiceProvider_Tests
{
  using MediatR;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using TestApp.Server.Features.WeatherForecast;
  using TestApp.Server.Integration.Tests.Infrastructure;
  using TestApp.Api.Features.WeatherForecast;

  public class GetService_Returns
  {
    private readonly IServiceProvider ServiceProvider;

    public GetService_Returns(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
    }
    public void IMediator()
    {
      IMediator mediator = ServiceProvider.GetService<IMediator>();
      mediator.ShouldNotBeNull();
    }

    public void GetWeatherForecastsHandler()
    {
      GetWeatherForecastsHandler getWeatherForecastsHandler = ServiceProvider.GetService<GetWeatherForecastsHandler>();
      getWeatherForecastsHandler.ShouldNotBeNull();

      IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse> handler = ServiceProvider.GetService<IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse>>();
      handler.ShouldNotBeNull();
    }

    public void Generic_IRequestHandler_GetWeatherForecastsRequest_GetWeatherForecastsResponse()
    {
      IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse> handler = ServiceProvider.GetService<IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse>>();
      handler.ShouldNotBeNull();
    }

    public void IConfiguration()
    {
      IConfiguration configuration = ServiceProvider.GetService<IConfiguration>();
      configuration.ShouldNotBeNull();
    }
  }
}
