# DevOps

Production Artifacts:

* Blazor-State Nuget Package on Nuget.org
* TimeWarp Templates Nuget Package on Nuget.org
* Documentation Site
* Sample Site

Development Artifacts:

* Blazor-State Nuget Package on Myget.
* TimeWarp Templates Nuget Package on Myget.org

Changes to tools may want to trigger new versions of outputs but currently they don't.

2019-01-03 15:44:40 Creating a Yaml for each output.


```
- task: DotNetCoreInstaller@0
  displayName: Use .NET Core SDK 2.1.500 
  inputs:
    version: 2.1.500
```

Do I need DotNetCoreInstaller given the build agent is my machine it has 
to have the version I am using installed already.
I am going to remove it

