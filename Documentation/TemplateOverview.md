# TimeWarp-Blazor Template

## Installation

```console
dotnet new --install TimeWarp.AspNetCore.Blazor.Templates
```

## Usage

To create new solution enter the following:

```console
dotnet new timewarp-blazor -n MyBlazorApp
```

To run the new solution change to the directory that contains the startup project.  In our template the startup project is the server project.

```console
cd .\MyBlazorApp\Source\MyBlazorApp.Server\
dotnet run
```

You should see similar console output to the following:

```console
λ  dotnet run
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Now listening on: https://localhost:5001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\git\temp\MyBlazorApp\Source\MyBlazorApp.Server
```

Open up your browser to <https://localhost:5001> and confirm you have running site.

## Content

The template creates 3 projects which will be deployed and 3 test projects.

### Projects

MyBlazorApp.Client - This is the user interface project . The "Single Page Application (SPA)"
MyBlazorApp.Server - This is the server project that serves up the SPA and is also the web api.
MyBlazorApp.Shared - This is a library of common code shared between the Client and Server Projects.

### Test Projects

MyBlazorApp.Client.Integration.Tests - contains integration tests for the SPA
MyBlazorApp.Server.Integration.Tests - contains integration tests for the web api
MyBlazorApp.EndToEnd.Tests - contains seleneum based web tests.  (Runs the app and controls a browser to test)
