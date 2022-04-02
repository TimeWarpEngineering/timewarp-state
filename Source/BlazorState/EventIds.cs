namespace BlazorState;

using Microsoft.Extensions.Logging;

internal class EventIds
{
  // Features - Normal
  //   JavascriptInterop
  //     JsonRequestHandler
  public static readonly EventId JsonRequestHandler_Initializing = new(200, nameof(JsonRequestHandler_Initializing));
  public static readonly EventId JsonRequestReceived = new(201, nameof(JsonRequestReceived));
  public static readonly EventId JsonRequestHandled = new(202, nameof(JsonRequestReceived));
  public static readonly EventId JsonRequestOfInvalidType = new(203, nameof(JsonRequestOfInvalidType));
  //   Routing
  // Pipeline - Middleware
  //   CloneState
  public static readonly EventId CloneStateBehavior_Initializing = new(400, nameof(CloneStateBehavior_Initializing));
  public static readonly EventId CloneStateBehavior_Cloning = new(401, nameof(CloneStateBehavior_Cloning));
  public static readonly EventId CloneStateBehavior_Ignoring = new(402, nameof(CloneStateBehavior_Ignoring));
  public static readonly EventId CloneStateBehavior_Exception = new(403, nameof(CloneStateBehavior_Exception));
  public static readonly EventId CloneStateBehavior_Restored = new(404, nameof(CloneStateBehavior_Restored));

  //   ReduxDevTools
  public static readonly EventId StartHandler_Initializing = new(500, nameof(StartHandler_Initializing));
  public static readonly EventId StartHandler_RequestReceived = new(501, nameof(StartHandler_RequestReceived));
  public static readonly EventId StartHandler_RequestHandled = new(502, nameof(StartHandler_RequestHandled));

  public static readonly EventId CommitHandler_Initializing = new(500, nameof(CommitHandler_Initializing));
  public static readonly EventId CommitHandler_RequestReceived = new(501, nameof(CommitHandler_RequestReceived));
  public static readonly EventId CommitHandler_RequestHandled = new(502, nameof(CommitHandler_RequestHandled));

  public static readonly EventId JumpToStateHandler_Initializing = new(510, nameof(JumpToStateHandler_Initializing));
  public static readonly EventId JumpToStateHandler_RequestReceived = new(511, nameof(JumpToStateHandler_RequestReceived));
  public static readonly EventId JumpToStateHandler_RequestHandled = new(512, nameof(JumpToStateHandler_RequestHandled));

  public static readonly EventId ReduxDevToolsInterop_Initializing = new(520, nameof(ReduxDevToolsInterop_Initializing));
  public static readonly EventId ReduxDevToolsInterop_DispatchingInit = new(521, nameof(ReduxDevToolsInterop_DispatchingInit));
  public static readonly EventId ReduxDevToolsInterop_DispatchingRequest = new(522, nameof(ReduxDevToolsInterop_DispatchingRequest));

  public static readonly EventId ReduxDevToolsPostProcessor_Constructing = new(530, nameof(ReduxDevToolsPostProcessor_Constructing));
  public static readonly EventId ReduxDevToolsPostProcessor_Begin = new(531, nameof(ReduxDevToolsPostProcessor_Begin));
  public static readonly EventId ReduxDevToolsPostProcessor_End = new(532, nameof(ReduxDevToolsPostProcessor_End));
  public static readonly EventId ReduxDevToolsPostProcessor_Exception = new(533, nameof(ReduxDevToolsPostProcessor_Exception));

  //   RenderSubscriptions
  public static readonly EventId RenderSubscriptionsPostProcessor_Constructing = new(600, nameof(ReduxDevToolsPostProcessor_Constructing));
  public static readonly EventId RenderSubscriptionsPostProcessor_Begin = new(601, nameof(ReduxDevToolsPostProcessor_Constructing));
  public static readonly EventId RenderSubscriptionsPostProcessor_End = new(602, nameof(ReduxDevToolsPostProcessor_Constructing));
  public static readonly EventId RenderSubscriptionsPostProcessor_Exception = new(603, nameof(ReduxDevToolsPostProcessor_Constructing));

  // Store - Blazor State Specific
  public static readonly EventId Store_Initializing = new(100, nameof(Store_Initializing));
  public static readonly EventId Store_GetState = new(101, nameof(Store_GetState));
  public static readonly EventId Store_CreateState = new(102, nameof(Store_CreateState));
  public static readonly EventId Store_SetState = new(103, nameof(Store_SetState));

  //Store.ReduxDevTools
  public static readonly EventId LoadStatesFromJson = new(104, nameof(LoadStatesFromJson));
  public static readonly EventId LoadStateFromJson = new(104, nameof(LoadStateFromJson));

  // Subscriptions - Blazor State Specific
  public static readonly EventId Subscriptions_Initializing = new(300, nameof(Subscriptions_Initializing));
  public static readonly EventId Subscriptions_Adding = new(301, nameof(Subscriptions_Adding));
  public static readonly EventId Subscriptions_RemovingComponentSubscriptions = new(302, nameof(Subscriptions_RemovingComponentSubscriptions));
  public static readonly EventId Subscriptions_ReRenderingSubscribers = new(303, nameof(Subscriptions_ReRenderingSubscribers));
  public static readonly EventId Subscriptions_RemoveSubscription = new(304, nameof(Subscriptions_RemoveSubscription));
}
