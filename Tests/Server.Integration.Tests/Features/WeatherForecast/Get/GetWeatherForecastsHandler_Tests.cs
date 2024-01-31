namespace GetWeatherForecastsHandler_;

public class Handle_Returns : BaseTest
{
  private readonly GetWeatherForecastsRequest GetWeatherForecastsRequest;

  public Handle_Returns
  (
    WebApplicationFactory<Startup> aWebApplicationFactory,
    JsonSerializerOptions aJsonSerializerOptions
  ) : base(aWebApplicationFactory, aJsonSerializerOptions)
  {
    GetWeatherForecastsRequest = new GetWeatherForecastsRequest { Days = 10 };
  }

  public async Task _10WeatherForecasts_Given_10DaysRequested()
  {
    GetWeatherForecastsResponse getWeatherForecastsResponse = await Send(GetWeatherForecastsRequest);

    ValidateGetWeatherForecastsResponse(getWeatherForecastsResponse);
  }

  private void ValidateGetWeatherForecastsResponse(GetWeatherForecastsResponse aGetWeatherForecastsResponse)
  {
    aGetWeatherForecastsResponse.CorrelationId.Should().Be(GetWeatherForecastsRequest.CorrelationId);
    aGetWeatherForecastsResponse.WeatherForecasts.Count.Should().Be(GetWeatherForecastsRequest.Days);
    aGetWeatherForecastsResponse.WeatherForecasts.Count.Should().Be(GetWeatherForecastsRequest.Days);
  }

}
