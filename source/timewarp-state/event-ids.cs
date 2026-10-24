#region Purpose
// Central list of logging EventIds used across TimeWarp.State.
#endregion

#region Design
// One internal static class grouped by area (Store, JavaScript interop, Subscriptions, behaviors, component), with
// numeric ranges per area so log filtering by EventId stays stable.
#endregion

namespace TimeWarp.State;

internal static class EventIds
{
  // Store - TimeWarp State Specific
  public static readonly EventId Store_Constructing = new(100, nameof(Store_Constructing));
  public static readonly EventId Store_Initializing = new(101, nameof(Store_Initializing));
  public static readonly EventId Store_GetState = new(102, nameof(Store_GetState));
  public static readonly EventId Store_CreateState = new(103, nameof(Store_CreateState));
  public static readonly EventId Store_SetState = new(104, nameof(Store_SetState));
  public static readonly EventId Store_RemoveState = new (105, nameof(Store_RemoveState));

  // Features - Normal
  //   JavascriptInterop
  //     JsonRequestHandler
  public static readonly EventId JsonRequestHandler_Initializing = new(200, nameof(JsonRequestHandler_Initializing));
  public static readonly EventId JsonRequestReceived = new(201, nameof(JsonRequestReceived));
  public static readonly EventId JsonRequestHandled = new(202, nameof(JsonRequestReceived));
  public static readonly EventId JsonRequestOfInvalidType = new(203, nameof(JsonRequestOfInvalidType));
  public static readonly EventId JsonRequestInvalidPayload = new(204, nameof(JsonRequestInvalidPayload));
  
  // Subscriptions - TimeWarp State Specific
  public static readonly EventId Subscriptions_Initializing = new(300, nameof(Subscriptions_Initializing));
  public static readonly EventId Subscriptions_Adding = new(301, nameof(Subscriptions_Adding));
  public static readonly EventId Subscriptions_RemovingComponentSubscriptions = new(302, nameof(Subscriptions_RemovingComponentSubscriptions));
  public static readonly EventId Subscriptions_ReRenderingSubscribers = new(303, nameof(Subscriptions_ReRenderingSubscribers));
  public static readonly EventId Subscriptions_RemoveSubscription = new(304, nameof(Subscriptions_RemoveSubscription));
  
  //   Routing
  // Pipeline - Middleware
  //   StateTransaction
  public static readonly EventId StateTransactionBehavior_Constructing = new(400, nameof(StateTransactionBehavior_Constructing));
  public static readonly EventId StateTransactionBehavior_Cloning = new(401, nameof(StateTransactionBehavior_Cloning));
  public static readonly EventId StateTransactionBehavior_Ignoring = new(402, nameof(StateTransactionBehavior_Ignoring));
  public static readonly EventId StateTransactionBehavior_Exception = new(403, nameof(StateTransactionBehavior_Exception));
  public static readonly EventId StateTransactionBehavior_Restoring = new(404, nameof(StateTransactionBehavior_Restoring));
  public static readonly EventId StateTransactionBehavior_ConcurrentAdvance = new(405, nameof(StateTransactionBehavior_ConcurrentAdvance));

  //   RenderSubscriptions
  public static readonly EventId RenderSubscriptionsPostProcessor_Constructing = new(600, nameof(RenderSubscriptionsPostProcessor_Constructing));
  public static readonly EventId RenderSubscriptionsPostProcessor_Begin = new(601, nameof(RenderSubscriptionsPostProcessor_Begin));
  public static readonly EventId RenderSubscriptionsPostProcessor_End = new(602, nameof(RenderSubscriptionsPostProcessor_End));
  public static readonly EventId RenderSubscriptionsPostProcessor_Exception = new(603, nameof(RenderSubscriptionsPostProcessor_Exception));
  public static readonly EventId RenderSubscriptionsPostProcessor_SkippedReRender = new(604, nameof(RenderSubscriptionsPostProcessor_SkippedReRender));
  
  #region TimeWarpStateComponent
  public static readonly EventId TimeWarpStateComponent_Constructed = new(700, nameof(TimeWarpStateComponent_Constructed));
  // SetParametersAsync begin
  public static readonly EventId TimeWarpStateComponent_CheckComplexParameter = new(701, nameof(TimeWarpStateComponent_CheckComplexParameter));
  public static readonly EventId TimeWarpStateComponent_ComplexParameterChanged = new(702, nameof(TimeWarpStateComponent_ComplexParameterChanged));
  public static readonly EventId TimeWarpStateComponent_ParameterChanged = new(703, nameof(TimeWarpStateComponent_ParameterChanged));
  // SetParametersAsync end
  // State Change Subscriptions
  public static readonly EventId TimeWarpStateComponent_ShouldReRender = new(704, nameof(TimeWarpStateComponent_ShouldReRender));
  // State Change Subscriptions End
  public static readonly EventId TimeWarpStateComponent_ShouldRender = new(705, nameof(TimeWarpStateComponent_Disposing));
  public static readonly EventId TimeWarpStateComponent_OnAfterRender = new(706, nameof(TimeWarpStateComponent_OnAfterRender));
  public static readonly EventId TimeWarpStateComponent_Disposing = new(707, nameof(TimeWarpStateComponent_Disposing));
  #endregion
  
  // StateInitializationPreProcessor
  public static readonly EventId StateInitializationPreProcessor_Waiting = new(800, nameof(StateInitializationPreProcessor_Waiting));
  public static readonly EventId StateInitializationPreProcessor_Completed = new(801, nameof(StateInitializationPreProcessor_Completed));

}
