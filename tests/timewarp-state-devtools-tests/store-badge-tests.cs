#region Purpose
// Proves a store badge is short, stable, and matchable from the filter box.
#endregion

#region Design
// Label and hue are pure functions of the store id and the render tag, so these tests
// do not start the inspector page. Matching is by prefix so short filters narrow the log.
// An empty label uses the untagged explanation, not the prefix sentence.
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

  public static void MatchOnlyTheStartOfAStoreIdOrBadge()
  {
    const string storeId = "791f2c3d-0000-4000-8000-00000000abcd";
    StoreBadge.Matches(storeId, "", "791f").ShouldBeTrue();
    StoreBadge.Matches(storeId, "", "abcd").ShouldBeFalse();
    StoreBadge.Matches(storeId, "", "f").ShouldBeFalse();
    StoreBadge.Matches(storeId, InspectorCapture.InteractiveRender, "liv").ShouldBeTrue();
    StoreBadge.Matches(storeId, InspectorCapture.InteractiveRender, "ive").ShouldBeFalse();
  }

  public static void ExplainPrerenderLiveAndPrefixBadges()
  {
    StoreBadge.PrerenderDescription.ShouldBe(
      "Prerender: a short-lived server store created while rendering a page's HTML; discarded after the request.");
    StoreBadge.InteractiveDescription.ShouldBe("Live: an interactive store.");
    StoreBadge.PrefixDescription.ShouldBe("Store id prefix: render kind unknown.");

    StoreBadge.Description(InspectorCapture.PrerenderRender).ShouldBe(StoreBadge.PrerenderDescription);
    StoreBadge.Description("PRERENDER").ShouldBe(StoreBadge.PrerenderDescription);
    StoreBadge.Description(InspectorCapture.InteractiveRender).ShouldBe(StoreBadge.InteractiveDescription);
    StoreBadge.Description("").ShouldBe(StoreBadge.PrefixDescription);
    StoreBadge.Description("other").ShouldBe(StoreBadge.PrefixDescription);

    const string storeId = "cdef-rest-of-the-id";
    StoreBadge.Label(storeId, "").ShouldBe("cdef");
    StoreBadge.Tooltip(storeId, "").ShouldBe(StoreBadge.PrefixDescription + "\n" + storeId);
    StoreBadge.Tooltip("", InspectorCapture.InteractiveRender).ShouldBe(StoreBadge.InteractiveDescription);
    StoreBadge.Tooltip(storeId, InspectorCapture.PrerenderRender)
      .ShouldBe(StoreBadge.PrerenderDescription + "\n" + storeId);

    StoreBadge.Label(storeId, InspectorCapture.PrerenderRender).ShouldBe(StoreBadge.PrerenderLabel);
    StoreBadge.Description(InspectorCapture.PrerenderRender).ShouldBe(StoreBadge.PrerenderDescription);
  }

  public static void ExplainAnEmptyBadgeAsUntagged()
  {
    StoreBadge.UntaggedDescription.ShouldBe("Untagged: no store id.");
    StoreBadge.Label("", "").ShouldBe("");
    StoreBadge.Tooltip("", "").ShouldBe(StoreBadge.UntaggedDescription);
    StoreBadge.Tooltip("", "").ShouldNotContain(StoreBadge.PrefixDescription);
    StoreBadge.DescriptionElementId("", "").ShouldBe(StoreBadge.UntaggedDescriptionId);
    StoreBadge.DescriptionElementId("cdef-rest", "").ShouldBe(StoreBadge.PrefixDescriptionId);
    StoreBadge.DescriptionElementId("id", InspectorCapture.PrerenderRender).ShouldBe(StoreBadge.PrerenderDescriptionId);
    StoreBadge.DescriptionElementId("id", InspectorCapture.InteractiveRender).ShouldBe(StoreBadge.InteractiveDescriptionId);
    StoreBadge.PrerenderDescriptionId.ShouldBe("store-badge-pre");
    StoreBadge.InteractiveDescriptionId.ShouldBe("store-badge-live");
    StoreBadge.PrefixDescriptionId.ShouldBe("store-badge-prefix");
    StoreBadge.UntaggedDescriptionId.ShouldBe("store-badge-untagged");
  }

  public static void LabelWasmAndServerRuntimes()
  {
    StoreBadge.RuntimeLabel(InspectorCapture.WasmRuntime).ShouldBe(StoreBadge.WasmLabel);
    StoreBadge.RuntimeLabel("WASM").ShouldBe(StoreBadge.WasmLabel);
    StoreBadge.RuntimeLabel(InspectorCapture.ServerRuntime).ShouldBe(StoreBadge.ServerLabel);
    StoreBadge.RuntimeLabel("").ShouldBe("");
    StoreBadge.RuntimeLabel("other").ShouldBe("");
  }

  public static void MatchARuntimeLabelFromTheStart()
  {
    const string storeId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
    StoreBadge.Matches(storeId, "", "wasm", InspectorCapture.WasmRuntime).ShouldBeTrue();
    StoreBadge.Matches(storeId, "", "was", InspectorCapture.WasmRuntime).ShouldBeTrue();
    StoreBadge.Matches(storeId, "", "srv", InspectorCapture.ServerRuntime).ShouldBeTrue();
    StoreBadge.Matches(storeId, "", "rv", InspectorCapture.ServerRuntime).ShouldBeFalse();
    StoreBadge.Matches(storeId, "", "wasm", InspectorCapture.ServerRuntime).ShouldBeFalse();
    StoreBadge.Matches(storeId, InspectorCapture.InteractiveRender, "live", InspectorCapture.WasmRuntime).ShouldBeTrue();
  }
}
