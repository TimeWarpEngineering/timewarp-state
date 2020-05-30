namespace JsonRequestHandler
{
  using BlazorState;
  using BlazorState.Features.JavaScriptInterop;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System.Text.Json;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.Counter.CounterState;

  // TODO: These used to pass with WebAssemblyHostBuilder
  // internal class won't run so they won't fail
  internal class Handle_Should: BaseTest
  {
    private readonly JsonRequestHandler JsonRequestHandler;

    private readonly JsonSerializerOptions JsonSerializerOptions;

    private CounterState CounterState => Store.GetState<CounterState>();

    public Handle_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
    {
      JsonRequestHandler = ServiceProvider.GetService<JsonRequestHandler>();
      JsonSerializerOptions = ServiceProvider.GetService<BlazorStateOptions>().JsonSerializerOptions;
    }

    public void Handle_Action()
    {
      //Arrange

      string requestTypeAssemblyQualifiedName = typeof(IncrementCounterAction).AssemblyQualifiedName;
      var incrementCounterAction = new IncrementCounterAction
      {
        Amount = 5
      };

      string requestAsJson = JsonSerializer.Serialize(incrementCounterAction, JsonSerializerOptions);
      int preActionCount = CounterState.Count;

      //Act
      JsonRequestHandler.Handle(requestTypeAssemblyQualifiedName, requestAsJson);

      //Assert
      CounterState.Count.ShouldBe(preActionCount + 5);
    }
  }
}