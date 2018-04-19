﻿using BlazorState.Behaviors.ReduxDevTools;
using BlazorState.Store;
using MediatR;

namespace BlazorState.Components
{
  /// <summary>
  /// Minimum implementation needed for BlazorState to function
  /// </summary>
  /// <example>
  ///   public class BlazorStateComponent : BlazorComponent,
  ///     IBlazorStateComponent
  ///  {
  ///    [Inject] public IMediator Mediator { get; set; }
  ///    [Inject] public IStore Store { get; set; }
  //// }
  /// </example>
public interface IBlazorStateComponent
  {
    IMediator Mediator { get; set; }
    IStore Store { get; set; }
  }
}
