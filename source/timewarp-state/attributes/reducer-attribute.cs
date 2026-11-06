#region Purpose
// Marks an ActionSet Action whose Handler the TimeWarp.State generator emits, and names that Handler for Mediator.
#endregion

#region Design
// Task 144, Option 3b (State half). Roslyn generators cannot see each other's output, so Mediator's generator never
// finds a Handler that the State generator emits. TimeWarp.Mediator 15.0.0-beta.5 reads GeneratedHandlerAttribute
// instead and dispatches to the name it carries, relative to the request's containing type (the *ActionSet).
// The name is a constructor parameter named handlerTypeName, not only a getter: TimeWarp.State reaches a consumer as
// a reference assembly, which keeps no getter body. Mediator reads that argument, including its default.
// The State generator emits the Handler only for the shape ReducerFacts accepts. Anything else is TWS0014, reported
// on this attribute before Mediator's TWM012 and CS0246 land in Mediator.g.cs.
#endregion

namespace TimeWarp.State;

/// <summary>
/// Asks the TimeWarp.State generator to emit the <c>Handler</c> for this action, which calls the action set's
/// <c>Reduce</c> method.
/// </summary>
/// <remarks>
/// Apply it to the nested <c>Action</c> class of a <c>partial</c> <c>*ActionSet</c> inside a <c>partial</c> state, and
/// declare <c>static void Reduce(TState state, Action action)</c> in the same action set:
/// <code>
/// public static partial class DecrementCountActionSet
/// {
///   [Reducer]
///   public sealed class Action : IAction { public required int Amount { get; init; } }
///
///   private static void Reduce(CounterState state, Action action) => state.Count -= action.Amount;
/// }
/// </code>
/// The generator emits <c>DecrementCountActionSet.Handler : StateActionHandler&lt;Action&gt;</c>. TimeWarp.Mediator
/// dispatches the action to it because this attribute derives from <see cref="GeneratedHandlerAttribute"/>.
/// A shape the generator cannot serve is reported as TWS0014.
/// </remarks>
/// <param name="handlerTypeName">
/// The simple name of the generated handler, nested in the action set. Defaults to <c>Handler</c>.
/// </param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ReducerAttribute(string handlerTypeName = ReducerAttribute.DefaultHandlerTypeName) : GeneratedHandlerAttribute
{
  /// <summary>The handler name used when none is given.</summary>
  public const string DefaultHandlerTypeName = "Handler";

  /// <inheritdoc />
  public override string HandlerTypeName { get; } = handlerTypeName;
}
