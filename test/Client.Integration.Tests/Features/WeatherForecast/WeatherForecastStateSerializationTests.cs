namespace TestApp.Client.Integration.Tests.Features.WeatherForecast
{
  using System;
  using BlazorState;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using System.Collections.Generic;
  using TestApp.Client.Features.WeatherForecast;
  using TestApp.Shared.Features.WeatherForecast;
  using AnyClone;
  using System.Text.Json;

  internal class WeatherForecastStateSerializationTests
  {

    public class Person
    {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public DateTime? BirthDay { get; set; }
    }

    public void ShouldSerializeAndParseSample()
    {
      var jsonSerializerOptions = new JsonSerializerOptions();
      Person person = new Person { FirstName = "Steve", LastName = "Cramer", BirthDay = new DateTime(1967, 09, 27) };
      string json = JsonSerializer.Serialize(person, jsonSerializerOptions);
      Person parsed = JsonSerializer.Deserialize<Person>(json, jsonSerializerOptions);
      parsed.BirthDay.ShouldBe(person.BirthDay);
      parsed.FirstName.ShouldBe(person.FirstName);
      parsed.LastName.ShouldBe(person.LastName);
    }

    public void ShouldSerializeAndParse()
    {
      //Arrange
      var jsonSerializerOptions = new JsonSerializerOptions{ PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
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
      weatherForecastDto.TemperatureC.ShouldBe(parsed.TemperatureC);
      weatherForecastDto.Summary.ShouldBe(parsed.Summary);
      weatherForecastDto.Date.ShouldBe(parsed.Date);
    }
  }
}
