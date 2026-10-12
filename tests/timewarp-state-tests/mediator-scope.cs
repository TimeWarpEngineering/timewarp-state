#region Purpose
// Assigns this test assembly to ClientPipeline so the mediator generator emits AddGeneratedMediator.
#endregion

#region Design
// ValidateOnBuild links every handler the referenced TimeWarp.State.Blazor assembly exposes. The scope marker
// type lives in TimeWarp.State. Without this attribute the generator does not emit the ClientPipeline sender.
#endregion

[assembly: MediatorScope(typeof(ClientPipeline))]
