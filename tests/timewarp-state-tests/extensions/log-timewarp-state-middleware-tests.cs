#region Purpose
// LogTimeWarpStateMiddleware lists manifest version 2 requests and behavior simple names in pipeline order.
#endregion

#region Design
// The service collection holds a Dictionary and an open-generic IPipelineBehavior registration as decoys.
// 15.0 does not register IPipelineBehavior<,>, so those descriptors must not appear. Order comes from the
// JSON the test passes, including a repeated behavior that stays one name. GetComponentOrder returns that
// list only for IPipelineBehavior<,>.
#endregion

namespace LogTimeWarpStateMiddlewareTests;

public class Should_
{
  public static void List_Manifest_Behaviors_In_Pipeline_Order()
  {
    const string ManifestJson =
      """
      {
        "version": 2,
        "namespace": "LogTimeWarpStateMiddlewareTests",
        "requests": [
          {
            "request": "TouchAction",
            "kind": "Action",
            "scope": "ClientPipeline",
            "handler": "TouchHandler",
            "behaviors": [
              "TimeWarp.State.StateTransactionBehavior<TouchAction, int>",
              "TimeWarp.State.StateInitializationPreProcessor<TouchAction, int>",
              "RegistrationProbeBehavior<TouchAction, int>"
            ]
          },
          {
            "request": "OtherAction",
            "kind": "Query",
            "handler": "OtherHandler",
            "behaviors": [
              "TimeWarp.State.StateTransactionBehavior<OtherAction, Unit>"
            ]
          }
        ],
        "notifications": [],
        "membership": []
      }
      """;

    ServiceCollection services = new();
    services.AddTransient<Dictionary<string, int>>();
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StateTransactionBehavior<,>));

    CaptureLogger logger = new();
    services.LogTimeWarpStateMiddleware(logger, ManifestJson);
    services.GetComponentOrder(typeof(ILogger)).ShouldBeEmpty();
    List<string> order = services.GetComponentOrder(typeof(IPipelineBehavior<,>));
    order.ShouldBe
    (
      [
        "StateTransactionBehavior",
        "StateInitializationPreProcessor",
        "RegistrationProbeBehavior"
      ]
    );

    string message = logger.Message.ShouldNotBeNull();
    message.ShouldContain("TimeWarp State (TimeWarp.Mediator) Pipeline Behavior Registrations:");
    message.ShouldContain("MediatorManifest version 2");
    message.ShouldContain("Requests:");
    message.ShouldContain("1. TouchAction (kind Action, scope ClientPipeline)");
    message.ShouldContain("2. OtherAction (kind Query, scope unscoped)");
    message.ShouldContain("Behaviors (in pipeline order):");
    message.ShouldContain("1. StateTransactionBehavior");
    message.ShouldContain("2. StateInitializationPreProcessor");
    message.ShouldContain("3. RegistrationProbeBehavior");
    message.ShouldNotContain("Dictionary");
    message.ShouldNotContain("profile");
  }

  private sealed class CaptureLogger : ILogger
  {
    public string? Message { get; private set; }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>
    (
      LogLevel logLevel,
      EventId eventId,
      TState state,
      Exception? exception,
      Func<TState, Exception?, string> formatter
    )
    {
      Message = formatter(state, exception);
    }
  }
}
