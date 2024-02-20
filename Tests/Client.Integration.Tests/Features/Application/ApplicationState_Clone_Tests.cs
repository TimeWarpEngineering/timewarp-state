namespace ApplicationState_;

// ReSharper disable once UnusedType.Global
// ReSharper disable once InconsistentNaming
public class Clone_Should : BaseTest
{
  public Clone_Should(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost)
  {
    ApplicationState = Store.GetState<ApplicationState>();
  }

  private ApplicationState ApplicationState { get; }

  // ReSharper disable once UnusedMember.Global
  public void Clone()
  {
    //Arrange
    ApplicationState.Initialize(name: "TestName", exceptionMessage: "Some ExceptionMessage");

    //Act
    ApplicationState clone = ApplicationState.Clone();

    //Assert
    ApplicationState.Should().NotBeSameAs(clone);
    ApplicationState.Name.Should().Be(clone.Name);
    ApplicationState.ExceptionMessage.Should().Be(clone.ExceptionMessage);
    ApplicationState.Guid.Should().NotBe(clone.Guid);
  }
}
