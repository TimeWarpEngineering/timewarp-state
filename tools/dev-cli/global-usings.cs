#region Purpose
// Global usings for the TimeWarp.State dev CLI
#endregion

#region Design
// Global using directives only: System namespaces, Nuru, Mediator, Amuru, Terminal, DevCli and
// Microsoft.Extensions.DependencyInjection for dev.cs and the endpoint files.
#endregion

global using System;
global using System.ComponentModel;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Text.Json;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Collections.Generic;
global using System.Xml.Linq;

global using TimeWarp.Nuru;
global using TimeWarp.Mediator;
global using static TimeWarp.Mediator.Unit;
global using TimeWarp.Amuru;
global using TimeWarp.Terminal;
global using DevCli;
global using DevCli.Commands;
global using Microsoft.Extensions.DependencyInjection;
