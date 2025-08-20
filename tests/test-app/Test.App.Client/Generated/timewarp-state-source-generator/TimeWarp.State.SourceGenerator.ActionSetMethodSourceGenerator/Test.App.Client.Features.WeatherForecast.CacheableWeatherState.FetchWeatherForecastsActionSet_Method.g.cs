#nullable enable

#pragma warning disable CS1591
namespace Test.App.Client.Features.WeatherForecast;

using System.Threading;
using System.Threading.Tasks;

partial class CacheableWeatherState
{
    public async Task FetchWeatherForecasts(CancellationToken? externalCancellationToken = null)
    {
        using CancellationTokenSource? linkedCts = externalCancellationToken.HasValue
            ? CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken.Value, CancellationToken)
            : null;

        await Sender.Send
        (
            new FetchWeatherForecastsActionSet.Action(),
            linkedCts?.Token ?? CancellationToken
        );
    }
}
#pragma warning restore CS1591