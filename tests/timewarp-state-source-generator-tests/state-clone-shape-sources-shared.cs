#region Purpose
// Source text for the [cloneshared] and icloneable member shapes the clone-stage tests use.
#endregion

#region Design
// [CloneShared] and ICloneable member shapes: injected services copied by reference, attribute precedence, and ICloneable bases.
// Every key has one [Input] on the matching Given_Shape test; state-clone-shape-guard-tests.cs fails when they drift.
#endregion

namespace StateCloneSourceGenerator_;

internal static partial class StateCloneShapeSources
{
  public static readonly Dictionary<string, string> SharedShapes = new()
  {
    ["CloneableNonSealedBase"] =
      """
      using TimeWarp.State;
      public class Cl : System.ICloneable { public int V { get; set; } public object Clone() => new Cl { V = V }; }
      public class ClD : Cl { public int W { get; set; } }
      [GenerateClone] public class CloneableHolder { public Cl? C { get; set; } }
      """,
    ["PrivateCloneableSubclass"] =
      """
      using TimeWarp.State;
      public class Base { public int V { get; set; } }
      [GenerateClone] public class Holder
      {
        public Base? B { get; set; }
        private sealed class Hidden : Base, System.ICloneable { public object Clone() => new Hidden { V = V }; }
      }
      """,
    ["CloneSharedServices"] =
      """
      using Microsoft.Extensions.Logging;
      using TimeWarp.State;
      public abstract class AppNavigation { public abstract string Uri { get; } }
      public sealed class ServiceState : State<ServiceState>
      {
        [CloneShared]
        public ILogger<ServiceState> Logger { get; }
        [CloneShared]
        private readonly System.Net.Http.HttpClient HttpClient;
        [CloneShared]
        public AppNavigation? Navigation { get; set; }
        public ServiceState(ILogger<ServiceState>? logger, System.Net.Http.HttpClient? httpClient)
        {
          Logger = logger!;
          HttpClient = httpClient!;
        }
        public int Count { get; set; }
        public override void Initialize() { }
      }
      """,
    ["CloneSharedPrecedence"] =
      """
      using Microsoft.Extensions.Logging;
      using TimeWarp.State;
      public interface IClock { }
      public abstract class ServiceStateBase<T> : State<T> where T : ServiceStateBase<T>
      {
        [CloneShared]
        private IClock? BaseClock;
        protected IClock? Clock => BaseClock;
      }
      public sealed class PrecedenceState : ServiceStateBase<PrecedenceState>
      {
        [CloneShared, System.Runtime.Serialization.IgnoreDataMember]
        private readonly ILogger<PrecedenceState>? Logger;
        [CloneShared, System.Text.Json.Serialization.JsonIgnore]
        public IClock? Timer { get; init; }
        [field: CloneShared]
        public IClock? Stopwatch { get; set; }
        public PrecedenceState(ILogger<PrecedenceState>? logger) { Logger = logger; }
        public override void Initialize() { }
      }
      """
  };
}
