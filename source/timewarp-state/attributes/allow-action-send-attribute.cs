#region Purpose
// Escape hatch so a state action handler may dispatch an action while TW0003 keeps the debt visible.
#endregion

#region Design
// Lives in the runtime package (not the analyzer assembly) so consumers can apply it from TimeWarp.State.
// The analyzer resolves it by metadata name. Target both type and method so a base such as
// DefaultApiHandler.HandleError can grandfather the toast-dispatch pattern during conversion.
// Inherited walks the handler base chain; TW0003 still fires on the attribute so the exemption
// cannot silently rot. Reason is required — empty suppressions hide the conversion work.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Suppresses TW0002 on a state action handler type or method so a dispatch may remain while it is
/// converted to a notification. TW0003 reports the exemption as an info diagnostic.
/// </summary>
/// <param name="reason">Why this handler is allowed to send an action.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public sealed class AllowActionSendAttribute
(
  string reason
) : Attribute
{
  /// <summary>
  /// Why this handler is allowed to send an action.
  /// </summary>
  public string Reason { get; } = reason;
}
