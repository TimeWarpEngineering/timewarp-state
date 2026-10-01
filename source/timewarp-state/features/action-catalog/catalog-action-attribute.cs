#region Purpose
// Opt-in marker that puts one ActionSet's Action into the generated per-assembly action catalog.
#endregion

#region Design
// Lives in the runtime package so consumers apply it from TimeWarp.State; the source generator and
// analyzer resolve it by metadata name (TimeWarp.State.CatalogActionAttribute).
// Only valid on the nested Action class of a *ActionSet (TWS0004). Description is required and is one
// plain sentence (TWS0005). Name defaults to <StateWithoutSuffix>.<ActionSetWithoutSuffix> and must be
// unique per assembly (TWS0006). DisplayName is optional authored UI copy; when given it must not be
// empty or whitespace (TWS0008). It is never derived here: consumers choose their own fallback.
// Permissions are opaque ids owned by the consumer; TimeWarp.State never enforces them.
// Unrelated to [TrackAction], which only drives the busy indicator.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Opts an action into the generated action catalog so hosts can enumerate and execute it at runtime
/// (for example a command palette or agent tools).
/// </summary>
/// <remarks>
/// Apply to the nested <c>Action</c> class of an <c>*ActionSet</c>. Actions without this attribute are never cataloged.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CatalogActionAttribute : Attribute
{
  /// <summary>
  /// One plain sentence that tells a human or an agent what the action does. Required.
  /// </summary>
  public string Description { get; set; } = string.Empty;

  /// <summary>
  /// Catalog name. Defaults to <c>&lt;StateWithoutSuffix&gt;.&lt;ActionSetWithoutSuffix&gt;</c>,
  /// for example <c>Credentials.AddPasskey</c>.
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  /// Human-facing label for UIs such as command palettes and menus, for example <c>Link Microsoft 365</c>.
  /// Optional; when set it must not be empty or whitespace.
  /// </summary>
  /// <remarks>
  /// <see cref="Name"/> stays the stable identifier. TimeWarp.State does not derive a label when this is not set;
  /// consumers decide how to fall back.
  /// </remarks>
  public string? DisplayName { get; set; }

  /// <summary>
  /// Consumer-defined permission or policy ids. Opaque to TimeWarp.State; the consumer enforces them.
  /// </summary>
  public string[] Permissions { get; set; } = [];

  /// <summary>
  /// Who the action is offered to. Defaults to <see cref="ActionVisibility.Human"/>.
  /// </summary>
  public ActionVisibility Visibility { get; set; } = ActionVisibility.Human;
}
