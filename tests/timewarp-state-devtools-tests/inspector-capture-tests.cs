#region Purpose
// Proves inspector spans are dropped and inspected-app spans are kept.
#endregion

#region Design
// ShouldCapture is the rule both the exporter and ingest use. These cases are the recursion guard
// without a host: InspectorState and timewarp.state.inspector=true never enter the log.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class InspectorCapture_Should
{
  public static void KeepAnInspectedAppSpan()
  {
    InspectorCapture.ShouldCapture("CounterState", inspectorTag: null).ShouldBeTrue();
  }

  public static void KeepASpanWithNoEnclosingState()
  {
    InspectorCapture.ShouldCapture(stateTypeName: null, inspectorTag: null).ShouldBeTrue();
  }

  public static void DropInspectorState()
  {
    InspectorCapture.ShouldCapture(InspectorCapture.InspectorStateTypeName, inspectorTag: null).ShouldBeFalse();
  }

  public static void DropAnExplicitInspectorTag()
  {
    InspectorCapture.ShouldCapture("CounterState", inspectorTag: "true").ShouldBeFalse();
    InspectorCapture.ShouldCapture("CounterState", inspectorTag: "TRUE").ShouldBeFalse();
  }
}
