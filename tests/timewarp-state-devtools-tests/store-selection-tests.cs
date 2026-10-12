#region Purpose
// Proves the State panel's store choice: a pin holds, null follows the live store, empty is the untagged bucket.
#endregion

#region Design
// StoreSelection is pure over the ListStores result and the pinned id, so these tests build the store list
// from an InspectorLog and do not start the inspector page.
#endregion

namespace TimeWarp.State.DevTools.Tests;

public class StoreSelection_Should
{
  private const string LiveStoreId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
  private const string NewerLiveStoreId = "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb";

  public static void KeepThePinWhenANewerStoreArrives()
  {
    InspectorLog log = new();
    log.Add(Init(LiveStoreId, InspectorCapture.InteractiveRender));
    StoreSelection.Effective(log.ListStores(), LiveStoreId).ShouldBe(LiveStoreId);

    log.Add(Init(NewerLiveStoreId, InspectorCapture.InteractiveRender));
    StoreSelection.Effective(log.ListStores(), LiveStoreId).ShouldBe(LiveStoreId);
    StoreSelection.Effective(log.ListStores(), null).ShouldBe(NewerLiveStoreId);
  }

  public static void TellTheUntaggedBucketApartFromNoPin()
  {
    InspectorLog log = new();
    log.Add(Init("", ""));
    log.Add(Init(LiveStoreId, InspectorCapture.InteractiveRender));

    StoreSelection.Effective(log.ListStores(), null).ShouldBe(LiveStoreId);
    StoreSelection.Effective(log.ListStores(), "").ShouldBe("");
  }

  public static void FollowTheDefaultWhenThePinnedStoreIsGone()
  {
    InspectorLog log = new();
    log.Add(Init(LiveStoreId, InspectorCapture.InteractiveRender));
    StoreSelection.Effective(log.ListStores(), NewerLiveStoreId).ShouldBe(LiveStoreId);

    log.Clear(raiseChanged: false);
    StoreSelection.Effective(log.ListStores(), LiveStoreId).ShouldBe("");
    StoreSelection.Default(log.ListStores()).ShouldBeNull();
  }

  private static ActionLogEntry Init(string storeId, string render) =>
    new()
    {
      Name = "CounterState.init",
      ActionName = "init",
      StateTypeName = "CounterState",
      Status = "Ok",
      Sequence = 0,
      DurationMilliseconds = 1,
      StartTimeUtc = DateTime.UtcNow,
      SnapshotJson = """{"count":0}""",
      SnapshotKind = InspectorCapture.SnapshotEventName,
      StoreId = storeId,
      Render = render
    };
}
