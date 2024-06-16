namespace TimeWarp.Features.Routing;

/// <summary>
/// A component that Sends RouteState.PushRouteInfo.Action for every OnAfterRenderAsync.
/// </summary>
/// <remarks>
/// <para>
/// Recommended: It is recommended to call 
/// <c>await Sender.Send(new RouteState.PushRouteInfo.Action());</c> 
/// in the <c>OnAfterRenderAsync</c> method of the consuming page component.
/// This reduces the number of times this action is fired, improving performance.
/// If you implement the recommended approach, you do not need to include this <c>TimeWarpPageRenderNotifier</c> component.
/// </para>
/// <para>
/// This component uses a nonce to ensure the component is always re-rendered.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;TimeWarpPageRenderNotifier @rendermode=InteractiveAuto Nonce=@Guid.NewGuid() /&gt;
/// </code>
/// </example>
public class TimeWarpPageRenderNotifier : ComponentBase
{
  [Inject] private ISender Sender { get; set; } = null!;
    
  /// <summary>
  /// The Nonce is used to ensure that the correct page title is set.
  /// Will reload this component on every page render.
  /// </summary>
  [Parameter, EditorRequired] public Guid? Nonce { get; set; }

  /// <inheritdoc />
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    await Sender.Send(new RouteState.PushRouteInfo.Action());
  }
}
