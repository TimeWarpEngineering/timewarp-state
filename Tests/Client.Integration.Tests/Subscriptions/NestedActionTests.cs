namespace TestApp.Client.Integration.Tests.Subscriptions_Tests
{
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.Counter.WrongNesting;

  public class NestedActionTests: BaseTest
  {
    public NestedActionTests(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

    public async Task ShouldThrowExceptionForImproperNestedActions()
    {
      // Arrange
      var improperNestedAction = new ImproperNestedAction();
      // Act
      // Assert
      Exception exception = await Should.ThrowAsync<Exception>
      (
        async () => await Send(improperNestedAction)
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
        async () => await Send(nonNestedAction)
      );
    }
  }
}