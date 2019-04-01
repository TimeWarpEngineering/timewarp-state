namespace BlazorState.Tests.Features.Counter
{
  using System;
  using BlazorState;
  using BlazorState.Features.JavaScriptInterop;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Microsoft.JSInterop;
  using Shouldly;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;

  internal class JsonRequestHandlerTests
  {
    public JsonRequestHandlerTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      JsonRequestHandler = ServiceProvider.GetService<JsonRequestHandler>();
      Store = ServiceProvider.GetService<IStore>();
      CounterState = Store.GetState<CounterState>();
    }

    private CounterState CounterState { get; set; }
    private IMediator Mediator { get; }
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

      string requestAsJson = Json.Serialize(incrementCounterAction);
      int preActionCount = CounterState.Count;

      //Act
      JsonRequestHandler.Handle(requestTypeAssemblyQualifiedName, requestAsJson);

      //Assert
      CounterState = Store.GetState<CounterState>();
      CounterState.Count.ShouldBe(preActionCount + 5);
    }

  }
}
