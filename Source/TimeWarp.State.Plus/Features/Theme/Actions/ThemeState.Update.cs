namespace TimeWarp.Features.Theme;

[UsedImplicitly]
public partial class ThemeState
{
  public static class Update
  {
    [UsedImplicitly]
    public class Action : IAction
    {
      public Theme NewTheme { get; init; }
    }

    [UsedImplicitly]
    internal class Handler
    (
      IStore store
    ): ActionHandler<Action>(store)
    {
      private ThemeState ThemeState => Store.GetState<ThemeState>();

      [UsedImplicitly]
      public override Task Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        ThemeState.CurrentTheme = action.NewTheme;
        return Task.CompletedTask;
      }
    }
  }
}
