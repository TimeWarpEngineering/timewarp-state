# Round 1 — general
**Date:** 2026-10-13
**Scope reviewed:** same as framework (product diff: Directory.Packages.props, test-app-server program/csproj, deleted server-pipeline-registration.cs, client-integration-tests csproj + testing-convention.cs, nuspec allow-list test fixture; plus task.md Results)

## Summary

The change bumps the three Mediator pins to beta.5 and swaps the hand-written `AddServerPipelineMediator` for the generated `AddGeneratedMediator<ServerPipeline>()`. Risk is low. I checked each claim against the repo and a local build and test run:

- The generated `AddGeneratedMediator_ServerPipeline` (server obj, generator 14.0.0-beta.5 per project.assets.json) registers the same services, with the same lifetimes, as the deleted file: Sender/Publisher_ServerPipeline transient, `ISender/IPublisher<ServerPipeline>` transient, `GetWeatherForecastsHandler` plus its `IRequestHandler` scoped. Behavior is unchanged.
- The generated `GeneratedMediatorServiceCollectionExtensions` is `internal` in `Microsoft.Extensions.DependencyInjection`. Test.App.Client grants `InternalsVisibleTo` only to Client.Integration.Tests, so the server host does not see two overloads. The comments in program.cs and the csproj are accurate.
- Both hosts grant `InternalsVisibleTo("Client.Integration.Tests")`, so keeping `Aliases="TestAppServer"` there is correct. The updated comments say why.
- No references to `AddServerPipelineMediator` / `ServerPipelineRegistration` remain.
- With CS1591 set to `none` in `.editorconfig`, dropping the `NoWarn` changes nothing. A `--no-incremental` rebuild of Client.Integration.Tests (which builds both hosts) reported only NU1510 and TW0007 warnings: no TWM, no CS1591, no CS0121.
- The nuspec allow-list fixture tracks the new Contracts version. The Roslyn hold has a reason in Results and is locked by an existing test.
- `./bin/dev test` reproduced the Results counts exactly (44 / 90 / 120+1 skip / 40+1 skip / 21 / 60 / 4 / 64+1 skip / 7+1 skip). The weather-endpoint integration tests go through the new server registration.

## Issues

None.
