namespace TimeWarp.Features.Theme;

public partial class ThemeState
{
  public static class UpdateActionSet
  {
    public sealed class Action : IAction
    {
      public Theme NewTheme { get; }
      public Action(Theme newTheme) 
      {
        NewTheme = newTheme;
      }
    }
    
    public sealed class Handler
    (
      IStore store
    ): StateActionHandler<Action>(store)
    {
      private ThemeState ThemeState => Store.GetState<ThemeState>();
      
      public override ValueTask Handle
      (
        Action action,
        CancellationToken cancellationToken
      )
      {
        ThemeState.CurrentTheme = action.NewTheme;
        return default;
      }
    }
  }
}
