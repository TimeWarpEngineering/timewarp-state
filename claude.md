# Claude.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TimeWarp.State** is a state management library implementing the Flux pattern using the Mediator pipeline. The core package has no Blazor dependency. **TimeWarp.State.Blazor** holds components, JavaScript interop, render subscriptions, and Redux DevTools. It handles both client-side (WebAssembly) and server-side Blazor with async state management, and console hosts can use the core package alone.

## Development Commands

### Git Workflow
**Worktree constraints**: Cannot switch/pull/delete branches across worktrees
- Create PRs: `gh pr create --head <branch> --base master --title "..." --body "..."`
- Merge PRs: `gh pr merge <PR#> --merge` (no --delete-branch)
- No squash/rebase commits

### Testing
```bash
# Run all tests using Fixie framework
dotnet run --file ./scripts/test.cs

# Run end-to-end tests using Playwright
UseHttp=true dotnet run --file ./scripts/e2e.cs

# Run specific test project
dotnet fixie <ProjectName>
```

### Development Server
```bash
# Run test application
dotnet run --file ./scripts/run-test-app.cs

# Manual test app run
dotnet watch --project ./tests/test-app/test-app-server/test-app-server.csproj
```

### Build & Package
```bash
# Build NuGet packages
dotnet run --file ./scripts/package.cs

# Build individual project
dotnet build --project <ProjectPath> --configuration Release
```

### Release Process
- **Master branch**: Builds and tests automatically, no publishing
- **GitHub Releases**: Trigger automatic NuGet publishing
- Release workflow validates version matches tag before publishing

### Analysis
```powershell
# Build and package analyzer
./BuildAndPackageAnalyzer.ps1
```

## Architecture Overview

### Core Libraries (Source/)
- **TimeWarp.State**: Store, state, actions, handlers, state-initialization and state-transaction behaviors. No Blazor reference. Embeds Analyzer and SourceGenerator as analyzers (not separate packages)
- **TimeWarp.State.Blazor**: Components, JavaScript interop, render subscriptions, Redux DevTools, wwwroot assets. Static web assets stay under `_content/TimeWarp.State/`
- **TimeWarp.State.Plus**: ActionTracking, Routing, persistence. Package id unchanged. Depends on TimeWarp.State.Blazor
- **TimeWarp.State.Analyzer**: Roslyn analyzers (embedded in main package)
- **TimeWarp.State.SourceGenerator**: Code generation (embedded in main package)
- **TimeWarp.State.Policies**: NetArchTest rules for architecture validation

### Testing Strategy
- **Unit Tests**: Core library functionality
- **Integration Tests**: Client integration testing
- **E2E Tests**: Playwright-based end-to-end testing
- **Architecture Tests**: NetArchTest validation
- **Test.App**: Comprehensive test application (Client/Server/Contracts)

### Key Patterns
- **CQRS/Flux**: Unidirectional data flow with Actions/StateActionHandlers
- **TimeWarp.Mediator 14.0.0-beta**: Generated `AddGeneratedMediator<TScope>()`, `ISender<ClientPipeline>` / `ISender<ServerPipeline>` (not reflection `AddMediator()`)
- **Async-First**: All operations are async by design
- **TypeScript Integration**: Strong typing for JavaScript interop

## Code Standards

### Required Formatting (.clinerules)
- **Indentation**: 2 spaces (no tabs), LF line endings
- **Brackets**: Allman style - all brackets on own lines aligned with parent
- **Namespaces**: File-scoped (`namespace Example;`)
- **Type Declaration**: Explicit types preferred over `var`
- **Naming**: 
  - Class scope: PascalCase (no underscore prefixes)
  - Method scope: camelCase for locals/parameters

### Example Class Structure
```csharp
namespace TimeWarp.State.Example;

public class UserService
{
  private readonly HttpClient HttpClient;
  private int RequestCount;

  public UserService
  (
    HttpClient httpClient
  )
  {
    HttpClient = httpClient;
  }

  public async Task<List<UserData>> GetUsersAsync
  (
    string[] userIds
  )
  {
    List<UserData> results = new();
    // Implementation...
    return results;
  }
}
```

## Project Configuration

### Framework
- **Target**: .NET 11 (`net11.0`)
- **SDK**: 11.0.100-rc.1.26425.128 from `global.json` (rollForward latestMinor, allowPrerelease true until .NET 11 GA on 2026-11-10; re-pin to 11.0.100 then)
- **Cloning**: default state clone is `TimeWarp.Features.Cloning` (no AnyClone/TypeSupport). Never use blocking waits (`SemaphoreSlim.Wait`, `Task.Wait`, `.Result`) in library code: .NET 11 throws on single-threaded browser WASM. Library projects declare `<SupportedPlatform Include="browser" />` so CA1416 flags them.
- **Nullable**: Disabled project-wide
- **ImplicitUsings**: Enabled

### Package Management
- **Central Management**: Uses Directory.Packages.props
- **Lock Files**: Enabled for repeatable builds
- **Local Feed**: ./LocalNugetFeed for development

### Build Process
1. Analyzers and Source Generators built first
2. TypeScript compilation for JavaScript interop
3. NuGet package creation with Source Link
4. Architecture tests validate design constraints

## Testing Framework

Uses **Fixie** testing framework instead of standard xUnit/NUnit. Test projects follow pattern:
- Test discovery by convention
- Async test support
- Custom test lifecycles for Blazor components

## Task Management

Follow structured task workflow using Kanban approach:
- Task files: `kanban/<column>/NNN-title/`
- Commit format: `Task: <TaskID> = <Status> <Description>`
- Move tasks between folders as status changes

## Package Structure

**Published NuGet Packages:**
- **TimeWarp.State**: Main package (includes embedded Analyzer/SourceGenerator)
- **TimeWarp.State.Plus**: Extended features package
- **TimeWarp.State.Policies**: Architecture testing rules

**Note**: Analyzer and SourceGenerator projects are **NOT** published as separate packages - they are embedded in the main TimeWarp.State package as analyzers.

## Essential Dependencies

- **Blazor**: UI framework (Server/WebAssembly)
- **TimeWarp.Mediator 14.0.0-beta**: CQRS/mediator via generated `AddGeneratedMediator<TScope>()` / named pipelines (`ISender<ClientPipeline>`, `ISender<ServerPipeline>`); not `AddMediator()`
- **Microsoft.JSInterop**: JavaScript interop for browser features
- **Fixie**: Testing framework
- **NetArchTest**: Architecture testing
- **Playwright**: End-to-end testing