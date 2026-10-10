#region Purpose
// Keeps the public Plus components in a trimmed WebAssembly publish.
#endregion

#region Design
// A host names these components from markup that lives in another assembly, often the server project of a
// Blazor Web App. That reference is not a root for the client's trim, so the WASM runtime cannot find the
// component and interactive WebAssembly never replaces the static prerender. The module initializer names
// each component with DynamicallyAccessedMembers.All.
#endregion

namespace TimeWarp.State.Plus;

internal static class ComponentTrimRoots
{
  [global::System.Runtime.CompilerServices.ModuleInitializer]
  [global::System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA2255:The 'ModuleInitializer' attribute should not be used in libraries",
    Justification = "Hosts reference these components from markup in another assembly. The client's trim does not see that reference.")]
  internal static void Root()
  {
    Root<global::TimeWarp.Features.Routing.TwPageTitle>();
    Root<global::TimeWarp.Features.Routing.TwBreadcrumb>();
    Root<global::TimeWarp.Features.Routing.TimeWarpPageRenderNotifier>();
  }

  private static void Root<
    [global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMembers(
      global::System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.All)] T>()
    where T : class
  {
  }
}
