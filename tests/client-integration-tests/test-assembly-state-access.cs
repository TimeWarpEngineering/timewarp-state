#region Purpose
// Opts this assembly into StateTestOptions before any test calls a guarded Initialize overload.
#endregion

#region Design
// ThrowIfNotTestAssembly no longer treats an assembly name that contains "test" as a pass.
// GetCallingAssembly is this assembly when a test calls those overloads.
#endregion

using System.Runtime.CompilerServices;

namespace Client.Integration.Tests;

internal static class TestAssemblyStateAccess
{
  [ModuleInitializer]
  internal static void EnableStateTestAccess() => StateTestOptions.Enable();
}
