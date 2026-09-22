#region Purpose
// Replaces the draft text. The persistence behavior writes session storage after this action.
#endregion

namespace Sample05Wasm.Features.DraftNote;

partial class DraftNoteState
{
  public static class UpdateTextActionSet
  {
    public sealed class Action : IAction
    {
      public string Text { get; }

      public Action(string text)
      {
        Text = text;
      }
    }

    public sealed class Handler : StateActionHandler<Action>
    {
      public Handler(IStore store) : base(store) { }

      private DraftNoteState DraftNoteState => Store.GetState<DraftNoteState>();

      public override ValueTask Handle(Action action, CancellationToken cancellationToken)
      {
        DraftNoteState.Text = action.Text ?? "";
        return ValueTask.CompletedTask;
      }
    }
  }
}
