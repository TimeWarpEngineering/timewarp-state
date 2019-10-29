namespace TestApp.Client.Integration.Tests.Subscriptions
{
  using MediatR;
  using Microsoft.Extensions.DependencyInjection;
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.Counter.WrongNesting;

  public class NestedActionTests
  {
    private readonly IMediator Mediator;
    private readonly IServiceProvider ServiceProvider;

    public NestedActionTests(TestFixture aTestFixture)
    {
      ServiceProvider = aTestFixture.ServiceProvider;
      Mediator = ServiceProvider.GetService<IMediator>();
    }

    public async Task ShouldThrowExceptionForImproperNestedActions()
    {
      // Arrange
      var improperNestedAction = new ImproperNestedAction();
      // Act
      // Assert
      Exception exception = await Should.ThrowAsync<Exception>
      (
        async () => _ = await Mediator.Send(improperNestedAction)
      );
    }

    public async Task ShouldThrowExceptionForNonNestedActions()
    {
      // Arrange
      var nonNestedAction = new NonNestedAction();
      // Act
      // Assert
      Exception exception = await Should.ThrowAsync<Exception>
      (
        async () => _ = await Mediator.Send(nonNestedAction)
      );
    }
  }
}