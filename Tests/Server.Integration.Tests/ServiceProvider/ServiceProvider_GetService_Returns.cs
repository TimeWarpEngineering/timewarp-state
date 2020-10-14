namespace ServiceProvider
{
  using MediatR;
  using Microsoft.AspNetCore.Mvc.Testing;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using System.Text.Json;
  using TestApp.Api.Features.WeatherForecast;
  using TestApp.Server;
  using TestApp.Server.Features.WeatherForecast;
  using TestApp.Server.Integration.Tests.Infrastructure;

  public class GetService_Returns : BaseTest
  {
    private readonly IServiceProvider ServiceProvider;

    public GetService_Returns
    (
      WebApplicationFactory<Startup> aWebApplicationFactory,
      JsonSerializerOptions aJsonSerializerOptions
    ) : base(aWebApplicationFactory, aJsonSerializerOptions)
    {
      ServiceProvider = ServiceScopeFactory.CreateScope().ServiceProvider;
    }
    public void IMediator()
    {
      IMediator mediator = ServiceProvider.GetService<IMediator>();
      mediator.ShouldNotBeNull();
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