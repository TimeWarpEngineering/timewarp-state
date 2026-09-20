# Round 1 — general
**Date:** 2026-09-20
**Scope reviewed:** branch task/089-wire-globalusingsanalyzer-with-kebab-global-usings vs origin/master (product commit 4fcad2ec)

## Summary

Wires GlobalUsingsAnalyzer 1.4.0 repo-wide in the Ganda shape (CPM pin with PrivateAssets/IncludeAssets, Directory.Build.props Code Analyzers PackageReference PrivateAssets=all), points editorconfig at kebab `global-usings.cs` with PascalCase diagnostic keys that match the analyzer assembly’s embedded option names, and folds repeated compilation-unit usings into per-project `global-usings.cs` (Plus tests, assembly markers, Test.App mediator files, e2e AssemblyInfo, sample mediator scopes, scripts runfiles). Risk is low: `TreatWarningsAsErrors` remains false, truly one-file Plus-test usings stay file-local after file-scoped namespaces, and targeted builds plus a scripts runfile smoke show no GlobalUsingsAnalyzer warnings with the package restored into project assets.

## Issues

