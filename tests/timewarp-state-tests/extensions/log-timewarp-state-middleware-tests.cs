#region Purpose
// LogTimeWarpStateMiddleware lists closed generated behaviors in service-collection order.
#endregion

#region Design
// StateTransactionBehavior is registered before StateInitializationPreProcessor so the list follows the collection,
// not the attribute order. A second closed registration of the same behavior stays one name. A Dictionary and an
// open-generic IPipelineBehavior registration stay out. RegistrationProbeBehavior proves the test compilation's
// own generator output is visible to GetComponentOrder.
#endregion

namespace LogTimeWarpStateMiddlewareTests;

public class Should_
{
  public static void List_Closed_Behaviors_In_Service_Collection_Order()
  {
    ServiceCollection services = new();
    services.AddScoped<StateTransactionBehavior<TouchAction, int>>();
    services.AddScoped<StateInitializationPreProcessor<TouchAction, int>>();
    services.AddScoped<RegistrationProbeBehavior<TouchAction, int>>();
    services.AddScoped<StateTransactionBehavior<TouchAction, int>>();
    services.AddTransient<Dictionary<string, int>>();
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(StateTransactionBehavior<,>));

    List<string> order = services.GetComponentOrder(typeof(IPipelineBehavior<,>));
    order.ShouldBe
    (
      [
        "StateTransactionBehavior",
        "StateInitializationPreProcessor",
        "RegistrationProbeBehavior"
      ]
    );

    CaptureLogger logger = new();
    services.LogTimeWarpStateMiddleware(logger);
    string message = logger.Message.ShouldNotBeNull();
    message.ShouldContain("TimeWarp State (TimeWarp.Mediator) Pipeline Behavior Registrations:");
    message.ShouldContain("Behaviors (in pipeline order):");
    message.ShouldContain("1. StateTransactionBehavior");
    message.ShouldContain("2. StateInitializationPreProcessor");
    message.ShouldContain("3. RegistrationProbeBehavior");
    message.ShouldNotContain("Dictionary");
  }

  private sealed class TouchAction : IAction;

  [NotTest]
  public sealed class RegistrationProbeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
  {
    public Task<TResponse> Handle
    (
      TRequest request,
      RequestHandlerDelegate<TResponse> next,
      CancellationToken cancellationToken
    ) => next(cancellationToken);
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
