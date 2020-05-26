namespace TestApp.Client.Integration.Tests.Features.Application
{
  using AnyClone;
  using BlazorState;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using TestApp.Client.Features.Application;
  using TestApp.Client.Integration.Tests.Infrastructure;

  internal class ApplicationStateCloneTests: BaseTest
  {
    public ApplicationStateCloneTests(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
    {
      IServiceProvider serviceProvider = aWebAssemblyHost.ServiceProvider;
      IStore store = serviceProvider.GetService<IStore>();
      ApplicationState = store.GetState<ApplicationState>();
    }

    private ApplicationState ApplicationState { get; set; }

    public void ShouldClone()
    {
      //Arrange
      ApplicationState.Initialize(aName: "TestName");

      //Act
      ApplicationState clone = ApplicationState.Clone();

      //Assert
      ApplicationState.ShouldNotBeSameAs(clone);
      ApplicationState.Name.ShouldBe(clone.Name);
      ApplicationState.Guid.ShouldNotBe(clone.Guid);
    }

  }
}
