#region Purpose
// Base for test-app input components, exposing common states through GetState<T>.
#endregion

#region Design
// Inherits TimeWarpStateInputComponent<T> so input components (CustomInput) get state access and subscriptions while
// keeping InputBase<T> binding and validation.
#endregion

// ReSharper disable UnusedMember.Global
namespace Test.App.Client.Features.Base.Components;

/// <summary>
/// Makes access to the State a little easier for test-app input components.
/// </summary>
public abstract class BaseInputComponent<T> : TimeWarpStateInputComponent<T>
{
  internal ApplicationState ApplicationState => GetState<ApplicationState>();
  internal CounterState CounterState => GetState<CounterState>();
  internal EventStreamState EventStreamState => GetState<EventStreamState>();
  internal WeatherForecastsState WeatherForecastsState => GetState<WeatherForecastsState>();
  internal ThemeState ThemeState => GetState<ThemeState>();
}
