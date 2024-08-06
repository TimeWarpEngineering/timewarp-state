namespace TimeWarp.State.Policies;

public class HaveInjectableConstructor : ICustomRule 
{
  public bool MeetsRule(TypeDefinition typeDefinition)
  {
    var type = typeDefinition.ToType();
    ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
    
    return constructors.Any(IsInjectableConstructor);
  }

  private static bool IsInjectableConstructor(ConstructorInfo constructor)
  {
    ParameterInfo[] parameters = constructor.GetParameters();
    return parameters.Length == 0 || parameters.All(p => !p.ParameterType.IsPrimitive && p.ParameterType != typeof(string));
  }
}
