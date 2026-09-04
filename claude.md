# Claude.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TimeWarp.State** is a state management library for Blazor applications implementing the Flux pattern using Mediator pipeline. It handles both client-side (WebAssembly) and server-side Blazor with async state management.

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
- **TimeWarp.State**: Main library with base classes, Redux DevTools, JavaScript interop
  - Embeds Analyzer and SourceGenerator as analyzers (not separate packages)
- **TimeWarp.State.Plus**: Extended functionality with ActionTracking, Routing, Themes
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
- **Target**: .NET 10 (`net10.0`)
- **SDK**: 10.0.301 from `global.json` (rollForward latestMinor)
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