namespace WeatherForecastDto_;

public class Should
{
  public void SerializeAndDeserialize()
  {
    //Arrange
    var jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    var weatherForecastDto = new WeatherForecastDto
    (
      aDate: DateTime.MinValue.ToUniversalTime(),
      aSummary: "Summary 1",
      aTemperatureC: 24
    );

    string json = JsonSerializer.Serialize(weatherForecastDto, jsonSerializerOptions);

    //Act
    WeatherForecastDto parsed = JsonSerializer.Deserialize<WeatherForecastDto>(json, jsonSerializerOptions);

    //Assert
    weatherForecastDto.TemperatureC.Should().Be(parsed.TemperatureC);
    weatherForecastDto.Summary.Should().Be(parsed.Summary);
    weatherForecastDto.Date.Should().Be(parsed.Date);
  }
}
