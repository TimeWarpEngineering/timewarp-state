namespace System.Reflection
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  public static class AssemblyExtensions
  {
    public static IEnumerable<Type> GetLoadedTypes(this Assembly aAssembly)
    {
      try
      {
        return aAssembly.GetTypes();
      }
      catch (ReflectionTypeLoadException ex)
      {
        return ex.Types.Where(aType => aType != null);
      }
    }
  }
}
