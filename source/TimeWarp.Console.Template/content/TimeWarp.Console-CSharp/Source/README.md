![TimeWarp Logo](https://raw.githubusercontent.com/TimeWarpEngineering/blazor-state/master/assets/Logo.png)

# timewarp-console template
A dotnet core template with MediatR based console commands. 
The command line help is derived from xml documentation comments.

## Motivation

* Increase the speed of developing command-line tools
* use consistent architecture pattern
* allow for the ability to expose other MediatR based functionality to the command-line.

## Github

  Currently this template is part of the blazor-state mono-repo located on github @
  https://github.com/TimeWarpEngineering/blazor-state

## Build Status

Master: 

[![Build Status](https://timewarpenterprises.visualstudio.com/Blazor-State/_apis/build/status/ConsoleTemplate-Yaml?branchName=master)](https://timewarpenterprises.visualstudio.com/Blazor-State/_build/latest?definitionId=14?branchName=master)

Development: 

[![Build Status](https://timewarpenterprises.visualstudio.com/Blazor-State/_apis/build/status/Development/ConsoleTemplate-Yaml?branchName=dev)](https://timewarpenterprises.visualstudio.com/Blazor-State/_build/latest?definitionId=13?branchName=dev)

## Installation

```
dotnet new --install TimeWarp.Console.Template
```

## Usage

### Generate the template
```
dotnet new timewarp-console -n MyConsoleApp
```

### Publish

```
.\Publish.ps1
```

This will run `dotnet publish` and build the Windows specific
output version and add that output directory to your powershell path.

> Modify this script to build for the OS of your system if not Windows 10.

### Run

```
MyConsoleApp --help
```

#### Output

```
λ  MyConsoleApp --help
Usage:
  MyConsoleApp [options] [command]

Options:
  --version    Display version information

Commands:
  SampleCommand     Sample Command.
```
### Run sub-command

```
MyConsoleApp SampleCommand --help
```

#### Output

```
λ  MyConsoleApp SampleCommand --help
SampleCommand:
   Sample Command.

Usage:
  MyConsoleApp SampleCommand [options]

Options:
  --Parameter1     Some string
  --Parameter2     Some integer
  --Parameter3     Another Integer

```
### Test Execution

From the test project directory. Run:
```
dotnet test
```

#### Output

```
λ  dotnet test
Build started, please wait...
Build completed.

Test run for C:\git\temp\Test1\MyConsoleApp\Tests\MyConsoleApp.Tests\bin\Debug\netcoreapp2.2\MyConsoleApp.Tests.dll(.NETCoreApp,Version=v2.2)
Microsoft (R) Test Execution Command Line Tool Version 15.9.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Starting test execution, please wait...

Total tests: 4. Passed: 4. Failed: 0. Skipped: 0.
Test Run Successful.
Test execution time: 2.2818 Seconds
```

## Template Contents

### Projects

Source\MyConsoleApp.csproj

### Test Projects

Tests\MyConsoleApp.Tests\MyConsoleApp.Tests.csproj

### Code style
The template includes the following coding style configurations.
* .editorconfig
* CodeMaid.config

and should follow the [TimeWarp Coding Standards](TODO)

## Contribute

When contributing to this repository,
please first discuss the change you wish to make via an issue, 
before making a change.

## License

The [Unlicense](https://choosealicense.com/licenses/unlicense/)
