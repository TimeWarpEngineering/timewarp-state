namespace TimeWarp.State.Policies;

public class HaveJsonConstructor : ICustomRule 
{
  public bool MeetsRule(TypeDefinition typeDefinition)
  {
    var type = typeDefinition.ToType();
    ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    
    return constructors.Any(c => c.GetCustomAttributes(typeof(JsonConstructorAttribute), false).Length != 0);
  }
}
