namespace TimeWarp.Features.Persistence;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PersistentStateAttribute
(
  PersistentStateMethod PersistentStateMethod
) : Attribute
{
  public readonly PersistentStateMethod PersistentStateMethod = PersistentStateMethod;
}
