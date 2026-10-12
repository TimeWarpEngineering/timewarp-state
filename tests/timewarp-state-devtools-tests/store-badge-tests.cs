#region Purpose
// Proves a store badge is short, stable, and matchable from the filter box.
#endregion

#region Design
// Label and hue are pure functions of the store id and the render tag, so these tests
// do not start the inspector page.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class StoreBadge_Should
{
  public static void LabelAKnownRenderKindAndOtherwiseTheIdPrefix()
  {
    StoreBadge.Label("aaaaaaaa-1111", InspectorCapture.PrerenderRender).ShouldBe(StoreBadge.PrerenderLabel);
    StoreBadge.Label("bbbbbbbb-2222", InspectorCapture.InteractiveRender).ShouldBe(StoreBadge.InteractiveLabel);
    StoreBadge.Label("cdef-rest", "").ShouldBe("cdef");
    StoreBadge.Label("", "").ShouldBe("");
  }

  public static void KeepHueStableForOneStore()
  {
    const string storeId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    int hue = StoreBadge.Hue(storeId);
    StoreBadge.Hue(storeId).ShouldBe(hue);
    hue.ShouldBeGreaterThanOrEqualTo(0);
    hue.ShouldBeLessThan(StoreBadge.HueCount);
    StoreBadge.Hue("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb").ShouldBeGreaterThanOrEqualTo(0);
  }

  public static void MatchAStoreIdOrItsBadge()
  {
    const string storeId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    StoreBadge.Matches(storeId, InspectorCapture.PrerenderRender, "pre").ShouldBeTrue();
    StoreBadge.Matches(storeId, InspectorCapture.PrerenderRender, "AAAA").ShouldBeTrue();
    StoreBadge.Matches(storeId, InspectorCapture.InteractiveRender, "live").ShouldBeTrue();
    StoreBadge.Matches(storeId, "", "nope").ShouldBeFalse();
    StoreBadge.Matches(storeId, "", "").ShouldBeTrue();
  }
}
