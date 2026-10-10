#region Purpose
// Opts one field or auto-property out of deep clone and into copy-by-reference.
#endregion

#region Design
// Injected services (ILogger<T>, HttpClient, NavigationManager, IJSRuntime) have no clone the generator can emit.
// [IgnoreDataMember] leaves the constructor's default, which is null, and that null becomes the live state.
// [CloneShared] assigns the member from the source after construction, so the clone keeps the same instance.
// The attribute wins over IgnoreDataMember, NonSerialized, and JsonIgnore. Those attributes govern
// serialization; they do not null a member that is also marked [CloneShared].
// Construction passes default arguments, so a service parameter must accept null.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Copies this field or auto-property by reference when the state is cloned.
/// </summary>
/// <remarks>
/// The generated clone assigns the member from the source and does not walk its type.
/// Use this for an injected service such as <c>ILogger&lt;T&gt;</c>, <c>HttpClient</c>,
/// <c>NavigationManager</c>, or <c>IJSRuntime</c>. Construction passes <c>default</c> for each
/// parameter, so a constructor that takes the service must accept null; the member is assigned
/// after construction. <c>IgnoreDataMember</c>, <c>NonSerialized</c>, and <c>JsonIgnore</c> leave
/// the constructor value. When a member has both, <see cref="CloneSharedAttribute"/> wins for cloning.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CloneSharedAttribute : Attribute;
