namespace TimeWarp.Features.Theme;

[UsedImplicitly]
public partial class ThemeState
{
  public static class UpdateActionSet
  {
    internal sealed class Action : IAction
    {
      public Theme NewTheme { get; }
      public Action(Theme newTheme) 
      {
        NewTheme = newTheme;
      }
    }
    
    internal sealed class Handler
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
