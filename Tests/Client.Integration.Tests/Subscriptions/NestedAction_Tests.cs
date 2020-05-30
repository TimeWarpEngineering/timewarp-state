namespace Actions
{
  using Shouldly;
  using System;
  using System.Threading.Tasks;
  using TestApp.Client.Features.Counter;
  using TestApp.Client.Integration.Tests.Infrastructure;
  using static TestApp.Client.Features.Counter.WrongNesting;

  public class Should_ThrowException_For: BaseTest
  {
    public Should_ThrowException_For(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

    public async Task ImproperNestedActions()
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

    public async Task NonNestedActions()
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