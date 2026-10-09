#region Purpose
// Opts an action class into action tracking ([TrackAction]).
#endregion

#region Design
// Plain marker attribute (class-only, single use). ActionTrackingBehavior checks it once per closed generic type.
#endregion

namespace TimeWarp.Features.ActionTracking;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TrackActionAttribute : Attribute { }
