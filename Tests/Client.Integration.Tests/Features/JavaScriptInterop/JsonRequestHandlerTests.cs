namespace BlazorState.Tests.Features.Counter
{
  using BlazorState;
  using BlazorState.Features.JavaScriptInterop;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using System.Text.Json;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.Counter.CounterState;

  internal class JsonRequestHandlerTests
  {
    private readonly JsonRequestHandler JsonRequestHandler;

    private readonly JsonSerializerOptions JsonSerializerOptions;

    private readonly IServiceProvider ServiceProvider;

    private readonly IStore Store;

    private CounterState CounterState => Store.GetState<CounterState>();

    public JsonRequestHandlerTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      JsonRequestHandler = ServiceProvider.GetService<JsonRequestHandler>();
      Store = ServiceProvider.GetService<IStore>();
      JsonSerializerOptions = ServiceProvider.GetService<BlazorStateOptions>().JsonSerializerOptions;
    }

    //public async Task ShouldPerformAction()
    public void ShouldPerformAction()
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