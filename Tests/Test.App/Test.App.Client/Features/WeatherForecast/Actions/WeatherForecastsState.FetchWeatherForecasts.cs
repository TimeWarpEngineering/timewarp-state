namespace Test.App.Client.Features.WeatherForecast;

using static Contracts.Features.WeatherForecast.GetWeatherForecasts;

internal sealed partial class WeatherForecastsState
{
  public static class FetchWeatherForecasts
  {
    public class Action : IAction;

    [UsedImplicitly]
    public class Handler
    (
      IStore store,
      HttpClient httpClient
    ) : BaseActionHandler<Action>(store)
    {
      private readonly HttpClient HttpClient = httpClient;

      public override async Task Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        var query = new Query()
        {
          Days = 10
        };

        Response? getWeatherForecastsResponse =
          await HttpClient.GetFromJsonAsync<Response>
          (
            query.GetRoute(),
            cancellationToken: cancellationToken
          );

        ArgumentNullException.ThrowIfNull(getWeatherForecastsResponse);

        WeatherForecastsState.WeatherForecastList = getWeatherForecastsResponse;
      }
    }
  }
}
