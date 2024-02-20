namespace Test.App.Client.Features.Application;

public partial class ApplicationState
{
  public override ApplicationState Hydrate(IDictionary<string, object> keyValuePairs) => new()
  {
    Guid =
      new Guid
      (
        keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Guid))].ToString() ??
        throw new InvalidOperationException("Guid is required.")
      ),
    Name =
      keyValuePairs[CamelCase.MemberNameToCamelCase(nameof(Name))].ToString() ??
      throw new InvalidOperationException("Name is required.")
  };

  internal void Initialize(string name, string exceptionMessage)
  {
    ThrowIfNotTestAssembly(Assembly.GetCallingAssembly());
    Name = name;
    ExceptionMessage = exceptionMessage;
  }
}
