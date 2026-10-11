#region Purpose
// Global usings for the inspected-app fixture used by the DevTools isolation tests.
#endregion

#region Design
// This assembly is the inspected app. It references TimeWarp.State and the exporter from source, and it does not reference the DevTools app.
#endregion

global using Microsoft.Extensions.DependencyInjection;
global using TimeWarp.Mediator;
global using TimeWarp.State;
