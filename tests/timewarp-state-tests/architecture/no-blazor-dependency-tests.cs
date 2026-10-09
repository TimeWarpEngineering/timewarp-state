#region Purpose
// Proves TimeWarp.State does not reference Blazor or JavaScript interop, and that TimeWarp.State.Blazor does.
#endregion

#region Design
// GetReferencedAssemblies is the reference set written into the assembly manifest. A PackageReference
// the compiler was given shows up here even when no type from it is used, so a leftover
// Microsoft.AspNetCore.Components.Web reference fails the core check.
#endregion

namespace TimeWarp.State.Architecture;

public class CoreAssembly_Should_
{
  public static void NotReferenceBlazorOrJavaScriptInterop()
  {
    string[] names = ReferencedNames(typeof(AssemblyMarker).Assembly);

    names.ShouldNotContain("Microsoft.AspNetCore.Components");
    names.ShouldNotContain("Microsoft.AspNetCore.Components.Web");
    names.ShouldNotContain("Microsoft.AspNetCore.Components.Forms");
    names.ShouldNotContain("Microsoft.JSInterop");
  }

  public static void BlazorPackage_ReferencesComponents()
  {
    string[] names = ReferencedNames(typeof(TimeWarp.State.Blazor.AssemblyMarker).Assembly);

    names.ShouldContain("Microsoft.AspNetCore.Components");
    names.ShouldContain("Microsoft.JSInterop");
    names.ShouldContain("TimeWarp.State");
  }

  private static string[] ReferencedNames(System.Reflection.Assembly assembly) =>
    assembly.GetReferencedAssemblies()
      .Select(assemblyName => assemblyName.Name ?? "")
      .ToArray();
}
