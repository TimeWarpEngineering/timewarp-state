namespace WeatherForecastsState_;

public class Clone_Should : BaseTest
{
  public Clone_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
  {
    WeatherForecastsState = Store.GetState<WeatherForecastsState>();
  }

  private WeatherForecastsState WeatherForecastsState { get; set; }

  public void Clone()
  {
    //Arrange
    var weatherForecasts = new List<WeatherForecastDto> {
        new WeatherForecastDto
        (
          aDate: DateTime.MinValue,
          aSummary: "Summary 1",
          aTemperatureC: 24
        ),
        new WeatherForecastDto
        (
          aDate: DateTime.MinValue,
          aSummary: "Summary 1",
          aTemperatureC: 24
        ),
      };
    WeatherForecastsState.Initialize(weatherForecasts);

    //Act
    var clone = WeatherForecastsState.Clone() as WeatherForecastsState;

    //Assert
    WeatherForecastsState.Should().NotBeSameAs(clone);
    WeatherForecastsState.WeatherForecasts.Count.Should().Be(clone.WeatherForecasts.Count);
    WeatherForecastsState.Guid.Should().NotBe(clone.Guid);
    WeatherForecastsState.WeatherForecasts[0].TemperatureC.Should().Be(clone.WeatherForecasts[0].TemperatureC);
    WeatherForecastsState.WeatherForecasts[0].Should().NotBe(clone.WeatherForecasts[0]);
  }

}
