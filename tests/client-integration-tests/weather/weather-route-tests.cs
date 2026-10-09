#region Purpose
// Proves GetWeatherForecasts.Query.GetRoute sends Days on the weather URL.
#endregion

#region Design
// Constructs the contract query directly. The server endpoint reads that query value.
#endregion

namespace WeatherRouteTests;

public class GetRoute_Should
{
  public static void Include_Days()
  {
    Query query = new()
    {
      Days = 10
    };

    query.GetRoute().ShouldBe("api/weather?Days=10");
  }
}
