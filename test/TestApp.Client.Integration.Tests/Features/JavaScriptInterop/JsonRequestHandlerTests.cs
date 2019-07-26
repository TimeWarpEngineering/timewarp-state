namespace BlazorState.Tests.Features.Counter
{
  using System;
  using BlazorState;
  using BlazorState.Features.JavaScriptInterop;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using System.Text.Json;

  internal class JsonRequestHandlerTests
  {
    public JsonRequestHandlerTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      JsonRequestHandler = ServiceProvider.GetService<JsonRequestHandler>();
      Store = ServiceProvider.GetService<IStore>();
      CounterState = Store.GetState<CounterState>();
    }

    private CounterState CounterState { get; set; }
    private IServiceProvider ServiceProvider { get; }
    private IStore Store { get; }
    private JsonRequestHandler JsonRequestHandler { get; }

    //public async Task ShouldPerformAction()
    public void ShouldPerformAction()
    {
      //Arrange

      string requestTypeAssemblyQualifiedName = typeof(IncrementCounterAction).AssemblyQualifiedName;
      var incrementCounterAction = new IncrementCounterAction
      {
        Amount = 5
      };

      string requestAsJson = JsonSerializer.Serialize(incrementCounterAction);
      int preActionCount = CounterState.Count;

      //Act
      JsonRequestHandler.Handle(requestTypeAssemblyQualifiedName, requestAsJson);

      //Assert
      CounterState = Store.GetState<CounterState>();
      CounterState.Count.ShouldBe(preActionCount + 5);
    }

  }
}
