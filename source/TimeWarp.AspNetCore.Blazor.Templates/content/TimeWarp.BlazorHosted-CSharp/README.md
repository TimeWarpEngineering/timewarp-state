# Directory Structure
* Artifacts - Build outputs go here. Doing a build.cmd/build.sh generates artifacts here (nupkgs, dlls, pdbs, etc.)
* Assets - Image Logos etc.
* Build - Build customizations (custom msbuild files/psake/fake/albacore/etc) scripts
* Documentation - DocFx folder
* Lib - Things that can NEVER exist in a nuget package
* Packages - NuGet packages
* Samples (optional) - Sample projects
* Source - Main projects (the product code)
* Tests - Test projects
* .gitignore - started with VisualStudio one from Github update as needed.
* build.ps - (optional) Powershell script to Bootstrap the build
* global.json - (optional) specify required SDK
* Nuget.config - specify Nuget sources
* README.md
* UNLICENSE.md
