#region Purpose
// Inputs for the golden parity tests: one source per case, covering every shape the four pre-142 generators emitted
// for (dispatch methods, catalog, persistence Load, clone and registration) plus their diagnostics.
#endregion

#region Design
// The verified files under golden/ were written by the four pre-142 generators (task 142, before the rewrite), so a
// golden test that passes proves the single TimeWarpStateGenerator emits the same hint names, text and diagnostics.
// Each case keeps to shapes the old generators already served; new output (the State manifest) is excluded from the
// comparison and pinned by its own tests.
// BareCompilation cases reference only System.Private.CoreLib, the way the old persistence test driver did.
#endregion

namespace TimeWarp.State.SourceGenerator.Tests;

internal static class GoldenSources
{
  public static readonly IReadOnlyDictionary<string, string> Cases = new Dictionary<string, string>(StringComparer.Ordinal)
  {
    ["counter"] =
      """
      namespace App.Features.Counter;

      using System;
      using TimeWarp.Features.Persistence;
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public enum Speed { Slow, Fast }

      [PersistentState(PersistentStateMethod.SessionStorage)]
      public sealed partial class CounterState : State<CounterState>
      {
        public int Count { get; private set; }

        public override void Initialize() { }

        public static class IncrementCountActionSet
        {
          public sealed class Action : IAction
          {
            public int Amount { get; init; } = 1;
          }
        }

        public static class AddActionSet
        {
          [CatalogAction(Description = "Add to the count.", DisplayName = "Add", Permissions = new[] { "counter.write" })]
          public sealed class Action : IAction
          {
            public Action(int amount = 1, Speed speed = Speed.Fast) { Amount = amount; Speed = speed; }
            public int Amount { get; }
            public Speed Speed { get; }
          }
        }

        public static class ScheduleActionSet
        {
          [CatalogAction(Description = "Schedule a reset.", Name = "Counter.ScheduleReset", Visibility = ActionVisibility.Agent)]
          public sealed record Action(Guid Id, DateTime At, string Label, double Weight = 0.5, bool Animate = false) : IAction;
        }

        public static class ResizeActionSet
        {
          public sealed class Action(int size) : IAction
          {
            public int Size { get; } = size;
          }
        }

        public static class RenameActionSet
        {
          [CatalogAction(Description = "Rename the counter.")]
          public sealed class Action : IAction
          {
            public required string Name { get; init; }
            public int Order { get; init; }
          }
        }

        public static class FiveSecondActionSet
        {
          public sealed record Action(int Delay) : IAction;
        }

        public Task FiveSecond(string reason) => Task.CompletedTask;
      }
      """,

    ["shapes"] =
      """
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed partial class RootState : State<RootState>
      {
        public override void Initialize() { }

        public static class PingActionSet
        {
          public sealed record Action(string Message = "ping") : IAction;
        }
      }

      namespace App.Shapes
      {
        public static partial class Outer
        {
          public sealed partial class InnerState : State<InnerState>
          {
            public override void Initialize() { }

            public static class TouchActionSet
            {
              public sealed class Action : IAction { }
            }
          }
        }

        public sealed partial class BoxState<T> : State<BoxState<T>>
        {
          public override void Initialize() { }

          public static class StoreActionSet
          {
            public readonly record struct Action(int Slot) : IAction;
          }
        }

        internal sealed class Secret { }

        public sealed partial class VaultState : State<VaultState>
        {
          public override void Initialize() { }

          public static class HideActionSet
          {
            public sealed class Action : IAction
            {
              internal Action(Secret secret) { }
            }
          }
        }

        public static class Docs
        {
          public static class SampleActionSet
          {
            public sealed class Action : IAction { }
          }
        }
      }
      """,

    ["failures"] =
      """
      namespace App.Failures;

      using TimeWarp.Features.Persistence;
      using TimeWarp.Mediator;
      using TimeWarp.State;

      public sealed class PlainState : State<PlainState>
      {
        public override void Initialize() { }

        public static class GoActionSet
        {
          public sealed class Action : IAction { }
        }
      }

      public sealed partial class AbstractState : State<AbstractState>
      {
        public override void Initialize() { }

        public static class RunActionSet
        {
          public abstract class Action : IAction { }
        }

        public int Clash { get; private set; }

        public static class ClashActionSet
        {
          public sealed class Action : IAction { }
        }
      }

      [PersistentState(PersistentStateMethod.LocalStorage)]
      public sealed class NotPartialState : State<NotPartialState>
      {
        public override void Initialize() { }
      }

      public static class Holder
      {
        [PersistentState(PersistentStateMethod.LocalStorage)]
        public sealed partial class NestedState : State<NestedState>
        {
          public override void Initialize() { }
        }
      }
      """,

    ["clone"] =
      """
      namespace App.Clone;

      using System;
      using System.Collections.Generic;
      using TimeWarp.State;

      [GenerateClone]
      public class Line
      {
        public string Sku { get; set; } = "";
        public int Quantity { get; set; }
      }

      public sealed class CartState : State<CartState>
      {
        public List<Line> Lines { get; private set; } = new();
        public Dictionary<string, int> Totals { get; private set; } = new();
        public Line? Featured { get; private set; }
        public override void Initialize() { }
      }

      public sealed class HandState : State<HandState>, ICloneable
      {
        public override void Initialize() { }
        public object Clone() => new HandState();
      }

      public sealed class BoxState : State<BoxState>
      {
        public object Payload { get; set; } = new();
        public override void Initialize() { }
      }
      """
  };

  public static readonly IReadOnlyDictionary<string, string> BareCases = new Dictionary<string, string>(StringComparer.Ordinal)
  {
    ["persistence-bare"] =
      """
      namespace App.Bare;

      [PersistentState(PersistentStateMethod.SessionStorage)]
      public partial class WidgetState { }

      [PersistentStateAttribute(PersistentStateMethod.LocalStorage)]
      public partial class GadgetState { }

      public static class Outer
      {
        [PersistentState(PersistentStateMethod.SessionStorage)]
        public partial class NestedState { }
      }

      public partial class Plain { }
      """
  };
}
