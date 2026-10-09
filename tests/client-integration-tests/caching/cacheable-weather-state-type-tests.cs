#region Purpose
// Proves CacheableWeatherState is IState of itself, so Hydrate returns CacheableWeatherState.
#endregion

#region Design
// Reflection only: no host. TimeWarpCacheableState<TState> derives from State<TState>, so the closed
// CacheableWeatherState inherits Hydrate's return type from that argument.
#endregion

namespace CacheableStateTests;

public class CacheableWeatherState_Type_Should
{
  public static void Implement_IState_Of_Itself()
  {
    typeof(IState<CacheableWeatherState>).IsAssignableFrom(typeof(CacheableWeatherState)).ShouldBeTrue();
  }

  public static void Hydrate_Should_Return_CacheableWeatherState()
  {
    MethodInfo? hydrate = typeof(CacheableWeatherState).GetMethod(nameof(CacheableWeatherState.Hydrate));

    hydrate.ShouldNotBeNull();
    hydrate.ReturnType.ShouldBe(typeof(CacheableWeatherState));
  }
}
