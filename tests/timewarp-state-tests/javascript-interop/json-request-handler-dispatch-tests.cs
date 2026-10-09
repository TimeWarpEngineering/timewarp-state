#region Purpose
// Proves JsonRequestHandler.Handle dispatches only allow-listed actions and fails closed otherwise.
#endregion

#region Design
// Builds a JsonRequestHandler from a small ServiceCollection (AddTimeWarpState over this assembly) with a
// RecordingSender and a no-op IJSRuntime; each test configures the allow-list with AddJavaScriptDispatch. Nested
// action types cover missing constructors, throwing setters and non-actions.
#endregion

namespace JsonRequestHandlerDispatchTests;

public class Should_
{
  private const string IncrementWireName =
    "JsonRequestHandlerDispatchTests.Should_+IncrementAction, timewarp-state-tests, Version=9.9.9.9, Culture=neutral, PublicKeyToken=null";

  public async Task Dispatch_An_Allowed_Action_By_Assembly_Qualified_Name()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<IncrementAction>()));

    await handler.Handle(IncrementWireName, """{"amount":7}""");

    sender.Sent.ShouldHaveSingleItem().ShouldBeOfType<IncrementAction>().Amount.ShouldBe(7);
  }

  public async Task Dispatch_An_Allowed_Action_By_Full_Name_And_Alias()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<IncrementAction>("Counter.Increment")));

    await handler.Handle(typeof(IncrementAction).FullName!, """{"amount":1}""");
    await handler.Handle("Counter.Increment", null);

    sender.Sent.Count.ShouldBe(2);
    sender.Sent[1].ShouldBeOfType<IncrementAction>().Amount.ShouldBe(0);
  }

  public static void Reject_An_Unknown_Name()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<IncrementAction>()));

    Should.Throw<InvalidRequestTypeException>(() => handler.Handle(typeof(OtherAction).AssemblyQualifiedName!, "{}"));
    sender.Sent.ShouldBeEmpty();
  }

  public static void Reject_Everything_When_Nothing_Is_Allowed()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, _ => { });

    Should.Throw<InvalidRequestTypeException>(() => handler.Handle(IncrementWireName, """{"amount":7}"""));
    sender.Sent.ShouldBeEmpty();
  }

  public static void Reject_A_Non_Action_Type()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<IncrementAction>()));

    Should.Throw<InvalidRequestTypeException>(() => handler.Handle(typeof(NotAnAction).AssemblyQualifiedName!, null));
    Should.Throw<InvalidRequestTypeException>(() => handler.Handle("System.Diagnostics.Process, System.Diagnostics.Process", null));
    sender.Sent.ShouldBeEmpty();
  }

  public static void Refuse_To_Allow_A_Non_Action_Type()
  {
    ServiceCollection services = new();

    Should.Throw<ArgumentException>(() => services.AddJavaScriptDispatch(b => b.Allow(typeof(NotAnAction))));
  }

  public static void Reject_Bad_Json()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<IncrementAction>()));

    InvalidRequestTypeException exception =
      Should.Throw<InvalidRequestTypeException>(() => handler.Handle(IncrementWireName, """{"amount":"seven"}"""));
    exception.InnerException.ShouldBeAssignableTo<System.Text.Json.JsonException>();
    Should.Throw<InvalidRequestTypeException>(() => handler.Handle(IncrementWireName, "not json"));
    Should.Throw<InvalidRequestTypeException>(() => handler.Handle(IncrementWireName, "null"));
    sender.Sent.ShouldBeEmpty();
  }

  public static void Reject_Empty_Json_For_A_Type_Without_A_Parameterless_Constructor()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<NoDefaultConstructorAction>()));

    Should.Throw<InvalidRequestTypeException>(() => handler.Handle(typeof(NoDefaultConstructorAction).FullName!, null));
    sender.Sent.ShouldBeEmpty();
  }

  public static void Reject_An_Empty_Name()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<IncrementAction>()));

    Should.Throw<InvalidRequestTypeException>(() => handler.Handle("  ", "{}"));
    sender.Sent.ShouldBeEmpty();
  }

  public static void Reject_A_Payload_Whose_Setter_Throws()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => services.AddJavaScriptDispatch(b => b.Allow<ThrowingSetterAction>()));

    InvalidRequestTypeException exception =
      Should.Throw<InvalidRequestTypeException>(() => handler.Handle(typeof(ThrowingSetterAction).FullName!, """{"amount":1}"""));
    exception.InnerException.ShouldBeOfType<InvalidOperationException>();
    sender.Sent.ShouldBeEmpty();
  }

  public static void Refuse_An_Alias_That_Collides_And_Leave_The_Registry_Unchanged()
  {
    ServiceCollection services = new();
    services.AddJavaScriptDispatch(b => b.Allow<IncrementAction>());

    Should.Throw<ArgumentException>(() => services.AddJavaScriptDispatch(b => b.Allow<OtherAction>(typeof(IncrementAction).FullName)));

    JavaScriptDispatchRegistry registry = services.BuildServiceProvider().GetRequiredService<JavaScriptDispatchRegistry>();
    registry.AllowedTypes.ShouldBe([typeof(IncrementAction)]);
    registry.TryResolve(typeof(OtherAction).FullName!, out _).ShouldBeFalse();
  }

  public static void Reject_Redux_DevTools_Requests_When_DevTools_Is_Disabled()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, _ => { });

    Should.Throw<InvalidRequestTypeException>(() => handler.Handle("TimeWarp.Features.ReduxDevTools.StartRequest", StartMessageJson));
    sender.Sent.ShouldBeEmpty();
  }

  public async Task Dispatch_Redux_DevTools_Requests_When_DevTools_Is_Enabled()
  {
    RecordingSender sender = new();
    JsonRequestHandler handler = CreateHandler(sender, services => new TimeWarpStateOptions(services).UseReduxDevTools());

    await handler.Handle("TimeWarp.Features.ReduxDevTools.StartRequest", StartMessageJson);

    sender.Sent.ShouldHaveSingleItem().GetType().FullName.ShouldBe("TimeWarp.Features.ReduxDevTools.StartRequest");
  }

  private const string StartMessageJson =
    """{"id":1,"payload":{},"source":"@devtools-extension","state":"{}","type":"START"}""";

  private static JsonRequestHandler CreateHandler(RecordingSender sender, Action<IServiceCollection> configure)
  {
    ServiceCollection services = new();
    configure(services);
    services.AddTimeWarpState(options => options.Assemblies = [typeof(Should_).Assembly]);
    services.AddTimeWarpStateBlazor();
    ServiceProvider serviceProvider = services.BuildServiceProvider();

    return new
    (
      NullLogger<JsonRequestHandler>.Instance,
      sender,
      new NullJsRuntime(),
      serviceProvider.GetRequiredService<TimeWarpStateOptions>(),
      serviceProvider.GetRequiredService<JavaScriptDispatchRegistry>()
    );
  }

  public sealed class IncrementAction : IAction
  {
    public int Amount { get; set; }
  }

  public sealed class OtherAction : IAction;

  public sealed class NoDefaultConstructorAction : IAction
  {
    public NoDefaultConstructorAction(int amount)
    {
      Amount = amount;
    }

    public int Amount { get; }
  }

  public sealed class ThrowingSetterAction : IAction
  {
    public int Amount { get => 0; set => throw new InvalidOperationException("setter rejects input"); }
  }

  public sealed class NotAnAction
  {
    public int Amount { get; set; }
  }

  private sealed class RecordingSender : ISender<ClientPipeline>
  {
    public List<object> Sent { get; } = [];

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
      Sent.Add(request);
      return Task.FromResult<object?>(null);
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest =>
      throw new NotSupportedException();

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
      throw new NotSupportedException();
  }

  private sealed class NullJsRuntime : IJSRuntime
  {
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => default;

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => default;
  }
}
