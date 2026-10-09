#region Purpose
// NetArchTest custom rule: the type has a public constructor DI can call (no primitive or string parameters).
#endregion

#region Design
// Treats a parameterless constructor as injectable. Used by the state policy for [PersistentState] states.
#endregion

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
