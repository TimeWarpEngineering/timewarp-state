#region Purpose
// Clears the draft. The empty snapshot is what session storage keeps.
#endregion

namespace Sample05Wasm.Features.DraftNote;

partial class DraftNoteState
{
  public static class ClearActionSet
  {
    public sealed class Action : IAction;

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private DraftNoteState DraftNoteState => Store.GetState<DraftNoteState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        DraftNoteState.Text = "";
        return ValueTask.CompletedTask;
      }
    }
  }
}
