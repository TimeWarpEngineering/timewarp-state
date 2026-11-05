#region Purpose
// Propagates clone failures and reports one TWSG002 per source location.
#endregion

#region Design
// A type-level failure is reported through the source member that reaches it, unless the type is a root.
// Reachability is computed only for roots that already produced a diagnostic, and skipped when that set is empty.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal sealed partial class StateClonePlanner
{

  private static void Fail(Slot slot, string detail, Location? location, Slot? cause = null, bool member = false)
  {
    if (slot.PrimaryFailure)
    {
      return;
    }

    slot.Kind = SlotKind.Error;
    slot.Detail ??= detail;
    slot.DiagnosticLocation ??= location ?? Location.None;
    slot.PrimaryFailure = true;
    slot.Cause = cause;
    slot.MemberFailure = member;
  }

  private static Location? LocationOf(ITypeSymbol type) =>
    type.Locations.FirstOrDefault(location => location.IsInSource) ?? type.Locations.FirstOrDefault();

  private static Location? SourceLocation(ITypeSymbol type) =>
    type.OriginalDefinition.Locations.FirstOrDefault(location => location.IsInSource);

  private static Location? FieldLocation(IFieldSymbol field) =>
    field.Locations.FirstOrDefault(location => location.IsInSource)
    ?? field.AssociatedSymbol?.Locations.FirstOrDefault(location => location.IsInSource);

  private void PropagateErrors()
  {
    bool changed = true;
    while (changed)
    {
      changed = false;
      foreach (Slot slot in Slots.Values)
      {
        if (slot.Kind is SlotKind.Error or SlotKind.Share)
        {
          continue;
        }

        foreach (Slot dependency in slot.Dependencies)
        {
          if (dependency.Kind != SlotKind.Error)
          {
            continue;
          }

          slot.Kind = SlotKind.Error;
          slot.Detail ??= $"it depends on '{dependency.Type.ToDisplayString()}', which cannot be cloned";
          changed = true;
          break;
        }
      }
    }
  }

  // One diagnostic per primary failure that has a source location: the member (or type) the author can change.
  // Failures inside BCL or referenced types surface through the source member that reaches them. A failed root with
  // no reported cause still gets one diagnostic at its declaration.
  private void Report(List<Diagnostic> diagnostics)
  {
    PlanningCancellation.ThrowIfCancellationRequested();
    Dictionary<Slot, Slot> rootOf = [];
    foreach (Slot root in Roots)
    {
      Mark(root, root, rootOf);
    }

    HashSet<string> reported = new(StringComparer.Ordinal);
    HashSet<Slot> reportedRoots = [];

    foreach (KeyValuePair<Slot, Slot> pair in rootOf)
    {
      PlanningCancellation.ThrowIfCancellationRequested();
      Slot slot = pair.Key;
      if (slot.Kind != SlotKind.Error || !slot.PrimaryFailure || slot.DiagnosticLocation is not { IsInSource: true } location)
      {
        continue;
      }

      // A type-level failure (no implementation, no constructor, ...) is reported through the member that reaches
      // it, unless the type is itself a root. A member failure caused by a deeper source member is left to that one.
      if (!slot.MemberFailure && !slot.IsRoot || HasDeeperMemberFailure(slot))
      {
        continue;
      }

      if (!reported.Add(slot.FullyQualified + "|" + slot.Detail + "|" + location.GetLineSpan()))
      {
        continue;
      }

      reportedRoots.Add(pair.Value);
      diagnostics.Add(Diagnostic.Create(
        StateCloneSourceGenerator.UnsupportedRule,
        location,
        slot.Type.ToDisplayString(),
        slot.Detail,
        RootSuffix(slot, pair.Value)));
    }

    if (reportedRoots.Count == 0)
    {
      ReportUncoveredRoots(diagnostics, rootOf, reportedRoots, reachableFromRoot: null);
      return;
    }

    ReportUncoveredRoots(diagnostics, rootOf, reportedRoots, []);
  }

  private void ReportUncoveredRoots(
    List<Diagnostic> diagnostics,
    Dictionary<Slot, Slot> rootOf,
    HashSet<Slot> reportedRoots,
    Dictionary<Slot, HashSet<Slot>>? reachableFromRoot)
  {
    foreach (Slot root in Roots)
    {
      PlanningCancellation.ThrowIfCancellationRequested();
      if (root.Kind != SlotKind.Error || reportedRoots.Contains(root))
      {
        continue;
      }

      if (reachableFromRoot is not null && ReachedByReportedRoot(root, reachableFromRoot, reportedRoots))
      {
        continue;
      }

      if (!AnyReportedBelow(root, rootOf, reportedRoots))
      {
        diagnostics.Add(Diagnostic.Create(
          StateCloneSourceGenerator.UnsupportedRule,
          SourceLocation(root.Type) ?? Location.None,
          root.Type.ToDisplayString(),
          root.Detail ?? "a member cannot be cloned",
          string.Empty));
      }
    }
  }

  private static bool HasDeeperMemberFailure(Slot slot)
  {
    HashSet<Slot> seen = [slot];
    for (Slot? cause = slot.Cause; cause is not null && seen.Add(cause); cause = cause.Cause)
    {
      if (cause.PrimaryFailure && cause.MemberFailure && cause.DiagnosticLocation is { IsInSource: true })
      {
        return true;
      }
    }

    return false;
  }

  private static bool AnyReportedBelow(Slot root, Dictionary<Slot, Slot> rootOf, HashSet<Slot> reportedRoots)
  {
    if (reportedRoots.Count == 0)
    {
      return false;
    }

    HashSet<Slot> seen = [];
    Stack<Slot> pending = new();
    pending.Push(root);
    while (pending.Count > 0)
    {
      Slot current = pending.Pop();
      if (!seen.Add(current))
      {
        continue;
      }

      if (current.Kind == SlotKind.Error && current.PrimaryFailure && current.DiagnosticLocation is { IsInSource: true }
          && rootOf.TryGetValue(current, out Slot? owner) && reportedRoots.Contains(owner))
      {
        return true;
      }

      foreach (Slot dependency in current.Dependencies)
      {
        pending.Push(dependency);
      }
    }

    return false;
  }

  private static bool ReachedByReportedRoot(Slot root, Dictionary<Slot, HashSet<Slot>> reachableFromRoot, HashSet<Slot> reportedRoots)
  {
    foreach (Slot reported in reportedRoots)
    {
      if (reported == root)
      {
        continue;
      }

      if (!reachableFromRoot.TryGetValue(reported, out HashSet<Slot>? reachable))
      {
        reachable = Reachable(reported);
        reachableFromRoot.Add(reported, reachable);
      }

      if (reachable.Contains(root))
      {
        return true;
      }
    }

    return false;
  }

  private static HashSet<Slot> Reachable(Slot from)
  {
    HashSet<Slot> seen = [];
    Stack<Slot> pending = new();
    pending.Push(from);
    while (pending.Count > 0)
    {
      Slot current = pending.Pop();
      if (!seen.Add(current))
      {
        continue;
      }

      foreach (Slot dependency in current.Dependencies)
      {
        pending.Push(dependency);
      }
    }

    return seen;
  }

  private static string RootSuffix(Slot slot, Slot root) =>
    slot == root ? string.Empty : $" (reached from '{root.Type.ToDisplayString()}')";

  private static void Mark(Slot slot, Slot root, Dictionary<Slot, Slot> rootOf)
  {
    Stack<Slot> pending = new();
    pending.Push(slot);
    while (pending.Count > 0)
    {
      Slot current = pending.Pop();
      if (rootOf.ContainsKey(current))
      {
        continue;
      }

      rootOf.Add(current, root);
      foreach (Slot dependency in current.Dependencies)
      {
        pending.Push(dependency);
      }
    }
  }

}
