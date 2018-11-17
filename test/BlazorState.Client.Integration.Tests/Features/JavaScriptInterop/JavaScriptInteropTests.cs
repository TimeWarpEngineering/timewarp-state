namespace BlazorState.Client.Integration.Tests.Features.JavaScriptInterop
{
  using System;
  using System.Threading.Tasks;
  using BlazorState.Client.Features.Counter;
  using BlazorState.Integration.Tests.Infrastructure;
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;

  internal class JavaScriptInteropTests
  {
    public JavaScriptInteropTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
      Store = ServiceProvider.GetService<IStore>();
      CounterState = Store.GetState<CounterState>();
    }

    private CounterState CounterState { get; set; }
    private IMediator Mediator { get; }
    private IServiceProvider ServiceProvider { get; }
    private IStore Store { get; }

    // TODO: Add Tests
    //private async Task ShouldCallJavascriptMethodFromCsharp()
    //{
      
    //}

    //private async Task ShouldCallCsharpMethodFromJavascript()
    //{

    //}
  }
}