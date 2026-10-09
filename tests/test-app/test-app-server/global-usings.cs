#region Purpose
// Global usings for Test.App.Server.
#endregion

#region Design
// Includes Test.App.Server.Generated so the server's generated ServerPipeline Sender and Publisher types resolve.
// ExceptionHandlings is global so the throw endpoint can name ThrowServerSideExceptionRequest without a file using.
#endregion

global using Test.App.Contracts.Features.ExceptionHandlings;
global using TimeWarp.Mediator;
global using TimeWarp.State;
global using Test.App.Server.Generated;
