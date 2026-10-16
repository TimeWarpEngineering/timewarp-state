#region Purpose
// Global usings for the DevTools tests.
#endregion

#region Design
// One list so individual files need no using directives.
#endregion

global using System.Diagnostics;
global using System.Net;
global using System.Net.Http;
global using System.Net.Http.Json;
global using System.Net.Sockets;
global using System.Reflection;
global using System.Text;
global using System.Text.Json;
global using System.Reflection.Metadata;
global using System.Reflection.PortableExecutable;
global using Aspire.Hosting;
global using Aspire.Hosting.ApplicationModel;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging.Abstractions;
global using OpenTelemetry;
global using OpenTelemetry.Trace;
global using Shouldly;
global using TimeWarp.Amuru;
global using TimeWarp.Features.Cloning;
global using TimeWarp.Mediator;
global using TimeWarp.State;
global using TimeWarp.State.DevTools;
global using TimeWarp.State.DevTools.Export;
global using TimeWarp.State.DevTools.Features.Inspector;
global using TimeWarp.State.DevTools.Hosting;
global using TimeWarp.State.DevTools.Tests;
global using TimeWarp.State.DevTools.Wasm;
