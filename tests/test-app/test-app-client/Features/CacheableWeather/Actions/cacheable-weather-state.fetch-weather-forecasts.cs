namespace Test.App.Client.Features.WeatherForecast;

using static Contracts.Features.WeatherForecast.GetWeatherForecasts;

public partial class CacheableWeatherState
{
  public static class FetchWeatherForecastsActionSet
  {
    internal sealed class Action : IAction;
    
    internal sealed class Handler : BaseActionHandler<Action>
    {
      private readonly HttpClient HttpClient;
      public Handler
      (
        IStore store,
        HttpClient httpClient
      ) : base(store)
      {
        HttpClient = httpClient;
      }

      public override async Task Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        await CacheableWeatherState.HandleWithCaching(action, UpdateStateAsync, cancellationToken);
      }

      private async Task UpdateStateAsync<TAction>(TAction action, CancellationToken cancellationToken)
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

        CacheableWeatherState.WeatherForecastList = getWeatherForecastsResponse;
      }
    }
  }
}
