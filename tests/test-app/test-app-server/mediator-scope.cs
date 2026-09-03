#region Purpose
// Assigns every handler in the server assembly to ServerPipeline.
#endregion

#region Design
// Assembly-level MediatorScope keeps API handlers off ClientPipeline store behaviors. A typed
// ISender<ServerPipeline>.Send of a client action is the TWM004 compile error. Reinforced on
// individual server request/handler types where contracts DTOs cannot carry the attribute.
#endregion

using TimeWarp.Mediator;
using TimeWarp.State;

[assembly: MediatorScope(typeof(ServerPipeline))]
