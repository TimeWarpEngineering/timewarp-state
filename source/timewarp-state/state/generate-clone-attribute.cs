#region Purpose
// Opts a non-state class or struct into the clone source generator.
#endregion

#region Design
// State&lt;T&gt; is included without this attribute. Other graphs (clone-suite fixtures, plain objects that call
// Clone()) opt in here. The attribute is not inherited, so a derived type chooses for itself. The generator emits
// an external cloner; the type does not need to be partial.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Emits an AOT-safe <c>Clone</c> extension for this class or struct.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
public sealed class GenerateCloneAttribute : Attribute
{
}
