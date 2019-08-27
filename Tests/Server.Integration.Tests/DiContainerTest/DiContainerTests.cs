namespace TestApp.Server.Integration.Tests.DiContainerTest
{
  using MediatR;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using TestApp.Server.Features.WeatherForecast;
  using TestApp.Server.Integration.Tests.Infrastructure;
  using TestApp.Api.Features.WeatherForecast;

  public class DiContainerTests
  {
    private readonly IServiceProvider ServiceProvider;

    public DiContainerTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      
    }
    public void ShouldGetServiceForMediator()
    {
      IMediator mediator = ServiceProvider.GetService<IMediator>();
      mediator.ShouldNotBeNull();

      GetWeatherForecastsHandler getWeatherForecastsHandler = ServiceProvider.GetService<GetWeatherForecastsHandler>();
      getWeatherForecastsHandler.ShouldNotBeNull();

      IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse> handler = ServiceProvider.GetService<IRequestHandler<GetWeatherForecastsRequest, GetWeatherForecastsResponse>>();
      handler.ShouldNotBeNull();

      // Something in preview 8 indirectly requires this interface.
      IConfiguration configuration = ServiceProvider.GetService<IConfiguration>();
      configuration.ShouldNotBeNull();

    }

  }


}
