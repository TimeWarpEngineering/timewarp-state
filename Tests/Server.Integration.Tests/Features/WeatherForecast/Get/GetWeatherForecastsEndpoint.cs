namespace TestApp.Features.WeatherForecast.GetWeatherForecastsHandler_Tests
{
  using Shouldly;
  using System.Threading.Tasks;
  using TestApp.Server.Integration.Tests.Infrastructure;
  using TestApp.Api.Features.WeatherForecast;
  using FluentAssertions;
  using System.Text.Json;
  using Microsoft.AspNetCore.Mvc.Testing;
  using TestApp.Server;

  internal class Endpoint_Returns : BaseTest
  {
    private readonly GetWeatherForecastsRequest GetWeatherForecastsRequest;

    public Endpoint_Returns
    (
      WebApplicationFactory<Startup> aWebApplicationFactory,
      JsonSerializerOptions aJsonSerializerOptions
    ) : base(aWebApplicationFactory, aJsonSerializerOptions)
    {
      GetWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };
    }

    public async Task _10WeatherForecasts_Given_10DaysRequested()
    {
      GetWeatherForecastsResponse getWeatherForecastsResponse =
        await GetJsonAsync<GetWeatherForecastsResponse>(GetWeatherForecastsRequest.RouteFactory);

      ValidateGetWeatherForecastsResponse(getWeatherForecastsResponse);
    }

    private void ValidateGetWeatherForecastsResponse(GetWeatherForecastsResponse aGetWeatherForecastsResponse)
    {
      aGetWeatherForecastsResponse.RequestId.ShouldBe(GetWeatherForecastsRequest.Id);
      aGetWeatherForecastsResponse.RequestId.Should().Be(GetWeatherForecastsRequest.Id);
      aGetWeatherForecastsResponse.WeatherForecasts.Count.ShouldBe(GetWeatherForecastsRequest.Days);
      aGetWeatherForecastsResponse.WeatherForecasts.Count.Should().Be(GetWeatherForecastsRequest.Days);
    }

  }
}