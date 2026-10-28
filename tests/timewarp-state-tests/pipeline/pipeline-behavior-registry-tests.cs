#region Purpose
// PipelineBehaviorRegistry accepts MediatorManifest version 2 and rejects a profile key or another version.
#endregion

#region Design
// Each test passes one JSON document to Read. Simple names drop the namespace and the generic argument list,
// and a behavior seen on an earlier request is not listed again.
#endregion

namespace PipelineBehaviorRegistryTests;

public class Should_
{
  public void Read_Kind_Scope_And_First_Seen_Behavior_Names()
  {
    const string ManifestJson =
      """
      {
        "version": 2,
        "namespace": "PipelineBehaviorRegistryTests",
        "requests": [
          {
            "request": "TickAction",
            "kind": "Action",
            "scope": "ClientPipeline",
            "handler": "TickHandler",
            "behaviors": [
              "TimeWarp.Features.StateTransactions.StateTransactionBehavior<TickAction, Unit>",
              "TimeWarp.State.StateInitializationPreProcessor<TickAction, Unit>"
            ]
          },
          {
            "request": "PlainRequest",
            "kind": "Request",
            "handler": "PlainHandler",
            "behaviors": [
              "StateTransactionBehavior<PlainRequest, Unit>"
            ]
          }
        ],
        "notifications": [],
        "membership": []
      }
      """;

    PipelineBehaviorRegistry.Read(ManifestJson);

    PipelineBehaviorRegistry.Requests.Count.ShouldBe(2);
    PipelineBehaviorRegistry.Requests[0].Kind.ShouldBe("Action");
    PipelineBehaviorRegistry.Requests[0].Scope.ShouldBe("ClientPipeline");
    PipelineBehaviorRegistry.Requests[1].Kind.ShouldBe("Request");
    PipelineBehaviorRegistry.Requests[1].Scope.ShouldBeNull();
    PipelineBehaviorRegistry.BehaviorNames().ShouldBe
    (
      [
        "StateTransactionBehavior",
        "StateInitializationPreProcessor"
      ]
    );
  }

  public void Reject_A_Profile_Key()
  {
    const string ManifestJson =
      """
      {
        "version": 2,
        "profile": "full",
        "requests": []
      }
      """;

    InvalidOperationException exception = Should.Throw<InvalidOperationException>
    (
      () => PipelineBehaviorRegistry.Read(ManifestJson)
    );
    exception.Message.ShouldContain("profile");
  }

  public void Reject_A_Version_Other_Than_2()
  {
    const string ManifestJson =
      """
      {
        "version": 1,
        "requests": []
      }
      """;

    InvalidOperationException exception = Should.Throw<InvalidOperationException>
    (
      () => PipelineBehaviorRegistry.Read(ManifestJson)
    );
    exception.Message.ShouldContain("version");
  }
}
