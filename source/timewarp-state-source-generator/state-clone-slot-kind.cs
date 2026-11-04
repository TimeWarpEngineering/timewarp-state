#region Purpose
// Names the four outcomes a clone slot can reach.
#endregion

#region Design
// Pending means classification has not finished. Share copies the value. Clone emits a method. Error is TWSG002.
#endregion

namespace TimeWarp.State.SourceGenerator;

internal enum SlotKind
{
  Pending,
  Share,
  Clone,
  Error
}

