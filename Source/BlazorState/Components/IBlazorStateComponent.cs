namespace BlazorState
{
  using MediatR;

  /// <summary>
  /// Minimum implementation needed for BlazorState to function
  /// </summary>
  /// <example>
  ///   public class BlazorStateComponent : BlazorComponent,
  ///     IBlazorStateComponent
  ///  {
  ///    [Inject] public IMediator Mediator { get; set; }
  ///    [Inject] public IStore Store { get; set; }
  ///    public void ReRender() => StateHasChanged();
  /// }
  /// </example>
  public interface IBlazorStateComponent // TODO: evaluate if this interface is even needed
  {
    IMediator Mediator { get; set; }
    IStore Store { get; set; }

    void OnStateSet();
    void ReRender();
  }
}