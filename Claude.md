# Claude.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**TimeWarp.State** is a state management library for Blazor applications implementing the Flux pattern using MediatR pipeline. It handles both client-side (WebAssembly) and server-side Blazor with async state management.

## Development Commands

### Git Workflow
**Worktree constraints**: Cannot switch/pull/delete branches across worktrees
- Create PRs: `gh pr create --head <branch> --base master --title "..." --body "..."`
- Merge PRs: `gh pr merge <PR#> --merge` (no --delete-branch)
- No squash/rebase commits

### Testing
```powershell
# Run all tests using Fixie framework
./RunTests.ps1

# Run end-to-end tests using Playwright
./RunE2ETests.ps1

# Run specific test project
dotnet fixie <ProjectName>
```

### Development Server
```powershell
# Run test application with watch mode
./RunTestApp.ps1

# Manual test app run
dotnet watch --project ./Tests/Test.App/Test.App.Server/Test.App.Server.csproj
```

### Build & Package
```powershell
# Build NuGet packages
./BuildNuGets.ps1

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
- **CQRS/Flux**: Unidirectional data flow with Actions/ActionHandlers
- **MediatR Pipeline**: Middleware-driven architecture
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
- **Target**: .NET 8.0 (`net8.0`)
- **SDK**: 8.0.100 with latest features
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
- Task files: `Kanban/<Status>/<TaskID>_<Description>.md`
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
- **MediatR**: CQRS/mediator pattern implementation
- **Microsoft.JSInterop**: JavaScript interop for browser features
- **Fixie**: Testing framework
- **NetArchTest**: Architecture testing
- **Playwright**: End-to-end testing