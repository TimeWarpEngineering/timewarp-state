namespace BlazorState.Features.Persistence.Attributes;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PersistentStateAttribute : Attribute
{
  public readonly PersistentStateMethod PersistentStateMethod;

  public PersistentStateAttribute(PersistentStateMethod persistentStateMethod)
  {
    PersistentStateMethod = persistentStateMethod;
  }
}

public enum PersistentStateMethod
{
  Server,
  SessionStorage,
  LocalStorage
}
