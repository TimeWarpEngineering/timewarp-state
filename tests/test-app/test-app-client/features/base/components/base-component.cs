#region Purpose
// Base component for test-app pages, exposing each test state through GetState<T>.
#endregion

#region Design
// Builds the @RenderModeDisplay fragment the E2E tests read, on top of TimeWarpStateComponent. Every property
// subscribes the component to that state's changes, except NoSubRouteState, which uses placeSubscription: false.
#endregion

namespace Test.App.Client.Features.Base.Components;

using CloneTest;
using Microsoft.AspNetCore.Components.Rendering;

/// <summary>
/// Makes access to the State a little easier and exposes the render-mode fragment the test pages render.
/// </summary>
public abstract class BaseComponent : TimeWarpStateComponent
{
  protected readonly RenderFragment RenderModeDisplay;

  protected string RenderModeDisplayString => $"RendererInfo.Name: {RendererInfo.Name}\nAssignedRenderMode: {AssignedRenderMode?.GetType().Name ?? "None"}\nRendererInfo.IsInteractive: {RendererInfo.IsInteractive}";

  protected BaseComponent()
  {
    RenderModeDisplay = builder =>
    {
      builder.OpenComponent<TimeWarp.Features.Developer.RenderModeDisplay>(0);
      builder.AddComponentParameter(1, nameof(TimeWarp.Features.Developer.RenderModeDisplay.RendererInfoName), RendererInfo.Name);
      builder.AddComponentParameter(2, nameof(TimeWarp.Features.Developer.RenderModeDisplay.AssignedRenderModeName), AssignedRenderMode?.GetType().Name ?? "None");
      builder.AddComponentParameter(3, nameof(TimeWarp.Features.Developer.RenderModeDisplay.RendererInfoIsInteractive), RendererInfo.IsInteractive);
      builder.CloseComponent();
    };
  }

  protected ApplicationState ApplicationState => GetState<ApplicationState>();
  internal BlueState BlueState => GetState<BlueState>();
  internal CounterState CounterState => GetState<CounterState>();
  internal CloneableState CloneableState => GetState<CloneableState>();
  internal EventStreamState EventStreamState => GetState<EventStreamState>();
  internal PurpleState PurpleState => GetState<PurpleState>();
  internal ActionTrackingState ActionTrackingState => GetState<ActionTrackingState>();
  internal RouteState RouteState => GetState<RouteState>();
  internal RouteState NoSubRouteState => GetState<RouteState>(placeSubscription: false);
  internal ThemeState ThemeState => GetState<ThemeState>();
  internal WeatherForecastsState WeatherForecastsState => GetState<WeatherForecastsState>();
  internal CacheableWeatherState CacheableWeatherState => GetState<CacheableWeatherState>();

  // ReSharper disable once UnusedMember.Global - Can be used by inheriting classes not in this library.
  protected Task Send(IRequest request) => Sender.Send(request);
}
