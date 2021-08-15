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
  public interface IBlazorStateComponent
  {
    string Id { get; }
    IMediator Mediator { get; set; }
    IStore Store { get; set; }

    void ReRender();
  }
}