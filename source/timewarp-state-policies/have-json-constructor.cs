#region Purpose
// NetArchTest custom rule: the type has a public constructor marked [JsonConstructor].
#endregion

#region Design
// Used by the state policy so [PersistentState] states can be deserialized when loaded from storage.
#endregion

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
