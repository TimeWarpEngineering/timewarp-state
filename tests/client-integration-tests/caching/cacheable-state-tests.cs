namespace CacheableStateTests;

using Test.App.Client.Features.WeatherForecast;

/// <summary>
/// Integration tests for TimeWarpCacheableState caching behavior.
/// Uses CacheableWeatherState from test-app.
/// </summary>
public class CacheableState_Should : BaseTest
{
  public CacheableState_Should(ClientHost clientHost) : base(clientHost) { }

  private CacheableWeatherState CacheableWeatherState => Store.GetState<CacheableWeatherState>();

  public void HaveNullCacheKey_Initially()
  {
    // Arrange - ensure fresh state
    Store.RemoveState<CacheableWeatherState>();

    // Act
    CacheableWeatherState state = Store.GetState<CacheableWeatherState>();

    // Assert
    state.CacheKey.ShouldBeNull();
    state.TimeStamp.ShouldBeNull();
  }

  public async Task SetCacheKey_AfterFetch()
  {
    // Arrange
    Store.RemoveState<CacheableWeatherState>();
    CacheableWeatherState.CacheKey.ShouldBeNull();

    // Act
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());

    // Assert - cache key should be set based on action type
    CacheableWeatherState.CacheKey.ShouldNotBeNull();
    CacheableWeatherState.CacheKey.ShouldContain("FetchWeatherForecastsActionSet");
  }

  public async Task SetTimestamp_AfterFetch()
  {
    // Arrange
    Store.RemoveState<CacheableWeatherState>();
    DateTime beforeFetch = DateTime.UtcNow;

    // Act
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());

    // Assert - timestamp should be set to approximately now
    CacheableWeatherState.TimeStamp.ShouldNotBeNull();
    CacheableWeatherState.TimeStamp!.Value.ShouldBeGreaterThanOrEqualTo(beforeFetch);
    CacheableWeatherState.TimeStamp!.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
  }

  public async Task ReturnCachedData_WhenCacheValid()
  {
    // Arrange
    Store.RemoveState<CacheableWeatherState>();
    
    // First fetch to populate cache
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());
    
    string? firstCacheKey = CacheableWeatherState.CacheKey;
    DateTime? firstTimestamp = CacheableWeatherState.TimeStamp;
    var firstForecasts = CacheableWeatherState.WeatherForecasts;
    
    firstCacheKey.ShouldNotBeNull();
    firstTimestamp.ShouldNotBeNull();
    firstForecasts.ShouldNotBeNull();

    // Act - fetch again (should use cache)
    await Send(new CacheableWeatherState.FetchWeatherForecastsActionSet.Action());

    // Assert - cache key and timestamp should remain the same (cache was used)
    CacheableWeatherState.CacheKey.ShouldBe(firstCacheKey);
    CacheableWeatherState.TimeStamp.ShouldBe(firstTimestamp);
  }
}
