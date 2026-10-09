#region Purpose
// Polyfill IsExternalInit so records compile on netstandard2.0.
#endregion

#region Design
// Empty internal static class in System.Runtime.CompilerServices, which the compiler needs for init accessors.
// Internal so it does not clash with other assemblies' copies.
#endregion

namespace System.Runtime.CompilerServices;

internal static class IsExternalInit;
