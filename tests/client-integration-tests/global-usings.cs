#region Purpose
// Global usings for the client integration tests.
#endregion

#region Design
// Trivial; no design decisions.
#endregion

global using TimeWarp.Features.Cloning;
global using TimeWarp.State;
global using Shouldly;
global using JetBrains.Annotations;
global using TimeWarp.Mediator;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;
global using System.Linq;
global using System.Net.Http;
global using System.Reflection;
global using System.Text.Json;
global using AnyClone.Tests.Extensions;
global using Test.App.Client.Features.Application;
global using Test.App.Client.Features.WeatherForecast;
global using static Test.App.Contracts.Features.WeatherForecast.GetWeatherForecasts;
global using Test.App.Client.Features.Blue;
global using Test.App.Client.Features.Counter;
global using Test.App.Client.Features.EventStream;
global using Test.App.Client.Features.Purple;
global using TestApp.Client.Integration.Tests.Infrastructure;
global using TimeWarp.Features.ActionTracking;
global using TimeWarp.Features.RenderSubscriptions;
global using TimeWarp.Fixie;
