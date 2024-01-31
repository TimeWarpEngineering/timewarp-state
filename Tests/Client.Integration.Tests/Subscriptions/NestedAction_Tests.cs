namespace Actions;

using static TestApp.Client.Features.Counter.WrongNesting;

public class Should_ThrowException_For : BaseTest
{
  public Should_ThrowException_For(ClientHost aWebAssemblyHost) : base(aWebAssemblyHost) { }

  public async Task ImproperNestedActions()
  {
    var improperNestedAction = new ImproperNestedAction();
    Func<Task> act = async () => await Send(improperNestedAction);
    await act.Should().ThrowAsync<NonNestedClassException>();
  }

  public async Task NonNestedActions()
  {
    var nonNestedAction = new NonNestedAction();
    Func<Task> act = async () => await Send(nonNestedAction);
    await act.Should().ThrowAsync<NonNestedClassException>();
  }
}
