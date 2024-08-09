namespace TimeWarp.State;

internal class EventIds
{
  // Store - TimeWarp State Specific
  public static readonly EventId Store_Constructing = new(100, nameof(Store_Constructing));
  public static readonly EventId Store_Initializing = new(101, nameof(Store_Initializing));
  public static readonly EventId Store_GetState = new(102, nameof(Store_GetState));
  public static readonly EventId Store_CreateState = new(103, nameof(Store_CreateState));
  public static readonly EventId Store_SetState = new(104, nameof(Store_SetState));
  public static readonly EventId Store_RemoveState = new (105, nameof(Store_RemoveState));

  // Store.ReduxDevTools
  public static readonly EventId LoadStatesFromJson = new(104, nameof(LoadStatesFromJson));
  public static readonly EventId LoadStateFromJson = new(105, nameof(LoadStateFromJson));
  
  // Features - Normal
  //   JavascriptInterop
  //     JsonRequestHandler
  public static readonly EventId JsonRequestHandler_Initializing = new(200, nameof(JsonRequestHandler_Initializing));
  public static readonly EventId JsonRequestReceived = new(201, nameof(JsonRequestReceived));
  public static readonly EventId JsonRequestHandled = new(202, nameof(JsonRequestReceived));
  public static readonly EventId JsonRequestOfInvalidType = new(203, nameof(JsonRequestOfInvalidType));
  
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

  //   ReduxDevTools
  public static readonly EventId StartHandler_Initializing = new(500, nameof(StartHandler_Initializing));
  public static readonly EventId StartHandler_RequestReceived = new(501, nameof(StartHandler_RequestReceived));
  public static readonly EventId StartHandler_RequestHandled = new(502, nameof(StartHandler_RequestHandled));

  public static readonly EventId JumpToStateHandler_Initializing = new(510, nameof(JumpToStateHandler_Initializing));
  public static readonly EventId JumpToStateHandler_RequestReceived = new(511, nameof(JumpToStateHandler_RequestReceived));
  public static readonly EventId JumpToStateHandler_RequestHandled = new(512, nameof(JumpToStateHandler_RequestHandled));

  public static readonly EventId ReduxDevToolsInterop_Initializing = new(520, nameof(ReduxDevToolsInterop_Initializing));
  public static readonly EventId ReduxDevToolsInterop_DispatchingInit = new(521, nameof(ReduxDevToolsInterop_DispatchingInit));
  public static readonly EventId ReduxDevToolsInterop_DispatchingRequest = new(522, nameof(ReduxDevToolsInterop_DispatchingRequest));

  public static readonly EventId ReduxDevToolsBehavior_Constructing = new(530, nameof(ReduxDevToolsBehavior_Constructing));
  public static readonly EventId ReduxDevToolsBehavior_Begin = new(531, nameof(ReduxDevToolsBehavior_Begin));
  public static readonly EventId ReduxDevToolsBehavior_End = new(532, nameof(ReduxDevToolsBehavior_End));
  public static readonly EventId ReduxDevToolsBehavior_Exception = new(533, nameof(ReduxDevToolsBehavior_Exception));

  public static readonly EventId CommitHandler_Initializing = new(540, nameof(CommitHandler_Initializing));
  public static readonly EventId CommitHandler_RequestReceived = new(541, nameof(CommitHandler_RequestReceived));
  public static readonly EventId CommitHandler_RequestHandled = new(542, nameof(CommitHandler_RequestHandled));

  //   RenderSubscriptions
  public static readonly EventId RenderSubscriptionsPostProcessor_Constructing = new(600, nameof(ReduxDevToolsBehavior_Constructing));
  public static readonly EventId RenderSubscriptionsPostProcessor_Begin = new(601, nameof(ReduxDevToolsBehavior_Constructing));
  public static readonly EventId RenderSubscriptionsPostProcessor_End = new(602, nameof(ReduxDevToolsBehavior_Constructing));
  public static readonly EventId RenderSubscriptionsPostProcessor_Exception = new(603, nameof(ReduxDevToolsBehavior_Constructing));
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
