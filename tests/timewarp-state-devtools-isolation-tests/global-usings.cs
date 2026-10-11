#region Purpose
// Global usings for the DevTools isolation tests.
#endregion

#region Design
// This project does not reference the DevTools app. A using for that assembly would defeat the test.
#endregion

global using System.Collections.Immutable;
global using System.Reflection;
global using System.Reflection.Metadata;
global using System.Reflection.PortableExecutable;
global using Sample04Server;
global using Sample04Server.Features.Counter;
global using Shouldly;
global using TimeWarp.Features.Cloning;
global using TimeWarp.State;
global using TimeWarp.State.DevTools.Export;
global using TimeWarp.State.DevTools.Isolation.Tests;
